using ExcelCompare.Application.Models;

namespace ExcelCompare.Application.Interfaces;

public interface IDuplicateDetectionService
{
    Task<DuplicateDetectionResult> DetectDuplicatesAsync(
        IEnumerable<Dictionary<string, object>> records, 
        string fileType);
    
    Task<List<DuplicateRecord>> DetectWithinFileAsync(
        IEnumerable<Dictionary<string, object>> records);
    
    Task<List<DuplicateRecord>> DetectAcrossBatchesAsync(
        IEnumerable<Dictionary<string, object>> records, 
        string fileType);
}
