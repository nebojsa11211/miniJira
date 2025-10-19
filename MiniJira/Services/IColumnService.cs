using MiniJira.Services.DTOs;

namespace MiniJira.Services;

public interface IColumnService
{
    /// <summary>
    /// Gets all columns ordered by Order property with localized name and description.
    /// </summary>
    /// <param name="culture">Optional culture code (e.g., "en-US", "hr-HR"). If null, uses current culture.</param>
    /// <returns>List of columns with task counts and translations.</returns>
    Task<List<ColumnDto>> GetAllColumnsAsync(string? culture = null);

    /// <summary>
    /// Gets a specific column by ID with localized name and description.
    /// </summary>
    /// <param name="id">The column ID.</param>
    /// <param name="culture">Optional culture code. If null, uses current culture.</param>
    /// <returns>The column DTO or null if not found.</returns>
    Task<ColumnDto?> GetColumnByIdAsync(Guid id, string? culture = null);

    /// <summary>
    /// Creates a new column with translations.
    /// </summary>
    /// <param name="request">The column creation request with color and translations.</param>
    /// <param name="culture">The culture to use for the primary localized response.</param>
    /// <returns>Result containing the created column or error message.</returns>
    Task<Result<ColumnDto>> CreateColumnAsync(CreateColumnRequest request, string culture);

    /// <summary>
    /// Updates an existing column.
    /// </summary>
    /// <param name="id">The column ID to update.</param>
    /// <param name="request">The column update request.</param>
    /// <param name="culture">The culture to use for the response.</param>
    /// <returns>Result containing the updated column or error message.</returns>
    Task<Result<ColumnDto>> UpdateColumnAsync(Guid id, UpdateColumnRequest request, string culture);

    /// <summary>
    /// Deletes a column. System columns and columns with tasks cannot be deleted.
    /// </summary>
    /// <param name="id">The column ID to delete.</param>
    /// <returns>Result indicating success or failure.</returns>
    Task<Result> DeleteColumnAsync(Guid id);

    /// <summary>
    /// Reorders columns based on the provided list of column IDs.
    /// </summary>
    /// <param name="request">Request containing ordered list of column IDs.</param>
    /// <returns>Result indicating success or failure.</returns>
    Task<Result> ReorderColumnsAsync(ReorderColumnsRequest request);
}
