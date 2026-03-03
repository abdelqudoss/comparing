using ExcelCompare.Application.Models;

namespace ExcelCompare.Application.Exceptions;

public class DuplicatesFoundException : Exception
{
    public DuplicateDetectionResult DuplicateResult { get; }
    
    public DuplicatesFoundException(DuplicateDetectionResult result) 
        : base("Duplicates were detected in the uploaded file.")
    {
        DuplicateResult = result;
    }
    
    public DuplicatesFoundException(string message, DuplicateDetectionResult result) 
        : base(message)
    {
        DuplicateResult = result;
    }
}
