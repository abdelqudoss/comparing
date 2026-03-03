namespace ExcelCompare.Domain.Entities;

public class UploadDuplicate
{
    public int Id { get; set; }
    public int UploadBatchId { get; set; }
    public string DuplicateType { get; set; } = string.Empty; // WithinFile, AcrossBatches, BothMemAndNid, MemOnly, NidOnly
    public string MatchedField { get; set; } = string.Empty; // Mem or Nid
    public string MatchedValue { get; set; } = string.Empty; // The actual value that matched
    public string Mem { get; set; } = string.Empty;
    public string Nid { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public int? ConflictingBatchId { get; set; }
    public DateTime DetectedAt { get; set; }
    
    public UploadBatch UploadBatch { get; set; } = null!;
    public UploadBatch? ConflictingBatch { get; set; }
}
