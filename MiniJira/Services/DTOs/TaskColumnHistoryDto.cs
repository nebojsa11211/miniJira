namespace MiniJira.Services.DTOs;

public class TaskColumnHistoryDto
{
    public Guid Id { get; set; }
    public Guid TaskId { get; set; }
    public Guid? FromColumnId { get; set; }
    public string? FromColumnName { get; set; }
    public Guid ToColumnId { get; set; }
    public string ToColumnName { get; set; } = string.Empty;
    public DateTime ChangedAt { get; set; }
    public string? ChangedBy { get; set; }
}
