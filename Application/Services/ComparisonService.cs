using ExcelCompare.Application.Interfaces;
using ExcelCompare.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Data.SqlClient;

namespace ExcelCompare.Application.Services;

public class ComparisonService : IComparisonService
{
    private readonly ApplicationDbContext _context;
    private readonly IUploadBatchRepository _batchRepository;

    public ComparisonService(ApplicationDbContext context, IUploadBatchRepository batchRepository)
    {
        _context = context;
        _batchRepository = batchRepository;
    }

    public async Task<ComparisonResult> CompareLatestBatchesAsync()
    {
        var latestSent = await _batchRepository.GetLatestByTypeAsync("Sent");
        var latestReceived = await _batchRepository.GetLatestByTypeAsync("Received");

        if (latestSent == null || latestReceived == null)
            throw new InvalidOperationException("No batches found for comparison");

        return await CompareBatchesAsync(latestSent.Id, latestReceived.Id);
    }

    public async Task<ComparisonResult> CompareBatchesAsync(int sentBatchId, int receivedBatchId)
    {
        var result = new ComparisonResult
        {
            SentBatchId = sentBatchId,
            ReceivedBatchId = receivedBatchId
        };

        var connectionString = _context.Database.GetDbConnection().ConnectionString;
        using var connection = new SqlConnection(connectionString);
        await connection.OpenAsync();

        // Find NEW records (in Received but not in Sent)
        result.NewRecords = await GetNewRecordsAsync(connection, sentBatchId, receivedBatchId);

        // Find MISSING records (in Sent but not in Received)
        result.MissingRecords = await GetMissingRecordsAsync(connection, sentBatchId, receivedBatchId);

        // Find CHANGED records (member_rank or ref_sn changed)
        result.ChangedRecords = await GetChangedRecordsAsync(connection, sentBatchId, receivedBatchId);

        // Count UNCHANGED records
        result.UnchangedCount = await GetUnchangedCountAsync(connection, sentBatchId, receivedBatchId);

        return result;
    }

    private async Task<List<ComparisonRecord>> GetNewRecordsAsync(
        SqlConnection connection, int sentBatchId, int receivedBatchId)
    {
        var sql = @"
            SELECT 
                r.Mem, r.Sn, r.Nid, r.Phone, r.FullName, 
                r.RegDate, r.BatchNo, r.MemberRank, r.RefSn
            FROM ReceivedRecords r
            LEFT JOIN SentRecords s ON r.Mem = s.Mem AND r.Nid = s.Nid AND s.UploadBatchId = @SentBatchId
            WHERE r.UploadBatchId = @ReceivedBatchId
            AND s.Id IS NULL";

        using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@SentBatchId", sentBatchId);
        command.Parameters.AddWithValue("@ReceivedBatchId", receivedBatchId);

        var records = new List<ComparisonRecord>();
        using var reader = await command.ExecuteReaderAsync();
        
        while (await reader.ReadAsync())
        {
            records.Add(new ComparisonRecord
            {
                Mem = reader.GetString(0),
                Sn = reader.GetString(1),
                Nid = reader.GetString(2),
                Phone = reader.GetString(3),
                FullName = reader.GetString(4),
                RegDate = reader.GetDateTime(5),
                BatchNo = reader.GetString(6),
                MemberRank = reader.GetString(7),
                RefSn = reader.GetString(8),
                ChangeType = "New"
            });
        }

        return records;
    }

    private async Task<List<ComparisonRecord>> GetMissingRecordsAsync(
        SqlConnection connection, int sentBatchId, int receivedBatchId)
    {
        var sql = @"
            SELECT 
                s.Mem, s.Sn, s.Nid, s.Phone, s.FullName, 
                s.RegDate, s.BatchNo, s.MemberRank, s.RefSn
            FROM SentRecords s
            LEFT JOIN ReceivedRecords r ON s.Mem = r.Mem AND s.Nid = r.Nid AND r.UploadBatchId = @ReceivedBatchId
            WHERE s.UploadBatchId = @SentBatchId
            AND r.Id IS NULL";

        using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@SentBatchId", sentBatchId);
        command.Parameters.AddWithValue("@ReceivedBatchId", receivedBatchId);

        var records = new List<ComparisonRecord>();
        using var reader = await command.ExecuteReaderAsync();
        
        while (await reader.ReadAsync())
        {
            records.Add(new ComparisonRecord
            {
                Mem = reader.GetString(0),
                Sn = reader.GetString(1),
                Nid = reader.GetString(2),
                Phone = reader.GetString(3),
                FullName = reader.GetString(4),
                RegDate = reader.GetDateTime(5),
                BatchNo = reader.GetString(6),
                MemberRank = reader.GetString(7),
                RefSn = reader.GetString(8),
                ChangeType = "Missing"
            });
        }

        return records;
    }

    private async Task<List<ComparisonRecord>> GetChangedRecordsAsync(
        SqlConnection connection, int sentBatchId, int receivedBatchId)
    {
        var sql = @"
            SELECT 
                r.Mem, r.Sn, r.Nid, r.Phone, r.FullName, 
                r.RegDate, r.BatchNo, r.MemberRank, r.RefSn,
                s.MemberRank AS OldMemberRank, s.RefSn AS OldRefSn
            FROM ReceivedRecords r
            INNER JOIN SentRecords s ON r.Mem = s.Mem AND r.Nid = s.Nid
            WHERE r.UploadBatchId = @ReceivedBatchId
            AND s.UploadBatchId = @SentBatchId
            AND (r.MemberRank != s.MemberRank OR r.RefSn != s.RefSn)";

        using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@SentBatchId", sentBatchId);
        command.Parameters.AddWithValue("@ReceivedBatchId", receivedBatchId);

        var records = new List<ComparisonRecord>();
        using var reader = await command.ExecuteReaderAsync();
        
        while (await reader.ReadAsync())
        {
            records.Add(new ComparisonRecord
            {
                Mem = reader.GetString(0),
                Sn = reader.GetString(1),
                Nid = reader.GetString(2),
                Phone = reader.GetString(3),
                FullName = reader.GetString(4),
                RegDate = reader.GetDateTime(5),
                BatchNo = reader.GetString(6),
                MemberRank = reader.GetString(7),
                RefSn = reader.GetString(8),
                OldMemberRank = reader.GetString(9),
                OldRefSn = reader.GetString(10),
                ChangeType = "Changed"
            });
        }

        return records;
    }

    private async Task<int> GetUnchangedCountAsync(
        SqlConnection connection, int sentBatchId, int receivedBatchId)
    {
        var sql = @"
            SELECT COUNT(*)
            FROM ReceivedRecords r
            INNER JOIN SentRecords s ON r.Mem = s.Mem AND r.Nid = s.Nid
            WHERE r.UploadBatchId = @ReceivedBatchId
            AND s.UploadBatchId = @SentBatchId
            AND r.MemberRank = s.MemberRank 
            AND r.RefSn = s.RefSn";

        using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@SentBatchId", sentBatchId);
        command.Parameters.AddWithValue("@ReceivedBatchId", receivedBatchId);

        var count = await command.ExecuteScalarAsync();
        return Convert.ToInt32(count);
    }
}
