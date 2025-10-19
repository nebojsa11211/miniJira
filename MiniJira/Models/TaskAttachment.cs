using System.ComponentModel.DataAnnotations;

namespace MiniJira.Models;

/// <summary>
/// Represents a file attached to a task
/// </summary>
public class TaskAttachment
{
    /// <summary>
    /// Unique identifier for this attachment
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// The task this file is attached to
    /// </summary>
    [Required]
    public Guid TaskId { get; set; }
    public Task Task { get; set; } = null!;

    /// <summary>
    /// The column (phase) the task was in when this file was added
    /// </summary>
    [Required]
    public Guid ColumnId { get; set; }
    public Column Column { get; set; } = null!;

    /// <summary>
    /// Original filename as uploaded by user
    /// </summary>
    [Required]
    [MaxLength(255)]
    public string FileName { get; set; } = string.Empty;

    /// <summary>
    /// Stored filename on server (unique to prevent collisions)
    /// </summary>
    [Required]
    [MaxLength(500)]
    public string StoredFileName { get; set; } = string.Empty;

    /// <summary>
    /// File size in bytes
    /// </summary>
    public long FileSizeBytes { get; set; }

    /// <summary>
    /// MIME type of the file
    /// </summary>
    [MaxLength(200)]
    public string? ContentType { get; set; }

    /// <summary>
    /// When this file was uploaded
    /// </summary>
    public DateTime UploadedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Optional: Who uploaded the file (for future user tracking)
    /// </summary>
    [MaxLength(200)]
    public string? UploadedBy { get; set; }
}
