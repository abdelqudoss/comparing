namespace ExcelCompare.Domain.Entities;

public class UploadBatch
{
    public int Id { get; set; }
    public string FileType { get; set; } = string.Empty; // "Sent" or "Received"
    public string FileName { get; set; } = string.Empty;
    public DateTime UploadedAt { get; set; }
    public int TotalRows { get; set; }
    public bool IsUpdate { get; set; }
    public int? OriginalBatchId { get; set; }
    
    public UploadBatch? OriginalBatch { get; set; }
    public ICollection<SentRecord> SentRecords { get; set; } = new List<SentRecord>();
    public ICollection<ReceivedRecord> ReceivedRecords { get; set; } = new List<ReceivedRecord>();
}
