using ExcelCompare.Domain.Entities;
using Microsoft.AspNetCore.Http;

namespace ExcelCompare.Application.Interfaces;

public interface IExcelUploadService
{
    Task<UploadBatch> UploadFileAsync(IFormFile file, string fileType, IProgress<int>? progress = null);
}
