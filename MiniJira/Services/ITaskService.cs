using MiniJira.Models;
using MiniJira.Services.DTOs;

namespace MiniJira.Services;

public interface ITaskService
{
    Task<List<TaskDto>> GetAllTasksAsync();
    Task<List<TaskDto>> GetTasksByStatusAsync(Models.TaskStatus status);
    Task<List<TaskDto>> GetHiddenTasksAsync();
    Task<TaskDto?> GetTaskByIdAsync(Guid id);
    Task<Result<TaskDto>> CreateTaskAsync(CreateTaskRequest request);
    Task<Result<TaskDto>> UpdateTaskAsync(Guid id, UpdateTaskRequest request);
    Task<Result<TaskDto>> UpdateTaskStatusAsync(Guid id, Models.TaskStatus newStatus);
    Task<Result<TaskDto>> UpdateTaskColumnAsync(Guid id, Guid newColumnId);
    Task<Result<TaskDto>> ChangeTaskOwnerAsync(Guid taskId, ChangeTaskOwnerRequest request, Guid changedBy);
    Task<Result> DeleteTaskAsync(Guid id);
    Task<Result<TaskDto>> HideTaskAsync(Guid id);
    Task<Result<TaskDto>> UnhideTaskAsync(Guid id);
    Task<List<TaskColumnHistoryDto>> GetTaskColumnHistoryAsync(Guid taskId);
    Task<List<TaskOwnerHistoryDto>> GetTaskOwnerHistoryAsync(Guid taskId);
    Task<List<string>> GetDistinctCustomersAsync();
}
