using MiniJira.Models;

namespace MiniJira.Services.DTOs;

public class TaskDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public Models.TaskStatus Status { get; set; }
    public Models.TaskPriority Priority { get; set; }
    public string? Customer { get; set; }
    public Guid? ColumnId { get; set; }
    public Guid? AssignedUserId { get; set; }
    public string? AssignedUserName { get; set; }
    public string? AssignedUserEmail { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
