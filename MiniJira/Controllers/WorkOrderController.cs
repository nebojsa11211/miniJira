using Microsoft.AspNetCore.Mvc;
using MiniJira.Services;

namespace MiniJira.Controllers;

[ApiController]
[Route("api/[controller]")]
public class WorkOrderController : ControllerBase
{
    private readonly IWorkOrderService _workOrderService;
    private readonly ILogger<WorkOrderController> _logger;

    public WorkOrderController(IWorkOrderService workOrderService, ILogger<WorkOrderController> logger)
    {
        _workOrderService = workOrderService;
        _logger = logger;
    }

    [HttpGet("{id}/export/excel")]
    public async Task<IActionResult> ExportToExcel(Guid id)
    {
        try
        {
            var result = await _workOrderService.ExportToExcelAsync(id);

            if (!result.IsSuccess)
            {
                return BadRequest(new { error = result.ErrorMessage });
            }

            var fileName = $"WorkOrder_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";
            return File(
                result.Value!,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                fileName
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error exporting work order {Id} to Excel", id);
            return StatusCode(500, new { error = "An error occurred while exporting to Excel" });
        }
    }

    [HttpGet("{id}/export/csv")]
    public async Task<IActionResult> ExportToCsv(Guid id)
    {
        try
        {
            var result = await _workOrderService.ExportToCsvAsync(id);

            if (!result.IsSuccess)
            {
                return BadRequest(new { error = result.ErrorMessage });
            }

            var fileName = $"WorkOrder_{DateTime.Now:yyyyMMdd_HHmmss}.csv";
            return File(
                System.Text.Encoding.UTF8.GetBytes(result.Value!),
                "text/csv",
                fileName
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error exporting work order {Id} to CSV", id);
            return StatusCode(500, new { error = "An error occurred while exporting to CSV" });
        }
    }

    [HttpPost("{id}/import/excel")]
    public async Task<IActionResult> ImportFromExcel(Guid id, IFormFile file)
    {
        try
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest(new { error = "No file uploaded" });
            }

            // Validate file extension
            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (extension != ".xlsx")
            {
                return BadRequest(new { error = "Only .xlsx files are supported" });
            }

            // Import the file
            using var stream = file.OpenReadStream();
            var result = await _workOrderService.ImportFromExcelAsync(id, stream);

            if (!result.IsSuccess)
            {
                return BadRequest(new { error = result.ErrorMessage });
            }

            return Ok(result.Value);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error importing work order {Id} from Excel", id);
            return StatusCode(500, new { error = "An error occurred while importing from Excel" });
        }
    }
}
