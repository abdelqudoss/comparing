using Microsoft.AspNetCore.Mvc;
using ExcelCompare.Application.Interfaces;

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
                TempData["Error"] = "Please select a file";
                return RedirectToAction(nameof(Index));
            }

            var progress = new Progress<int>(processed =>
            {
                // In production, use SignalR for real-time updates
            });

            var batch = await _uploadService.UploadFileAsync(file, fileType, progress);

            TempData["Success"] = $"File uploaded successfully. Total records: {batch.TotalRows}";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            TempData["Error"] = $"Upload failed: {ex.Message}";
            return RedirectToAction(nameof(Index));
        }
    }
}
