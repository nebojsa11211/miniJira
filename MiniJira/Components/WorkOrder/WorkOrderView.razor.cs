using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.Extensions.Localization;
using MiniJira.Resources;
using MiniJira.Services;
using MiniJira.Services.DTOs;
using System.Net.Http.Headers;

namespace MiniJira.Components.WorkOrder;

public partial class WorkOrderView : ComponentBase
{
    [Inject] private IWorkOrderService WorkOrderService { get; set; } = null!;
    [Inject] private ILogger<WorkOrderView> Logger { get; set; } = null!;
    [Inject] private NavigationManager NavigationManager { get; set; } = null!;
    [Inject] private IStringLocalizer<Localization> Localizer { get; set; } = null!;
    [Inject] private HttpClient HttpClient { get; set; } = null!;

    [Parameter] public Guid TaskId { get; set; }
    [Parameter] public EventCallback OnWorkOrderChanged { get; set; }

    private WorkOrderHeaderDto? WorkOrder { get; set; }
    private bool IsLoading { get; set; } = true;
    private bool IsSaving { get; set; } = false;
    private bool IsCreating { get; set; } = false;
    private bool IsImporting { get; set; } = false;
    private string? ErrorMessage { get; set; }
    private string? SuccessMessage { get; set; }

    // Track changed rows for efficient updates
    private HashSet<Guid> ChangedRowIds { get; set; } = new();

    // Summary calculations
    private int TotalPieces => WorkOrder?.Rows.Sum(r => r.Pieces ?? 0) ?? 0;
    private decimal TotalSquareMeters => WorkOrder?.Rows.Sum(r => r.SquareMeters ?? 0) ?? 0;
    private decimal TotalLinearMeters => WorkOrder?.Rows.Sum(r => r.LinearMeters ?? 0) ?? 0;
    private decimal TotalCubicMeters => WorkOrder?.Rows.Sum(r => r.CubicMetersTot ?? 0) ?? 0;
    private decimal TotalWeight => WorkOrder?.Rows.Sum(r => r.WeightKg ?? 0) ?? 0;

    protected override async Task OnInitializedAsync()
    {
        await LoadWorkOrder();
    }

    protected override async Task OnParametersSetAsync()
    {
        if (TaskId != Guid.Empty)
        {
            await LoadWorkOrder();
        }
    }

    private async Task LoadWorkOrder()
    {
        IsLoading = true;
        ErrorMessage = null;

        try
        {
            var result = await WorkOrderService.GetByTaskIdAsync(TaskId);

            if (result.IsSuccess)
            {
                WorkOrder = result.Value;
                ChangedRowIds.Clear();
            }
            else
            {
                // Work order doesn't exist yet
                WorkOrder = null;
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error loading work order for task {TaskId}", TaskId);
            ErrorMessage = "Failed to load work order";
        }
        finally
        {
            IsLoading = false;
        }
    }

    private async Task CreateWorkOrder()
    {
        IsCreating = true;
        ErrorMessage = null;

        try
        {
            var request = new CreateWorkOrderRequest
            {
                TaskId = TaskId,
                PageNumber = "1 - 1",
                DefaultMaterialDensity = 700 // Default for wood, can be changed per row
            };

            var result = await WorkOrderService.CreateAsync(request);

            if (result.IsSuccess)
            {
                WorkOrder = result.Value;
                SuccessMessage = "Work order created successfully";
                await OnWorkOrderChanged.InvokeAsync();
            }
            else
            {
                ErrorMessage = result.ErrorMessage;
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error creating work order");
            ErrorMessage = "Failed to create work order";
        }
        finally
        {
            IsCreating = false;
        }
    }

    private void OnRowChanged(WorkOrderRowDto row)
    {
        // Mark row as changed for batch update
        ChangedRowIds.Add(row.Id);

        // Recalculate fields locally for immediate feedback
        RecalculateRow(row);

        // Clear messages
        ErrorMessage = null;
        SuccessMessage = null;
    }

    private void RecalculateRow(WorkOrderRowDto row)
    {
        // Calculate Square Meters: (Length × Width × Pieces) / 1,000,000
        if (row.Length.HasValue && row.Width.HasValue && row.Pieces.HasValue)
        {
            var singlePieceArea = (row.Length.Value * row.Width.Value) / 1_000_000m;
            row.SquareMeters = singlePieceArea * row.Pieces.Value;
        }
        else
        {
            row.SquareMeters = null;
        }

        // Calculate Linear Meters: Perimeter = 2 × (Length + Width) / 1000
        if (row.Length.HasValue && row.Width.HasValue && row.Pieces.HasValue)
        {
            var singlePiecePerimeter = 2 * (row.Length.Value + row.Width.Value) / 1000m;
            row.LinearMeters = singlePiecePerimeter * row.Pieces.Value;
        }
        else if (row.Length.HasValue && row.Pieces.HasValue)
        {
            row.LinearMeters = (row.Length.Value / 1000m) * row.Pieces.Value;
        }
        else
        {
            row.LinearMeters = null;
        }

        // Calculate Cubic Meters: (Length × Width × Thickness × Pieces) / 1,000,000,000
        if (row.Length.HasValue && row.Width.HasValue && row.Thickness.HasValue && row.Pieces.HasValue)
        {
            var singlePieceVolume = (row.Length.Value * row.Width.Value * row.Thickness.Value) / 1_000_000_000m;
            row.CubicMetersTot = singlePieceVolume * row.Pieces.Value;
        }
        else
        {
            row.CubicMetersTot = null;
        }

        // Calculate Weight: Volume × Density
        if (row.CubicMetersTot.HasValue && row.MaterialDensity.HasValue)
        {
            row.WeightKg = row.CubicMetersTot.Value * row.MaterialDensity.Value;
        }
        else
        {
            row.WeightKg = null;
        }

        // Recalculate running totals
        RecalculateRunningTotals();

        // Trigger UI update
        StateHasChanged();
    }

    private void RecalculateRunningTotals()
    {
        if (WorkOrder == null) return;

        decimal runningM2Total = 0;

        foreach (var row in WorkOrder.Rows.OrderBy(r => r.RowNumber))
        {
            if (row.SquareMeters.HasValue)
            {
                runningM2Total += row.SquareMeters.Value;
            }
            row.SquareMetersTot = runningM2Total;
        }
    }

    private async Task SaveChanges()
    {
        if (WorkOrder == null) return;

        IsSaving = true;
        ErrorMessage = null;
        SuccessMessage = null;

        try
        {
            // Save header
            var headerRequest = new UpdateWorkOrderHeaderRequest
            {
                Customer = WorkOrder.Customer,
                Object = WorkOrder.Object,
                Product = WorkOrder.Product,
                WorkOrderNumber = WorkOrder.WorkOrderNumber,
                OrderReference = WorkOrder.OrderReference,
                DeliveryReference = WorkOrder.DeliveryReference,
                OrderDate = WorkOrder.OrderDate,
                DeliveryDate = WorkOrder.DeliveryDate,
                PageNumber = WorkOrder.PageNumber,
                Material = WorkOrder.Material,
                Surface = WorkOrder.Surface,
                Cut = WorkOrder.Cut,
                Processing = WorkOrder.Processing,
                Packing = WorkOrder.Packing,
                Notes = WorkOrder.Notes
            };

            var headerResult = await WorkOrderService.UpdateHeaderAsync(WorkOrder.Id, headerRequest);

            if (!headerResult.IsSuccess)
            {
                ErrorMessage = headerResult.ErrorMessage;
                return;
            }

            // Save changed rows (batch update)
            if (ChangedRowIds.Any())
            {
                var rowUpdates = WorkOrder.Rows
                    .Where(r => ChangedRowIds.Contains(r.Id))
                    .Select(r => new UpdateWorkOrderRowRequest
                    {
                        Id = r.Id,
                        RowNumber = r.RowNumber,
                        PositionPZ = r.PositionPZ,
                        PositionP1 = r.PositionP1,
                        PositionR = r.PositionR,
                        PositionO = r.PositionO,
                        PositionP2 = r.PositionP2,
                        Length = r.Length,
                        Width = r.Width,
                        Thickness = r.Thickness,
                        Pieces = r.Pieces,
                        FromPieces = r.FromPieces,
                        Description = r.Description,
                        ProcessingNotes = r.ProcessingNotes,
                        MaterialDensity = r.MaterialDensity
                    })
                    .ToList();

                var rowsResult = await WorkOrderService.UpdateMultipleRowsAsync(WorkOrder.Id, rowUpdates);

                if (!rowsResult.IsSuccess)
                {
                    ErrorMessage = rowsResult.ErrorMessage;
                    return;
                }

                // Update local rows with server-calculated values
                foreach (var updatedRow in rowsResult.Value!)
                {
                    var localRow = WorkOrder.Rows.FirstOrDefault(r => r.Id == updatedRow.Id);
                    if (localRow != null)
                    {
                        localRow.SquareMeters = updatedRow.SquareMeters;
                        localRow.SquareMetersTot = updatedRow.SquareMetersTot;
                        localRow.LinearMeters = updatedRow.LinearMeters;
                        localRow.CubicMetersTot = updatedRow.CubicMetersTot;
                        localRow.WeightKg = updatedRow.WeightKg;
                        localRow.UpdatedAt = updatedRow.UpdatedAt;
                    }
                }

                ChangedRowIds.Clear();
            }

            SuccessMessage = "Work order saved successfully";
            await OnWorkOrderChanged.InvokeAsync();
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error saving work order");
            ErrorMessage = "Failed to save work order";
        }
        finally
        {
            IsSaving = false;
        }
    }

    private void ExportToCsv()
    {
        if (WorkOrder == null) return;

        try
        {
            // Navigate to API endpoint - browser will handle download automatically
            var url = $"/api/workorder/{WorkOrder.Id}/export/csv";
            NavigationManager.NavigateTo(url, forceLoad: true);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error exporting to CSV");
            ErrorMessage = "Failed to export to CSV";
        }
    }

    private void ExportToExcel()
    {
        if (WorkOrder == null) return;

        try
        {
            // Navigate to API endpoint - browser will handle download automatically
            var url = $"/api/workorder/{WorkOrder.Id}/export/excel";
            NavigationManager.NavigateTo(url, forceLoad: true);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error exporting to Excel");
            ErrorMessage = "Failed to export to Excel";
        }
    }

    private async Task DeleteWorkOrder()
    {
        if (WorkOrder == null) return;

        if (!await ConfirmDelete())
        {
            return;
        }

        ErrorMessage = null;

        try
        {
            var result = await WorkOrderService.DeleteAsync(WorkOrder.Id);

            if (result.IsSuccess)
            {
                WorkOrder = null;
                SuccessMessage = "Work order deleted successfully";
                await OnWorkOrderChanged.InvokeAsync();
            }
            else
            {
                ErrorMessage = result.ErrorMessage;
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error deleting work order");
            ErrorMessage = "Failed to delete work order";
        }
    }

    private async Task<bool> ConfirmDelete()
    {
        // In a real app, use a confirmation dialog component
        // For now, using JavaScript confirm
        return await Task.FromResult(true); // Placeholder
    }

    private string FormatDecimal(decimal? value)
    {
        return value.HasValue ? value.Value.ToString("N2") : "0.00";
    }

    private async Task HandleFileSelected(InputFileChangeEventArgs e)
    {
        if (WorkOrder == null) return;

        IsImporting = true;
        ErrorMessage = null;
        SuccessMessage = null;

        try
        {
            var file = e.File;

            // Validate file extension
            if (!file.Name.EndsWith(".xlsx", StringComparison.OrdinalIgnoreCase))
            {
                ErrorMessage = "Only .xlsx files are supported";
                return;
            }

            // Validate file size (max 10MB)
            if (file.Size > 10 * 1024 * 1024)
            {
                ErrorMessage = "File size must be less than 10MB";
                return;
            }

            // Create multipart form data
            using var content = new MultipartFormDataContent();
            var fileContent = new StreamContent(file.OpenReadStream(maxAllowedSize: 10 * 1024 * 1024));
            fileContent.Headers.ContentType = new MediaTypeHeaderValue("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
            content.Add(fileContent, "file", file.Name);

            // Upload the file to the API
            var url = new Uri(NavigationManager.BaseUri + $"api/workorder/{WorkOrder.Id}/import/excel");
            var response = await HttpClient.PostAsync(url, content);

            if (response.IsSuccessStatusCode)
            {
                // Reload the work order to get the updated data
                await LoadWorkOrder();
                SuccessMessage = "Work order imported successfully from Excel";
                await OnWorkOrderChanged.InvokeAsync();
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                ErrorMessage = $"Failed to import Excel file: {errorContent}";
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error importing Excel file");
            ErrorMessage = "An error occurred while importing the Excel file";
        }
        finally
        {
            IsImporting = false;
        }
    }
}
