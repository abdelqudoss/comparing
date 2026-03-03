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

    public List<string> GetColumnNames()
    {
        _reader = ExcelReaderFactory.CreateReader(_stream);
        
        var result = _reader.AsDataSet(new ExcelDataSetConfiguration
        {
            ConfigureDataTable = _ => new ExcelDataTableConfiguration
            {
                UseHeaderRow = true
            }
        });

        if (result.Tables.Count == 0)
            return new List<string>();

        var table = result.Tables[0];
        return table.Columns.Cast<DataColumn>()
            .Select(c => c.ColumnName.ToLower().Trim())
            .ToList();
    }

    public IEnumerable<Dictionary<string, object>> ReadRecords()
    {
        if (_reader == null)
        {
            _reader = ExcelReaderFactory.CreateReader(_stream);
        }
        else
        {
            // Reset stream if reader already exists
            _stream.Position = 0;
            _reader.Dispose();
            _reader = ExcelReaderFactory.CreateReader(_stream);
        }
        
        var result = _reader.AsDataSet(new ExcelDataSetConfiguration
        {
            ConfigureDataTable = _ => new ExcelDataTableConfiguration
            {
                UseHeaderRow = true
            }
        });

        if (result.Tables.Count == 0)
            yield break;

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
