using ExcelCompare.Application.Interfaces;
using ExcelCompare.Domain.Entities;
using ExcelCompare.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ExcelCompare.Application.Services;

public class DataVersioningService : IDataVersioningService
{
    private readonly ApplicationDbContext _context;

    public DataVersioningService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<int> MarkAsUpdateAsync(int newBatchId, List<int> originalBatchIds)
    {
        var batch = await _context.UploadBatches.FindAsync(newBatchId);
        
        if (batch == null)
            throw new ArgumentException($"Batch {newBatchId} not found");

        batch.IsUpdate = true;
        
        // Link to the first original batch (primary)
        if (originalBatchIds.Any())
        {
            batch.OriginalBatchId = originalBatchIds.First();
        }

        await _context.SaveChangesAsync();
        
        return originalBatchIds.Count;
    }

    public async Task<List<UploadBatch>> GetRecordHistoryAsync(string mem, string nid, string fileType)
    {
        var batchIds = new List<int>();

        if (fileType == "Sent")
        {
            // Find all batches containing this Mem or Nid
            var query = _context.SentRecords.AsQueryable();
            
            if (!string.IsNullOrEmpty(mem))
            {
                var memBatches = await query
                    .Where(r => r.Mem == mem)
                    .Select(r => r.UploadBatchId)
                    .Distinct()
                    .ToListAsync();
                batchIds.AddRange(memBatches);
            }

            if (!string.IsNullOrEmpty(nid))
            {
                var nidBatches = await query
                    .Where(r => r.Nid == nid)
                    .Select(r => r.UploadBatchId)
                    .Distinct()
                    .ToListAsync();
                batchIds.AddRange(nidBatches);
            }
        }
        else
        {
            var query = _context.ReceivedRecords.AsQueryable();
            
            if (!string.IsNullOrEmpty(mem))
            {
                var memBatches = await query
                    .Where(r => r.Mem == mem)
                    .Select(r => r.UploadBatchId)
                    .Distinct()
                    .ToListAsync();
                batchIds.AddRange(memBatches);
            }

            if (!string.IsNullOrEmpty(nid))
            {
                var nidBatches = await query
                    .Where(r => r.Nid == nid)
                    .Select(r => r.UploadBatchId)
                    .Distinct()
                    .ToListAsync();
                batchIds.AddRange(nidBatches);
            }
        }

        var uniqueBatchIds = batchIds.Distinct().ToList();

        var batches = await _context.UploadBatches
            .Where(b => uniqueBatchIds.Contains(b.Id))
            .OrderByDescending(b => b.UploadedAt)
            .ToListAsync();

        return batches;
    }
}
