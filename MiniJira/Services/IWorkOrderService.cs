using MiniJira.Services.DTOs;

namespace MiniJira.Services;

public interface IWorkOrderService
{
    /// <summary>
    /// Gets the work order for a specific task
    /// </summary>
    Task<Result<WorkOrderHeaderDto>> GetByTaskIdAsync(Guid taskId);

    /// <summary>
    /// Creates a new work order for a task
    /// </summary>
    Task<Result<WorkOrderHeaderDto>> CreateAsync(CreateWorkOrderRequest request);

    /// <summary>
    /// Updates the work order header information
    /// </summary>
    Task<Result<WorkOrderHeaderDto>> UpdateHeaderAsync(Guid headerId, UpdateWorkOrderHeaderRequest request);

    /// <summary>
    /// Updates a single work order row
    /// </summary>
    Task<Result<WorkOrderRowDto>> UpdateRowAsync(Guid headerId, UpdateWorkOrderRowRequest request);

    /// <summary>
    /// Updates multiple work order rows at once (batch update)
    /// </summary>
    Task<Result<List<WorkOrderRowDto>>> UpdateMultipleRowsAsync(Guid headerId, List<UpdateWorkOrderRowRequest> rows);

    /// <summary>
    /// Deletes a work order
    /// </summary>
    Task<Result> DeleteAsync(Guid headerId);

    /// <summary>
    /// Exports work order to Excel format (.xlsx) matching the original layout
    /// </summary>
    Task<Result<byte[]>> ExportToExcelAsync(Guid headerId);

    /// <summary>
    /// Exports work order to CSV format
    /// </summary>
    Task<Result<string>> ExportToCsvAsync(Guid headerId);

    /// <summary>
    /// Imports work order data from an Excel file (.xlsx) matching the RadniNalog structure
    /// </summary>
    Task<Result<WorkOrderHeaderDto>> ImportFromExcelAsync(Guid headerId, Stream excelFileStream);

    /// <summary>
    /// Checks if a task already has a work order
    /// </summary>
    Task<bool> ExistsForTaskAsync(Guid taskId);
}
