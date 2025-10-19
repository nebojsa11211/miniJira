using Microsoft.AspNetCore.Mvc;
using MiniJira.Services;
using MiniJira.Services.DTOs;

namespace MiniJira.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ColumnController : ControllerBase
{
    private readonly IColumnService _columnService;
    private readonly ITaskService _taskService;
    private readonly ILocalizationService _localizationService;
    private readonly ILogger<ColumnController> _logger;

    public ColumnController(
        IColumnService columnService,
        ITaskService taskService,
        ILocalizationService localizationService,
        ILogger<ColumnController> logger)
    {
        _columnService = columnService;
        _taskService = taskService;
        _localizationService = localizationService;
        _logger = logger;
    }

    /// <summary>
    /// Gets all active columns ordered by their position.
    /// </summary>
    /// <param name="culture">Optional culture code (e.g., en-US, hr-HR). Defaults to current culture.</param>
    /// <returns>List of columns with localized names and descriptions.</returns>
    [HttpGet]
    [ProducesResponseType(typeof(List<ColumnDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<ColumnDto>>> GetAllColumns([FromQuery] string? culture = null)
    {
        _logger.LogInformation("GET /api/column - Culture: {Culture}", culture ?? "default");
        var columns = await _columnService.GetAllColumnsAsync(culture);
        return Ok(columns);
    }

    /// <summary>
    /// Gets a specific column by ID.
    /// </summary>
    /// <param name="id">The column ID.</param>
    /// <param name="culture">Optional culture code. Defaults to current culture.</param>
    /// <returns>The column or 404 if not found.</returns>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ColumnDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ColumnDto>> GetColumnById(Guid id, [FromQuery] string? culture = null)
    {
        _logger.LogInformation("GET /api/column/{ColumnId} - Culture: {Culture}", id, culture ?? "default");

        var column = await _columnService.GetColumnByIdAsync(id, culture);
        if (column == null)
        {
            return NotFound(new { message = $"Column with ID {id} not found" });
        }

        return Ok(column);
    }

    /// <summary>
    /// Creates a new column with translations.
    /// </summary>
    /// <param name="request">The column creation request.</param>
    /// <returns>The created column or error details.</returns>
    [HttpPost]
    [ProducesResponseType(typeof(ColumnDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ColumnDto>> CreateColumn([FromBody] CreateColumnRequest request)
    {
        _logger.LogInformation("POST /api/column - Creating new column");

        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var culture = _localizationService.CurrentCulture.Name;
        var result = await _columnService.CreateColumnAsync(request, culture);

        if (!result.IsSuccess)
        {
            return BadRequest(new { message = result.ErrorMessage });
        }

        return CreatedAtAction(
            nameof(GetColumnById),
            new { id = result.Value!.Id },
            result.Value);
    }

    /// <summary>
    /// Updates an existing column.
    /// </summary>
    /// <param name="id">The column ID to update.</param>
    /// <param name="request">The column update request.</param>
    /// <returns>The updated column or error details.</returns>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(ColumnDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ColumnDto>> UpdateColumn(Guid id, [FromBody] UpdateColumnRequest request)
    {
        _logger.LogInformation("PUT /api/column/{ColumnId}", id);

        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var culture = _localizationService.CurrentCulture.Name;
        var result = await _columnService.UpdateColumnAsync(id, request, culture);

        if (!result.IsSuccess)
        {
            if (result.ErrorMessage.Contains("not found"))
            {
                return NotFound(new { message = result.ErrorMessage });
            }
            return BadRequest(new { message = result.ErrorMessage });
        }

        return Ok(result.Value);
    }

    /// <summary>
    /// Deletes a column. System columns and columns with tasks cannot be deleted.
    /// </summary>
    /// <param name="id">The column ID to delete.</param>
    /// <returns>Success or error details.</returns>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteColumn(Guid id)
    {
        _logger.LogInformation("DELETE /api/column/{ColumnId}", id);

        var result = await _columnService.DeleteColumnAsync(id);

        if (!result.IsSuccess)
        {
            if (result.ErrorMessage.Contains("not found"))
            {
                return NotFound(new { message = result.ErrorMessage });
            }
            return BadRequest(new { message = result.ErrorMessage });
        }

        return NoContent();
    }

    /// <summary>
    /// Reorders columns based on the provided list of column IDs.
    /// </summary>
    /// <param name="request">Request containing ordered list of column IDs.</param>
    /// <returns>Success or error details.</returns>
    [HttpPut("reorder")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ReorderColumns([FromBody] ReorderColumnsRequest request)
    {
        _logger.LogInformation("PUT /api/column/reorder");

        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var result = await _columnService.ReorderColumnsAsync(request);

        if (!result.IsSuccess)
        {
            return BadRequest(new { message = result.ErrorMessage });
        }

        return NoContent();
    }

    /// <summary>
    /// Updates the column for a specific task.
    /// </summary>
    /// <param name="taskId">The task ID to update.</param>
    /// <param name="columnId">The new column ID.</param>
    /// <returns>The updated task or error details.</returns>
    [HttpPut("task/{taskId}/column/{columnId}")]
    [ProducesResponseType(typeof(TaskDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TaskDto>> UpdateTaskColumn(Guid taskId, Guid columnId)
    {
        _logger.LogInformation("PUT /api/column/task/{TaskId}/column/{ColumnId}", taskId, columnId);

        var result = await _taskService.UpdateTaskColumnAsync(taskId, columnId);

        if (!result.IsSuccess)
        {
            if (result.ErrorMessage.Contains("not found"))
            {
                return NotFound(new { message = result.ErrorMessage });
            }
            return BadRequest(new { message = result.ErrorMessage });
        }

        return Ok(result.Value);
    }

    /// <summary>
    /// Gets the column change history for a specific task.
    /// </summary>
    /// <param name="taskId">The task ID.</param>
    /// <returns>List of column changes ordered by most recent first.</returns>
    [HttpGet("task/{taskId}/history")]
    [ProducesResponseType(typeof(List<TaskColumnHistoryDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<TaskColumnHistoryDto>>> GetTaskColumnHistory(Guid taskId)
    {
        _logger.LogInformation("GET /api/column/task/{TaskId}/history", taskId);

        var history = await _taskService.GetTaskColumnHistoryAsync(taskId);
        return Ok(history);
    }
}
