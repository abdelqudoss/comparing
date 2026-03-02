namespace ExcelCompare.Domain.Entities;

public class ReceivedRecord
{
    public int Id { get; set; }
    public int UploadBatchId { get; set; }
    public string Mem { get; set; } = string.Empty;
    public string Sn { get; set; } = string.Empty;
    public string Nid { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public DateTime RegDate { get; set; }
    public string BatchNo { get; set; } = string.Empty;
    public string MemberRank { get; set; } = string.Empty;
    public string RefSn { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    
    public UploadBatch UploadBatch { get; set; } = null!;
}
