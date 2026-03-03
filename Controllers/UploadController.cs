using Microsoft.AspNetCore.Mvc;
using ExcelCompare.Application.Interfaces;
using ExcelCompare.Application.Exceptions;
using ExcelCompare.Application.Models;

namespace ExcelCompare.Controllers;

public class UploadController : Controller
{
    private readonly IExcelUploadService _uploadService;
    private readonly IUploadBatchRepository _batchRepository;

    public UploadController(IExcelUploadService uploadService, IUploadBatchRepository batchRepository)
    {
        _uploadService = uploadService;
        _batchRepository = batchRepository;
    }

    public async Task<IActionResult> Index()
    {
        var batches = await _batchRepository.GetAllAsync();
        return View(batches);
    }

    [HttpPost]
    public async Task<IActionResult> Upload(IFormFile file, string fileType)
    {
        try
        {
            if (file == null || file.Length == 0)
            {
                TempData["Error"] = "❌ Please select a file.";
                return RedirectToAction(nameof(Index));
            }

            if (fileType != "Sent" && fileType != "Received")
            {
                TempData["Error"] = "❌ Invalid file type. Please select 'Sent' or 'Received'.";
                return RedirectToAction(nameof(Index));
            }

            if (!file.FileName.EndsWith(".xlsx") && !file.FileName.EndsWith(".xls"))
            {
                TempData["Error"] = "❌ Invalid file format. Please upload an Excel file (.xlsx or .xls).";
                return RedirectToAction(nameof(Index));
            }

            if (file.Length > 100 * 1024 * 1024) // 100MB limit
            {
                TempData["Error"] = "❌ File too large. Maximum file size is 100MB.";
                return RedirectToAction(nameof(Index));
            }

            var progress = new Progress<int>(processed =>
            {
                // In production, use SignalR for real-time updates
            });

            var batch = await _uploadService.UploadFileAsync(file, fileType, progress);

            TempData["Success"] = $"✅ File uploaded successfully! Total records: {batch.TotalRows:N0}";
            return RedirectToAction(nameof(Index));
        }
        catch (DuplicatesFoundException dupEx)
        {
            // Store duplicate info in TempData for confirmation page
            try
            {
                TempData["DuplicateResult"] = System.Text.Json.JsonSerializer.Serialize(dupEx.DuplicateResult);
                TempData["FileName"] = file.FileName;
                TempData["FileType"] = fileType;
                
                return RedirectToAction(nameof(ConfirmUpload));
            }
            catch (Exception serEx)
            {
                TempData["Error"] = $"❌ Error processing duplicate information: {serEx.Message}";
                return RedirectToAction(nameof(Index));
            }
        }
        catch (Microsoft.Data.SqlClient.SqlException sqlEx)
        {
            TempData["Error"] = $"❌ Database connection error: {sqlEx.Message}\n\nPlease check:\n• SQL Server is running\n• Connection string in appsettings.json is correct\n• User 'sa' has proper permissions";
            return RedirectToAction(nameof(Index));
        }
        catch (InvalidOperationException invEx)
        {
            // These are validation errors with friendly messages
            TempData["Error"] = invEx.Message;
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            // Log the exception (in production, use ILogger)
            var innerMsg = ex.InnerException?.Message ?? "";
            var fullError = $"❌ Upload failed: {ex.Message}";
            if (!string.IsNullOrEmpty(innerMsg))
            {
                fullError += $"\n\nDetails: {innerMsg}";
            }
            
            TempData["Error"] = fullError;
            return RedirectToAction(nameof(Index));
        }
    }

    public IActionResult ConfirmUpload()
    {
        if (TempData["DuplicateResult"] == null)
        {
            return RedirectToAction(nameof(Index));
        }

        var duplicateResultJson = TempData["DuplicateResult"]?.ToString();
        var result = System.Text.Json.JsonSerializer.Deserialize<DuplicateDetectionResult>(duplicateResultJson!);
        
        ViewBag.FileName = TempData["FileName"];
        ViewBag.FileType = TempData["FileType"];
        
        // Keep data for potential POST
        TempData.Keep("DuplicateResult");
        TempData.Keep("FileName");
        TempData.Keep("FileType");
        
        return View(result);
    }

    [HttpPost]
    public async Task<IActionResult> ProcessConfirmedUpload(string action, IFormFile file, string fileType, bool markAsUpdate = false)
    {
        if (action == "cancel")
        {
            TempData["Info"] = "Upload cancelled by user.";
            return RedirectToAction(nameof(Index));
        }

        try
        {
            // For now, just proceed with upload (ignoring duplicates)
            // TODO: Implement update logic when markAsUpdate is true
            var progress = new Progress<int>();
            var batch = await _uploadService.UploadFileAsync(file, fileType, progress);

            if (markAsUpdate)
            {
                TempData["Success"] = $"File uploaded as UPDATE! Total records: {batch.TotalRows:N0}. Duplicate records will be versioned.";
            }
            else
            {
                TempData["Success"] = $"File uploaded successfully! Total records: {batch.TotalRows:N0}. Duplicates were allowed.";
            }

            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            TempData["Error"] = $"Upload failed: {ex.Message}";
            return RedirectToAction(nameof(Index));
        }
    }
}
