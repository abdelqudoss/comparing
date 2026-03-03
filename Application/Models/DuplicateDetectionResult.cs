namespace ExcelCompare.Application.Models;

public class DuplicateDetectionResult
{
    public List<DuplicateRecord> WithinFileMemDuplicates { get; set; } = new();
    public List<DuplicateRecord> WithinFileNidDuplicates { get; set; } = new();
    public List<DuplicateRecord> CrossBatchMemDuplicates { get; set; } = new();
    public List<DuplicateRecord> CrossBatchNidDuplicates { get; set; } = new();
    
    // Combined lists for easier display
    public List<DuplicateRecord> DuplicatesByMem => WithinFileMemDuplicates.Concat(CrossBatchMemDuplicates).ToList();
    public List<DuplicateRecord> DuplicatesByNid => WithinFileNidDuplicates.Concat(CrossBatchNidDuplicates).ToList();
    
    public int WithinFileDuplicateCount => WithinFileMemDuplicates.Count + WithinFileNidDuplicates.Count;
    public int CrossBatchDuplicateCount => CrossBatchMemDuplicates.Count + CrossBatchNidDuplicates.Count;
    public int TotalDuplicateCount => WithinFileDuplicateCount + CrossBatchDuplicateCount;
    
    public int TotalWithinFileDuplicates => WithinFileMemDuplicates.Count + WithinFileNidDuplicates.Count;
    public int TotalCrossBatchDuplicates => CrossBatchMemDuplicates.Count + CrossBatchNidDuplicates.Count;
    public int TotalDuplicates => TotalWithinFileDuplicates + TotalCrossBatchDuplicates;
    
    public bool HasDuplicates => TotalDuplicates > 0;
    
    public string Recommendation
    {
        get
        {
            if (!HasDuplicates) return "No duplicates found. Safe to proceed.";
            if (TotalCrossBatchDuplicates > 0) return "Duplicates found with existing data. Consider marking as update.";
            return "Duplicates found within file. Review before proceeding.";
        }
    }
    
    public string RecommendedAction => Recommendation;
    
    public Dictionary<string, int> GetStatistics()
    {
        return new Dictionary<string, int>
        {
            ["WithinFile_Mem"] = WithinFileMemDuplicates.Count,
            ["WithinFile_Nid"] = WithinFileNidDuplicates.Count,
            ["CrossBatch_Mem"] = CrossBatchMemDuplicates.Count,
            ["CrossBatch_Nid"] = CrossBatchNidDuplicates.Count,
            ["Total"] = TotalDuplicates
        };
    }
}
