using System.ComponentModel.DataAnnotations;

namespace MiniJira.Models;

/// <summary>
/// Represents a comment on a task
/// </summary>
public class TaskComment
{
    /// <summary>
    /// Unique identifier for this comment
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// The task this comment belongs to
    /// </summary>
    [Required]
    public Guid TaskId { get; set; }
    public Task Task { get; set; } = null!;

    /// <summary>
    /// The column (phase) the task was in when this comment was added
    /// </summary>
    [Required]
    public Guid ColumnId { get; set; }
    public Column Column { get; set; } = null!;

    /// <summary>
    /// The comment text
    /// </summary>
    [Required]
    [MaxLength(5000)]
    public string Content { get; set; } = string.Empty;

    /// <summary>
    /// When this comment was created
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Optional: Who created the comment (for future user tracking)
    /// </summary>
    [MaxLength(200)]
    public string? CreatedBy { get; set; }

    /// <summary>
    /// When this comment was last updated (if edited)
    /// </summary>
    public DateTime? UpdatedAt { get; set; }
}
