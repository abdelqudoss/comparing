using ExcelCompare.Application.Interfaces;
using ExcelCompare.Domain.Entities;
using ExcelCompare.Infrastructure.Data;
using Microsoft.AspNetCore.Http;

namespace ExcelCompare.Application.Services;

public class ExcelUploadService : IExcelUploadService
{
    private readonly ApplicationDbContext _context;
    private readonly IUploadBatchRepository _batchRepository;
    private readonly BulkInsertService _bulkInsertService;

    public ExcelUploadService(
        ApplicationDbContext context,
        IUploadBatchRepository batchRepository,
        BulkInsertService bulkInsertService)
    {
        _context = context;
        _batchRepository = batchRepository;
        _bulkInsertService = bulkInsertService;
    }

    public async Task<UploadBatch> UploadFileAsync(IFormFile file, string fileType, IProgress<int>? progress = null)
    {
        if (file == null || file.Length == 0)
            throw new ArgumentException("File is empty");

        if (fileType != "Sent" && fileType != "Received")
            throw new ArgumentException("FileType must be 'Sent' or 'Received'");

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
        batch.TotalRows = records.Count;

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

        await _batchRepository.SaveChangesAsync();

        return batch;
    }
}
