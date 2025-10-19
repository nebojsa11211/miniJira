using System.ComponentModel.DataAnnotations;

namespace MiniJira.Models;

public class TaskOwnerHistory
{
    public Guid Id { get; set; }

    [Required]
    public Guid TaskId { get; set; }

    public Guid? PreviousOwnerId { get; set; }

    public Guid? NewOwnerId { get; set; }

    [Required]
    public Guid ChangedBy { get; set; }

    [Required]
    public DateTime ChangedAt { get; set; } = DateTime.UtcNow;
}
