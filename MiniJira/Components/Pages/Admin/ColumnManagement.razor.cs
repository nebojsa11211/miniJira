using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using MiniJira.Services;
using MiniJira.Services.DTOs;
using MiniJira.Resources;

namespace MiniJira.Components.Pages.Admin;

public partial class ColumnManagement
{
    [Inject]
    private IColumnService ColumnService { get; set; } = null!;

    [Inject]
    private ILocalizationService LocalizationService { get; set; } = null!;

    [Inject]
    private IStringLocalizer<Localization> Localizer { get; set; } = null!;

    private List<ColumnDto> columns = new();
    private bool isLoading = true;
    private string? errorMessage = null;
    private string? successMessage = null;

    private bool showColumnDialog = false;
    private ColumnDto? selectedColumn = null;

    private bool showDeleteConfirmation = false;
    private ColumnDto? columnToDelete = null;

    private ColumnDto? draggedColumn = null;

    protected override async Task OnInitializedAsync()
    {
        await LoadColumns();
    }

    private async Task LoadColumns()
    {
        try
        {
            isLoading = true;
            errorMessage = null;

            var culture = LocalizationService.CurrentCulture.Name;
            columns = await ColumnService.GetAllColumnsAsync(culture);

            isLoading = false;
        }
        catch (Exception ex)
        {
            errorMessage = string.Format(Localizer["ColumnManagement.FailedToLoad"], ex.Message);
            isLoading = false;
        }
    }

    private void ShowCreateDialog()
    {
        selectedColumn = null;
        showColumnDialog = true;
    }

    private void ShowEditDialog(ColumnDto column)
    {
        selectedColumn = column;
        showColumnDialog = true;
    }

    private void HideColumnDialog()
    {
        showColumnDialog = false;
        selectedColumn = null;
    }

    private async Task HandleSaveColumn(ColumnDto column)
    {
        try
        {
            errorMessage = null;
            var culture = LocalizationService.CurrentCulture.Name;

            Result result;

            if (selectedColumn == null)
            {
                // Create new column
                var request = new CreateColumnRequest
                {
                    Color = column.Color,
                    Translations = column.Translations
                };

                result = await ColumnService.CreateColumnAsync(request, culture);

                if (result.IsSuccess)
                {
                    successMessage = Localizer["ColumnManagement.ColumnCreated"];
                }
            }
            else
            {
                // Update existing column
                var request = new UpdateColumnRequest
                {
                    Color = column.Color,
                    Translations = column.Translations
                };

                result = await ColumnService.UpdateColumnAsync(selectedColumn.Id, request, culture);

                if (result.IsSuccess)
                {
                    successMessage = Localizer["ColumnManagement.ColumnUpdated"];
                }
            }

            if (result.IsSuccess)
            {
                await LoadColumns();
                HideColumnDialog();
                AutoHideSuccessMessage();
            }
            else
            {
                errorMessage = result.ErrorMessage;
            }
        }
        catch (Exception ex)
        {
            errorMessage = string.Format(Localizer["ColumnManagement.FailedToSave"], ex.Message);
        }
    }

    private void ShowDeleteDialog(ColumnDto column)
    {
        if (column.IsSystem || column.TaskCount > 0)
        {
            return;
        }

        columnToDelete = column;
        showDeleteConfirmation = true;
    }

    private void HideDeleteDialog()
    {
        showDeleteConfirmation = false;
        columnToDelete = null;
    }

    private async Task HandleDeleteColumn()
    {
        if (columnToDelete == null) return;

        try
        {
            errorMessage = null;

            var result = await ColumnService.DeleteColumnAsync(columnToDelete.Id);

            if (result.IsSuccess)
            {
                successMessage = Localizer["ColumnManagement.ColumnDeleted"];
                await LoadColumns();
                AutoHideSuccessMessage();
            }
            else
            {
                errorMessage = result.ErrorMessage;
            }

            HideDeleteDialog();
        }
        catch (Exception ex)
        {
            errorMessage = string.Format(Localizer["ColumnManagement.FailedToDelete"], ex.Message);
            HideDeleteDialog();
        }
    }

    private void HandleDragStart(ColumnDto column)
    {
        if (column.IsSystem) return;
        draggedColumn = column;
    }

    private async Task HandleDrop(ColumnDto targetColumn)
    {
        if (draggedColumn == null || draggedColumn.Id == targetColumn.Id || draggedColumn.IsSystem)
        {
            draggedColumn = null;
            return;
        }

        try
        {
            errorMessage = null;

            // Reorder columns
            var reorderedColumns = columns.OrderBy(c => c.Order).ToList();
            var draggedIndex = reorderedColumns.FindIndex(c => c.Id == draggedColumn.Id);
            var targetIndex = reorderedColumns.FindIndex(c => c.Id == targetColumn.Id);

            if (draggedIndex >= 0 && targetIndex >= 0)
            {
                // Remove dragged column and insert at target position
                var columnToMove = reorderedColumns[draggedIndex];
                reorderedColumns.RemoveAt(draggedIndex);
                reorderedColumns.Insert(targetIndex, columnToMove);

                // Create request with new order
                var request = new ReorderColumnsRequest
                {
                    ColumnIds = reorderedColumns.Select(c => c.Id).ToList()
                };

                var result = await ColumnService.ReorderColumnsAsync(request);

                if (result.IsSuccess)
                {
                    // Optimistic UI update
                    columns = reorderedColumns;
                    StateHasChanged();

                    successMessage = Localizer["ColumnManagement.ColumnsReordered"];
                    AutoHideSuccessMessage();

                    // Reload to get accurate data from server
                    await LoadColumns();
                }
                else
                {
                    errorMessage = result.ErrorMessage;
                    await LoadColumns(); // Reload on error
                }
            }
        }
        catch (Exception ex)
        {
            errorMessage = string.Format(Localizer["ColumnManagement.FailedToReorder"], ex.Message);
            await LoadColumns(); // Reload on error
        }
        finally
        {
            draggedColumn = null;
        }
    }

    private void ClearSuccessMessage()
    {
        successMessage = null;
    }

    private async void AutoHideSuccessMessage()
    {
        await Task.Delay(3000);
        successMessage = null;
        StateHasChanged();
    }
}
