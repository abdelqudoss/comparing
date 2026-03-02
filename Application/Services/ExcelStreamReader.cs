using ExcelDataReader;
using System.Data;

namespace ExcelCompare.Application.Services;

public class ExcelStreamReader : IDisposable
{
    private readonly Stream _stream;
    private IExcelDataReader? _reader;

    public ExcelStreamReader(Stream stream)
    {
        _stream = stream;
    }

    public IEnumerable<Dictionary<string, object>> ReadRecords()
    {
        _reader = ExcelReaderFactory.CreateReader(_stream);
        
        var result = _reader.AsDataSet(new ExcelDataSetConfiguration
        {
            ConfigureDataTable = _ => new ExcelDataTableConfiguration
            {
                UseHeaderRow = true
            }
        });

        var table = result.Tables[0];
        
        foreach (DataRow row in table.Rows)
        {
            var record = new Dictionary<string, object>();
            foreach (DataColumn column in table.Columns)
            {
                record[column.ColumnName.ToLower().Trim()] = row[column] ?? string.Empty;
            }
            yield return record;
        }
    }

    public void Dispose()
    {
        _reader?.Dispose();
    }
}
