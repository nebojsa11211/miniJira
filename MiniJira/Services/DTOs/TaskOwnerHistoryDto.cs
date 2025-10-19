namespace MiniJira.Services.DTOs;

public class TaskOwnerHistoryDto
{
    public Guid Id { get; set; }
    public Guid TaskId { get; set; }
    public Guid? PreviousOwnerId { get; set; }
    public string? PreviousOwnerName { get; set; }
    public Guid? NewOwnerId { get; set; }
    public string? NewOwnerName { get; set; }
    public Guid ChangedBy { get; set; }
    public string ChangedByName { get; set; } = string.Empty;
    public DateTime ChangedAt { get; set; }
}
