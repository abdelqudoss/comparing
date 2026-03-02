using ExcelCompare.Domain.Entities;

namespace ExcelCompare.Application.Interfaces;

public interface IUploadBatchRepository
{
    Task<UploadBatch?> GetByIdAsync(int id);
    Task<UploadBatch?> GetLatestByTypeAsync(string fileType);
    Task<IEnumerable<UploadBatch>> GetAllAsync();
    Task<UploadBatch> AddAsync(UploadBatch batch);
    Task SaveChangesAsync();
}
