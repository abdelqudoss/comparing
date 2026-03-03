namespace ExcelCompare.Application.Models;

public class DuplicateRecord
{
    public string Mem { get; set; } = string.Empty;
    public string Nid { get; set; } = string.Empty;
    public string Sn { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string DuplicateReason { get; set; } = string.Empty;
    public int? ExistingBatchId { get; set; }
    public string? ExistingBatchFileName { get; set; }
    public DateTime? ExistingBatchDate { get; set; }
    public int OccurrenceCount { get; set; }
    
    // Backward compatibility
    public int Count => OccurrenceCount;
    public List<int> ConflictingBatchIds => ExistingBatchId.HasValue ? new List<int> { ExistingBatchId.Value } : new();
}
