using ExcelCompare.Application.Interfaces;
using ExcelCompare.Application.Models;
using ExcelCompare.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ExcelCompare.Application.Services;

public class DuplicateDetectionService : IDuplicateDetectionService
{
    private readonly ApplicationDbContext _context;

    public DuplicateDetectionService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<DuplicateDetectionResult> DetectDuplicatesAsync(
        IEnumerable<Dictionary<string, object>> records, 
        string fileType)
    {
        var result = new DuplicateDetectionResult();
        
        var recordsList = records.ToList();
        
        // Detect within-file duplicates
        var withinFileDuplicates = await DetectWithinFileAsync(recordsList);
        foreach (var dup in withinFileDuplicates)
        {
            if (dup.DuplicateReason == "Mem")
                result.WithinFileMemDuplicates.Add(dup);
            else if (dup.DuplicateReason == "Nid")
                result.WithinFileNidDuplicates.Add(dup);
        }
        
        // Detect cross-batch duplicates
        var crossBatchDuplicates = await DetectAcrossBatchesAsync(recordsList, fileType);
        foreach (var dup in crossBatchDuplicates)
        {
            if (dup.DuplicateReason == "Mem")
                result.CrossBatchMemDuplicates.Add(dup);
            else if (dup.DuplicateReason == "Nid")
                result.CrossBatchNidDuplicates.Add(dup);
        }
        
        return result;
    }

    public async Task<List<DuplicateRecord>> DetectWithinFileAsync(
        IEnumerable<Dictionary<string, object>> records)
    {
        var duplicates = new List<DuplicateRecord>();
        var recordsList = records.ToList();
        
        // Group by Mem to find duplicates
        var memGroups = recordsList
            .GroupBy(r => GetStringValue(r, "mem"))
            .Where(g => g.Count() > 1 && !string.IsNullOrWhiteSpace(g.Key));
        
        foreach (var group in memGroups)
        {
            var first = group.First();
            duplicates.Add(new DuplicateRecord
            {
                Mem = group.Key,
                Nid = GetStringValue(first, "nid"),
                Sn = GetStringValue(first, "sn"),
                FullName = GetStringValue(first, "fullname"),
                Phone = GetStringValue(first, "phone"),
                DuplicateReason = "Mem",
                OccurrenceCount = group.Count()
            });
        }
        
        // Group by Nid to find duplicates
        var nidGroups = recordsList
            .GroupBy(r => GetStringValue(r, "nid"))
            .Where(g => g.Count() > 1 && !string.IsNullOrWhiteSpace(g.Key));
        
        foreach (var group in nidGroups)
        {
            var first = group.First();
            duplicates.Add(new DuplicateRecord
            {
                Mem = GetStringValue(first, "mem"),
                Nid = group.Key,
                Sn = GetStringValue(first, "sn"),
                FullName = GetStringValue(first, "fullname"),
                Phone = GetStringValue(first, "phone"),
                DuplicateReason = "Nid",
                OccurrenceCount = group.Count()
            });
        }
        
        return duplicates;
    }

    public async Task<List<DuplicateRecord>> DetectAcrossBatchesAsync(
        IEnumerable<Dictionary<string, object>> records, 
        string fileType)
    {
        var duplicates = new List<DuplicateRecord>();
        var recordsList = records.ToList();
        
        // Get all Mem values from new records
        var memValues = recordsList
            .Select(r => GetStringValue(r, "mem"))
            .Where(m => !string.IsNullOrWhiteSpace(m))
            .Distinct()
            .ToList();
        
        // Get all Nid values from new records
        var nidValues = recordsList
            .Select(r => GetStringValue(r, "nid"))
            .Where(n => !string.IsNullOrWhiteSpace(n))
            .Distinct()
            .ToList();
        
        // Check for Mem duplicates in existing data
        if (fileType == "Sent")
        {
            var existingMemRecords = await _context.SentRecords
                .Include(r => r.UploadBatch)
                .Where(r => memValues.Contains(r.Mem))
                .Select(r => new { r.Mem, r.Nid, r.FullName, r.Phone, r.UploadBatchId, r.UploadBatch.FileName, r.UploadBatch.UploadedAt })
                .ToListAsync();
            
            foreach (var existing in existingMemRecords)
            {
                duplicates.Add(new DuplicateRecord
                {
                    Mem = existing.Mem,
                    Nid = existing.Nid,
                    FullName = existing.FullName,
                    Phone = existing.Phone,
                    DuplicateReason = "Mem",
                    ExistingBatchId = existing.UploadBatchId,
                    ExistingBatchFileName = existing.FileName,
                    ExistingBatchDate = existing.UploadedAt,
                    OccurrenceCount = 1
                });
            }
            
            // Check for Nid duplicates in existing data
            var existingNidRecords = await _context.SentRecords
                .Include(r => r.UploadBatch)
                .Where(r => nidValues.Contains(r.Nid))
                .Select(r => new { r.Mem, r.Nid, r.FullName, r.Phone, r.UploadBatchId, r.UploadBatch.FileName, r.UploadBatch.UploadedAt })
                .ToListAsync();
            
            foreach (var existing in existingNidRecords)
            {
                // Avoid adding duplicate if already added by Mem check
                if (!duplicates.Any(d => d.Mem == existing.Mem && d.Nid == existing.Nid && d.DuplicateReason == "Mem"))
                {
                    duplicates.Add(new DuplicateRecord
                    {
                        Mem = existing.Mem,
                        Nid = existing.Nid,
                        FullName = existing.FullName,
                        Phone = existing.Phone,
                        DuplicateReason = "Nid",
                        ExistingBatchId = existing.UploadBatchId,
                        ExistingBatchFileName = existing.FileName,
                        ExistingBatchDate = existing.UploadedAt,
                        OccurrenceCount = 1
                    });
                }
            }
        }
        else // Received
        {
            var existingMemRecords = await _context.ReceivedRecords
                .Include(r => r.UploadBatch)
                .Where(r => memValues.Contains(r.Mem))
                .Select(r => new { r.Mem, r.Nid, r.FullName, r.Phone, r.UploadBatchId, r.UploadBatch.FileName, r.UploadBatch.UploadedAt })
                .ToListAsync();
            
            foreach (var existing in existingMemRecords)
            {
                duplicates.Add(new DuplicateRecord
                {
                    Mem = existing.Mem,
                    Nid = existing.Nid,
                    FullName = existing.FullName,
                    Phone = existing.Phone,
                    DuplicateReason = "Mem",
                    ExistingBatchId = existing.UploadBatchId,
                    ExistingBatchFileName = existing.FileName,
                    ExistingBatchDate = existing.UploadedAt,
                    OccurrenceCount = 1
                });
            }
            
            // Check for Nid duplicates in existing data
            var existingNidRecords = await _context.ReceivedRecords
                .Include(r => r.UploadBatch)
                .Where(r => nidValues.Contains(r.Nid))
                .Select(r => new { r.Mem, r.Nid, r.FullName, r.Phone, r.UploadBatchId, r.UploadBatch.FileName, r.UploadBatch.UploadedAt })
                .ToListAsync();
            
            foreach (var existing in existingNidRecords)
            {
                // Avoid adding duplicate if already added by Mem check
                if (!duplicates.Any(d => d.Mem == existing.Mem && d.Nid == existing.Nid && d.DuplicateReason == "Mem"))
                {
                    duplicates.Add(new DuplicateRecord
                    {
                        Mem = existing.Mem,
                        Nid = existing.Nid,
                        FullName = existing.FullName,
                        Phone = existing.Phone,
                        DuplicateReason = "Nid",
                        ExistingBatchId = existing.UploadBatchId,
                        ExistingBatchFileName = existing.FileName,
                        ExistingBatchDate = existing.UploadedAt,
                        OccurrenceCount = 1
                    });
                }
            }
        }
        
        return duplicates;
    }
    
    private string GetStringValue(Dictionary<string, object> record, string key)
    {
        if (record.TryGetValue(key, out var value) && value != null && value != DBNull.Value)
        {
            return value.ToString()?.Trim() ?? string.Empty;
        }
        return string.Empty;
    }
}
