using ExcelCompare.Application.Interfaces;
using ExcelCompare.Application.Exceptions;
using ExcelCompare.Application.Models;
using ExcelCompare.Domain.Entities;
using ExcelCompare.Infrastructure.Data;
using Microsoft.AspNetCore.Http;

namespace ExcelCompare.Application.Services;

public class ExcelUploadService : IExcelUploadService
{
    private readonly ApplicationDbContext _context;
    private readonly IUploadBatchRepository _batchRepository;
    private readonly BulkInsertService _bulkInsertService;
    private readonly IDuplicateDetectionService _duplicateDetectionService;

    public ExcelUploadService(
        ApplicationDbContext context,
        IUploadBatchRepository batchRepository,
        BulkInsertService bulkInsertService,
        IDuplicateDetectionService duplicateDetectionService)
    {
        _context = context;
        _batchRepository = batchRepository;
        _bulkInsertService = bulkInsertService;
        _duplicateDetectionService = duplicateDetectionService;
    }

    public async Task<UploadBatch> UploadFileAsync(IFormFile file, string fileType, IProgress<int>? progress = null)
    {
        if (file == null || file.Length == 0)
            throw new ArgumentException("File is empty");

        if (fileType != "Sent" && fileType != "Received")
            throw new ArgumentException("FileType must be 'Sent' or 'Received'");

        // Validate Excel structure before creating batch
        using var validationStream = file.OpenReadStream();
        using var validationReader = new ExcelStreamReader(validationStream);
        
        var columnNames = validationReader.GetColumnNames();
        
        if (columnNames.Count == 0)
        {
            throw new InvalidOperationException("❌ Excel file has no columns. Please check if the file is empty or corrupted.");
        }

        var requiredColumns = new[] { "mem", "sn", "nid", "phone", "fullname", "reg_date", "batch_no", "member_rank", "ref_sn" };
        var missingColumns = requiredColumns.Where(c => !columnNames.Contains(c)).ToList();

        if (missingColumns.Any())
        {
            throw new InvalidOperationException(
                $"❌ Excel file is missing required columns: {string.Join(", ", missingColumns)}. " +
                $"\n\n📋 Found columns: {string.Join(", ", columnNames)}. " +
                $"\n\n✅ Required columns: {string.Join(", ", requiredColumns)}");
        }

        var batch = new UploadBatch
        {
            FileType = fileType,
            FileName = file.FileName,
            UploadedAt = DateTime.UtcNow,
            TotalRows = 0
        };

        batch = await _batchRepository.AddAsync(batch);

        using var stream = file.OpenReadStream();
        using var reader = new ExcelStreamReader(stream);
        
        var records = reader.ReadRecords().ToList();
        
        if (records.Count == 0)
        {
            throw new InvalidOperationException("❌ No data rows found in Excel file. The file has headers but no data. Please add at least one row of data.");
        }

        batch.TotalRows = records.Count;

        // Detect duplicates before inserting
        var duplicateResult = await _duplicateDetectionService.DetectDuplicatesAsync(
            records, 
            fileType);

        if (duplicateResult.HasDuplicates)
        {
            // Delete the batch since we're not proceeding
            _context.UploadBatches.Remove(batch);
            await _context.SaveChangesAsync();
            
            throw new DuplicatesFoundException(duplicateResult);
        }

        try
        {
            if (fileType == "Sent")
            {
                await _bulkInsertService.BulkInsertSentRecordsAsync(
                    batch.Id,
                    records,
                    progress);
            }
            else
            {
                await _bulkInsertService.BulkInsertReceivedRecordsAsync(
                    batch.Id,
                    records,
                    progress);
            }

            // Update the batch with the total rows count
            await _batchRepository.SaveChangesAsync();

            return batch;
        }
        catch (Exception ex)
        {
            // If bulk insert fails, provide more context
            throw new InvalidOperationException(
                $"❌ Failed to insert records into database: {ex.Message}. " +
                $"Please check that all data values are valid.", ex);
        }
    }
}
