using ExcelCompare.Domain.Entities;

namespace ExcelCompare.Application.Interfaces;

public interface IDataVersioningService
{
    Task<int> MarkAsUpdateAsync(int newBatchId, List<int> originalBatchIds);
    Task<List<UploadBatch>> GetRecordHistoryAsync(string mem, string nid, string fileType);
}
