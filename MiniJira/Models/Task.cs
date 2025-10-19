using System.ComponentModel.DataAnnotations;

namespace MiniJira.Models;

public class Task
{
    public Guid Id { get; set; }

    [Required]
    [StringLength(200)]
    public string Title { get; set; } = string.Empty;

    [Required]
    [StringLength(5000)]
    public string Description { get; set; } = string.Empty;

    [Required]
    public TaskStatus Status { get; set; } = TaskStatus.ToDo;

    [Required]
    public TaskPriority Priority { get; set; } = TaskPriority.Medium;

    /// <summary>
    /// Customer name - who this work is for
    /// </summary>
    [StringLength(200)]
    public string? Customer { get; set; }

    // New column-based system
    public Guid? ColumnId { get; set; }
    public Column? Column { get; set; }

    // User assignment
    public Guid? AssignedUserId { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public bool IsDeleted { get; set; } = false;

    public bool IsHidden { get; set; } = false;

    // Work Order
    public virtual WorkOrderHeader? WorkOrder { get; set; }

    [Timestamp]
    public byte[]? RowVersion { get; set; }
}
