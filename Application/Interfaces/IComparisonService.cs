using ExcelCompare.Domain.Entities;

namespace ExcelCompare.Application.Interfaces;

public interface IComparisonService
{
    Task<ComparisonResult> CompareLatestBatchesAsync();
    Task<ComparisonResult> CompareBatchesAsync(int sentBatchId, int receivedBatchId);
}

public class ComparisonResult
{
    public int SentBatchId { get; set; }
    public int ReceivedBatchId { get; set; }
    public List<ComparisonRecord> NewRecords { get; set; } = new();
    public List<ComparisonRecord> MissingRecords { get; set; } = new();
    public List<ComparisonRecord> ChangedRecords { get; set; } = new();
    public int UnchangedCount { get; set; }
}

public class ComparisonRecord
{
    public string Mem { get; set; } = string.Empty;
    public string Sn { get; set; } = string.Empty;
    public string Nid { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public DateTime RegDate { get; set; }
    public string BatchNo { get; set; } = string.Empty;
    public string MemberRank { get; set; } = string.Empty;
    public string RefSn { get; set; } = string.Empty;
    public string? ChangeType { get; set; }
    public string? OldMemberRank { get; set; }
    public string? OldRefSn { get; set; }
}
