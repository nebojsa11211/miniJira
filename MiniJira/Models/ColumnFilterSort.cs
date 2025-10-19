namespace MiniJira.Models;

/// <summary>
/// Holds the filter and sort state for a specific column.
/// </summary>
public class ColumnFilterSort
{
    /// <summary>
    /// The ID of the column this filter/sort applies to
    /// </summary>
    public Guid ColumnId { get; set; }

    /// <summary>
    /// Current sort option applied to the column
    /// </summary>
    public SortOption SortOption { get; set; } = SortOption.None;

    /// <summary>
    /// Selected priority levels to filter by (empty means all priorities shown)
    /// </summary>
    public HashSet<TaskPriority> SelectedPriorities { get; set; } = new();

    /// <summary>
    /// Selected assignee user IDs to filter by (empty means all assignees shown)
    /// </summary>
    public HashSet<Guid> SelectedAssignees { get; set; } = new();

    /// <summary>
    /// Whether any filters are currently active
    /// </summary>
    public bool HasActiveFilters => SelectedPriorities.Count > 0 || SelectedAssignees.Count > 0;

    /// <summary>
    /// Whether any sort option is applied (other than None)
    /// </summary>
    public bool HasActiveSort => SortOption != SortOption.None;

    /// <summary>
    /// Clears all filters but preserves sort option
    /// </summary>
    public void ClearFilters()
    {
        SelectedPriorities.Clear();
        SelectedAssignees.Clear();
    }

    /// <summary>
    /// Clears sort option but preserves filters
    /// </summary>
    public void ClearSort()
    {
        SortOption = SortOption.None;
    }

    /// <summary>
    /// Clears both filters and sort
    /// </summary>
    public void ClearAll()
    {
        ClearFilters();
        ClearSort();
    }
}
