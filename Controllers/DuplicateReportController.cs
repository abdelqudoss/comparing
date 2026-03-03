using Microsoft.AspNetCore.Mvc;
using ExcelCompare.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ExcelCompare.Controllers;

public class DuplicateReportController : Controller
{
    private readonly ApplicationDbContext _context;

    public DuplicateReportController(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index(string? filterType = null)
    {
        var query = _context.UploadDuplicates
            .Include(d => d.UploadBatch)
            .Include(d => d.ConflictingBatch)
            .AsQueryable();

        if (!string.IsNullOrEmpty(filterType))
        {
            query = filterType.ToLower() switch
            {
                "withinfile" => query.Where(d => d.ConflictingBatchId == null),
                "crossbatch" => query.Where(d => d.ConflictingBatchId != null),
                _ => query
            };
        }

        var duplicates = await query
            .OrderByDescending(d => d.DetectedAt)
            .Take(500)
            .ToListAsync();

        ViewBag.FilterType = filterType;
        ViewBag.TotalDuplicates = await _context.UploadDuplicates.CountAsync();
        ViewBag.WithinFileCount = await _context.UploadDuplicates.CountAsync(d => d.ConflictingBatchId == null);
        ViewBag.CrossBatchCount = await _context.UploadDuplicates.CountAsync(d => d.ConflictingBatchId != null);

        return View(duplicates);
    }
}
