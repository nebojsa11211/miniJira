using Microsoft.AspNetCore.Components;
using MiniJira.Models;

namespace MiniJira.Components.Shared;

/// <summary>
/// Component for filtering and sorting tasks within a single column.
/// Provides UI controls for selecting sort options and filtering by priority and assignee.
/// </summary>
public partial class ColumnFilterPanel : ComponentBase
{
    /// <summary>
    /// The current filter and sort state for this column
    /// </summary>
    [Parameter, EditorRequired]
    public ColumnFilterSort FilterState { get; set; } = default!;

    /// <summary>
    /// List of available assignees that can be filtered
    /// </summary>
    [Parameter]
    public List<User> AvailableAssignees { get; set; } = new();

    /// <summary>
    /// Callback invoked when filter or sort settings change
    /// </summary>
    [Parameter]
    public EventCallback OnFilterChanged { get; set; }

    private bool isExpanded = false;

    /// <summary>
    /// Gets or sets the current sort option with automatic change notification
    /// </summary>
    private SortOption CurrentSortOption
    {
        get => FilterState.SortOption;
        set
        {
            if (FilterState.SortOption != value)
            {
                FilterState.SortOption = value;
                NotifyFilterChanged();
            }
        }
    }

    /// <summary>
    /// Toggles the expansion state of the filter panel
    /// </summary>
    private void TogglePanel()
    {
        isExpanded = !isExpanded;
    }

    /// <summary>
    /// Checks if a specific priority is currently selected in the filter
    /// </summary>
    private bool IsPrioritySelected(TaskPriority priority)
    {
        return FilterState.SelectedPriorities.Contains(priority);
    }

    /// <summary>
    /// Toggles the selection of a priority filter
    /// </summary>
    private void TogglePriority(TaskPriority priority, bool isChecked)
    {
        if (isChecked)
        {
            FilterState.SelectedPriorities.Add(priority);
        }
        else
        {
            FilterState.SelectedPriorities.Remove(priority);
        }
        NotifyFilterChanged();
    }

    /// <summary>
    /// Checks if a specific assignee is currently selected in the filter
    /// </summary>
    private bool IsAssigneeSelected(Guid assigneeId)
    {
        return FilterState.SelectedAssignees.Contains(assigneeId);
    }

    /// <summary>
    /// Toggles the selection of an assignee filter
    /// </summary>
    private void ToggleAssignee(Guid assigneeId, bool isChecked)
    {
        if (isChecked)
        {
            FilterState.SelectedAssignees.Add(assigneeId);
        }
        else
        {
            FilterState.SelectedAssignees.Remove(assigneeId);
        }
        NotifyFilterChanged();
    }

    /// <summary>
    /// Clears all filters and sort options
    /// </summary>
    private void ClearAll()
    {
        FilterState.ClearAll();
        NotifyFilterChanged();
    }

    /// <summary>
    /// Notifies parent component that filter settings have changed
    /// </summary>
    private async void NotifyFilterChanged()
    {
        await OnFilterChanged.InvokeAsync();
    }
}
