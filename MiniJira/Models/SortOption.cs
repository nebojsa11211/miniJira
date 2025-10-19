namespace MiniJira.Models;

/// <summary>
/// Defines the available sorting options for tasks within a column.
/// </summary>
public enum SortOption
{
    /// <summary>
    /// No sorting applied (default order)
    /// </summary>
    None = 0,

    /// <summary>
    /// Sort by priority from highest to lowest
    /// </summary>
    PriorityHighToLow = 1,

    /// <summary>
    /// Sort by priority from lowest to highest
    /// </summary>
    PriorityLowToHigh = 2,

    /// <summary>
    /// Sort by assignee name alphabetically
    /// </summary>
    AssigneeAZ = 3,

    /// <summary>
    /// Sort by creation date, newest first
    /// </summary>
    CreatedNewest = 4,

    /// <summary>
    /// Sort by creation date, oldest first
    /// </summary>
    CreatedOldest = 5,

    /// <summary>
    /// Sort by title alphabetically
    /// </summary>
    TitleAZ = 6
}
