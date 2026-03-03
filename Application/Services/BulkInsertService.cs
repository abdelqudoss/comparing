using System.Data;
using Microsoft.Data.SqlClient;
using ExcelCompare.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace ExcelCompare.Application.Services;

public class BulkInsertService
{
    private readonly ApplicationDbContext _context;
    private readonly IConfiguration _configuration;
    private const int BatchSize = 10000;

    public BulkInsertService(ApplicationDbContext context, IConfiguration configuration)
    {
        _context = context;
        _configuration = configuration;
    }

    public async Task BulkInsertSentRecordsAsync(
        int uploadBatchId,
        IEnumerable<Dictionary<string, object>> records,
        IProgress<int>? progress = null,
        bool isUpdate = false)
    {
        var dataTable = CreateSentRecordDataTable();
        int processedCount = 0;
        int batchCount = 0;

        foreach (var record in records)
        {
            var row = dataTable.NewRow();
            row["UploadBatchId"] = uploadBatchId;
            row["Mem"] = GetStringValue(record, "mem");
            row["Sn"] = GetStringValue(record, "sn");
            row["Nid"] = GetStringValue(record, "nid");
            row["Phone"] = GetStringValue(record, "phone");
            row["FullName"] = GetStringValue(record, "fullname");
            row["RegDate"] = GetDateValue(record, "reg_date");
            row["BatchNo"] = GetStringValue(record, "batch_no");
            row["MemberRank"] = GetStringValue(record, "member_rank");
            row["RefSn"] = GetStringValue(record, "ref_sn");
            row["CreatedAt"] = DateTime.UtcNow;

            dataTable.Rows.Add(row);
            processedCount++;
            batchCount++;

            if (batchCount >= BatchSize)
            {
                await ExecuteBulkCopyAsync(dataTable, "SentRecords");
                dataTable.Clear();
                batchCount = 0;
                progress?.Report(processedCount);
            }
        }

        if (dataTable.Rows.Count > 0)
        {
            await ExecuteBulkCopyAsync(dataTable, "SentRecords");
            progress?.Report(processedCount);
        }
    }

    public async Task BulkInsertReceivedRecordsAsync(
        int uploadBatchId,
        IEnumerable<Dictionary<string, object>> records,
        IProgress<int>? progress = null,
        bool isUpdate = false)
    {
        var dataTable = CreateReceivedRecordDataTable();
        int processedCount = 0;
        int batchCount = 0;

        foreach (var record in records)
        {
            var row = dataTable.NewRow();
            row["UploadBatchId"] = uploadBatchId;
            row["Mem"] = GetStringValue(record, "mem");
            row["Sn"] = GetStringValue(record, "sn");
            row["Nid"] = GetStringValue(record, "nid");
            row["Phone"] = GetStringValue(record, "phone");
            row["FullName"] = GetStringValue(record, "fullname");
            row["RegDate"] = GetDateValue(record, "reg_date");
            row["BatchNo"] = GetStringValue(record, "batch_no");
            row["MemberRank"] = GetStringValue(record, "member_rank");
            row["RefSn"] = GetStringValue(record, "ref_sn");
            row["CreatedAt"] = DateTime.UtcNow;

            dataTable.Rows.Add(row);
            processedCount++;
            batchCount++;

            if (batchCount >= BatchSize)
            {
                await ExecuteBulkCopyAsync(dataTable, "ReceivedRecords");
                dataTable.Clear();
                batchCount = 0;
                progress?.Report(processedCount);
            }
        }

        if (dataTable.Rows.Count > 0)
        {
            await ExecuteBulkCopyAsync(dataTable, "ReceivedRecords");
            progress?.Report(processedCount);
        }
    }

    private async Task ExecuteBulkCopyAsync(DataTable dataTable, string tableName)
    {
        var connectionString = _configuration.GetConnectionString("DefaultConnection");
        
        using var connection = new SqlConnection(connectionString);
        await connection.OpenAsync();

        using var bulkCopy = new SqlBulkCopy(connection)
        {
            DestinationTableName = tableName,
            BatchSize = BatchSize,
            BulkCopyTimeout = 300
        };

        foreach (DataColumn column in dataTable.Columns)
        {
            bulkCopy.ColumnMappings.Add(column.ColumnName, column.ColumnName);
        }

        await bulkCopy.WriteToServerAsync(dataTable);
    }

    private DataTable CreateSentRecordDataTable()
    {
        var table = new DataTable();
        table.Columns.Add("UploadBatchId", typeof(int));
        table.Columns.Add("Mem", typeof(string));
        table.Columns.Add("Sn", typeof(string));
        table.Columns.Add("Nid", typeof(string));
        table.Columns.Add("Phone", typeof(string));
        table.Columns.Add("FullName", typeof(string));
        table.Columns.Add("RegDate", typeof(DateTime));
        table.Columns.Add("BatchNo", typeof(string));
        table.Columns.Add("MemberRank", typeof(string));
        table.Columns.Add("RefSn", typeof(string));
        table.Columns.Add("CreatedAt", typeof(DateTime));
        return table;
    }

    private DataTable CreateReceivedRecordDataTable()
    {
        return CreateSentRecordDataTable();
    }

    private string GetStringValue(Dictionary<string, object> record, string key)
    {
        if (record.TryGetValue(key, out var value) && value != null && value != DBNull.Value)
        {
            return value.ToString()?.Trim() ?? string.Empty;
        }
        return string.Empty;
    }

    private DateTime GetDateValue(Dictionary<string, object> record, string key)
    {
        if (record.TryGetValue(key, out var value) && value != null && value != DBNull.Value)
        {
            if (value is DateTime dt)
                return dt;
            
            if (DateTime.TryParse(value.ToString(), out var parsed))
                return parsed;
        }
        return DateTime.MinValue;
    }
}
