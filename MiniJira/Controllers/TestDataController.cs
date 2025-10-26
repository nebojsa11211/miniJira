using Microsoft.AspNetCore.Mvc;
using MiniJira.Services;

namespace MiniJira.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TestDataController : ControllerBase
{
    private readonly IDataResetService _dataResetService;
    private readonly ILogger<TestDataController> _logger;

    public TestDataController(IDataResetService dataResetService, ILogger<TestDataController> logger)
    {
        _dataResetService = dataResetService;
        _logger = logger;
    }

    [HttpPost("seed")]
    public async Task<IActionResult> SeedTestData([FromQuery] int count = 100, [FromQuery] double donePercentage = 0.7)
    {
        try
        {
            _logger.LogInformation("API: Starting to seed {Count} test tasks", count);
            await _dataResetService.SeedTestTasksAsync(count, donePercentage);
            _logger.LogInformation("API: Successfully seeded {Count} test tasks", count);

            return Ok(new {
                Success = true,
                Message = $"Successfully generated {count} test tasks ({(int)(count * donePercentage)} Done, {count - (int)(count * donePercentage)} In Progress/To Do)",
                TaskCount = count,
                DonePercentage = donePercentage
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "API: Error seeding test data");
            return StatusCode(500, new {
                Success = false,
                Message = $"Error generating test data: {ex.Message}"
            });
        }
    }
}
