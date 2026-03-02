using Microsoft.AspNetCore.Mvc;
using ExcelCompare.Application.Interfaces;

namespace ExcelCompare.Controllers;

public class ComparisonController : Controller
{
    private readonly IComparisonService _comparisonService;
    private readonly IUploadBatchRepository _batchRepository;

    public ComparisonController(IComparisonService comparisonService, IUploadBatchRepository batchRepository)
    {
        _comparisonService = comparisonService;
        _batchRepository = batchRepository;
    }

    public async Task<IActionResult> Index()
    {
        var batches = await _batchRepository.GetAllAsync();
        return View(batches);
    }

    [HttpPost]
    public async Task<IActionResult> CompareLatest()
    {
        try
        {
            var result = await _comparisonService.CompareLatestBatchesAsync();
            return View("Results", result);
        }
        catch (Exception ex)
        {
            TempData["Error"] = $"Comparison failed: {ex.Message}";
            return RedirectToAction(nameof(Index));
        }
    }

    [HttpPost]
    public async Task<IActionResult> CompareSelected(int sentBatchId, int receivedBatchId)
    {
        try
        {
            var result = await _comparisonService.CompareBatchesAsync(sentBatchId, receivedBatchId);
            return View("Results", result);
        }
        catch (Exception ex)
        {
            TempData["Error"] = $"Comparison failed: {ex.Message}";
            return RedirectToAction(nameof(Index));
        }
    }

    public IActionResult Results(ComparisonResult result)
    {
        return View(result);
    }
}
