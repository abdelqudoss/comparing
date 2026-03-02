using ExcelCompare.Application.Interfaces;
using ExcelCompare.Domain.Entities;
using ExcelCompare.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ExcelCompare.Infrastructure.Data;

public class UploadBatchRepository : IUploadBatchRepository
{
    private readonly ApplicationDbContext _context;

    public UploadBatchRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<UploadBatch?> GetByIdAsync(int id)
    {
        return await _context.UploadBatches.FindAsync(id);
    }

    public async Task<UploadBatch?> GetLatestByTypeAsync(string fileType)
    {
        return await _context.UploadBatches
            .Where(b => b.FileType == fileType)
            .OrderByDescending(b => b.UploadedAt)
            .FirstOrDefaultAsync();
    }

    public async Task<IEnumerable<UploadBatch>> GetAllAsync()
    {
        return await _context.UploadBatches
            .OrderByDescending(b => b.UploadedAt)
            .ToListAsync();
    }

    public async Task<UploadBatch> AddAsync(UploadBatch batch)
    {
        _context.UploadBatches.Add(batch);
        await _context.SaveChangesAsync();
        return batch;
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}
