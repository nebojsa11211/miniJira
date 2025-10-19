using System.ComponentModel.DataAnnotations;

namespace MiniJira.Models;

/// <summary>
/// Tracks when a task moves between columns
/// </summary>
public class TaskColumnHistory
{
    /// <summary>
    /// Unique identifier for this history entry
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// The task that was moved
    /// </summary>
    [Required]
    public Guid TaskId { get; set; }
    public Task Task { get; set; } = null!;

    /// <summary>
    /// The column the task was moved from (null if this is the first assignment)
    /// </summary>
    public Guid? FromColumnId { get; set; }
    public Column? FromColumn { get; set; }

    /// <summary>
    /// The column the task was moved to
    /// </summary>
    [Required]
    public Guid ToColumnId { get; set; }
    public Column ToColumn { get; set; } = null!;

    /// <summary>
    /// When this change occurred
    /// </summary>
    public DateTime ChangedAt { get; set; }

    /// <summary>
    /// Optional: Who made the change (for future user tracking)
    /// </summary>
    [MaxLength(200)]
    public string? ChangedBy { get; set; }
}
