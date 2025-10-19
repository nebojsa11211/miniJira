using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MiniJira.Data;
using MiniJira.Models;
using MiniJira.Services.DTOs;
using System.ComponentModel.DataAnnotations;

namespace MiniJira.Services;

public class TaskService : ITaskService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<TaskService> _logger;
    private readonly IUserService _userService;

    public TaskService(ApplicationDbContext context, ILogger<TaskService> logger, IUserService userService)
    {
        _context = context;
        _logger = logger;
        _userService = userService;
    }

    public async Task<List<TaskDto>> GetAllTasksAsync()
    {
        _logger.LogInformation("Retrieving all tasks");
        var tasks = await _context.Tasks
            .AsNoTracking()
            .Where(t => !t.IsDeleted && !t.IsHidden)
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync();

        var taskDtos = new List<TaskDto>();
        foreach (var task in tasks)
        {
            taskDtos.Add(await MapToDtoAsync(task));
        }
        return taskDtos;
    }

    public async Task<List<TaskDto>> GetTasksByStatusAsync(Models.TaskStatus status)
    {
        _logger.LogInformation("Retrieving tasks with status {Status}", status);
        var tasks = await _context.Tasks
            .AsNoTracking()
            .Where(t => t.Status == status && !t.IsDeleted && !t.IsHidden)
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync();

        var taskDtos = new List<TaskDto>();
        foreach (var task in tasks)
        {
            taskDtos.Add(await MapToDtoAsync(task));
        }
        return taskDtos;
    }

    public async Task<TaskDto?> GetTaskByIdAsync(Guid id)
    {
        _logger.LogInformation("Retrieving task with ID {TaskId}", id);
        var task = await _context.Tasks
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.Id == id && !t.IsDeleted && !t.IsHidden);

        if (task == null)
        {
            _logger.LogWarning("Task with ID {TaskId} not found", id);
        }

        return task == null ? null : await MapToDtoAsync(task);
    }

    public async Task<Result<TaskDto>> CreateTaskAsync(CreateTaskRequest request)
    {
        try
        {
            _logger.LogInformation("Creating new task with title '{Title}'", request.Title);

            // Validate the request
            var validationResults = new List<ValidationResult>();
            var validationContext = new ValidationContext(request);
            if (!Validator.TryValidateObject(request, validationContext, validationResults, true))
            {
                var errorMessage = string.Join(", ", validationResults.Select(v => v.ErrorMessage));
                _logger.LogWarning("Task creation validation failed: {ErrorMessage}", errorMessage);
                return Result<TaskDto>.Failure(errorMessage);
            }

            var task = new Models.Task
            {
                Id = Guid.NewGuid(),
                Title = request.Title.Trim(),
                Description = request.Description.Trim(),
                Priority = request.Priority,
                Customer = request.Customer?.Trim(),
                Status = Models.TaskStatus.ToDo,
                ColumnId = MapStatusToColumnId(Models.TaskStatus.ToDo), // Set initial column
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.Tasks.Add(task);

            // Log initial column assignment
            if (task.ColumnId.HasValue)
            {
                var history = new TaskColumnHistory
                {
                    Id = Guid.NewGuid(),
                    TaskId = task.Id,
                    FromColumnId = null, // Initial assignment
                    ToColumnId = task.ColumnId.Value,
                    ChangedAt = DateTime.UtcNow
                };
                _context.TaskColumnHistories.Add(history);
            }

            await _context.SaveChangesAsync();

            _logger.LogInformation("Task created successfully with ID {TaskId}", task.Id);
            return Result<TaskDto>.Success(await MapToDtoAsync(task));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create task");
            return Result<TaskDto>.Failure($"Failed to create task: {ex.Message}");
        }
    }

    public async Task<Result<TaskDto>> UpdateTaskAsync(Guid id, UpdateTaskRequest request)
    {
        try
        {
            _logger.LogInformation("Updating task {TaskId}", id);

            // Validate the request
            var validationResults = new List<ValidationResult>();
            var validationContext = new ValidationContext(request);
            if (!Validator.TryValidateObject(request, validationContext, validationResults, true))
            {
                var errorMessage = string.Join(", ", validationResults.Select(v => v.ErrorMessage));
                _logger.LogWarning("Task update validation failed for task {TaskId}: {ErrorMessage}", id, errorMessage);
                return Result<TaskDto>.Failure(errorMessage);
            }

            var task = await _context.Tasks.FindAsync(id);
            if (task == null)
            {
                _logger.LogWarning("Task with ID {TaskId} not found for update", id);
                return Result<TaskDto>.Failure($"Task with ID {id} not found");
            }

            task.Title = request.Title.Trim();
            task.Description = request.Description.Trim();
            task.Status = request.Status;
            task.Priority = request.Priority;
            task.Customer = request.Customer?.Trim();
            task.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            _logger.LogInformation("Task {TaskId} updated successfully", id);
            return Result<TaskDto>.Success(await MapToDtoAsync(task));
        }
        catch (DbUpdateConcurrencyException ex)
        {
            _logger.LogWarning(ex, "Concurrency conflict updating task {TaskId}", id);
            return Result<TaskDto>.Failure("The task was modified by another user. Please refresh and try again.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update task {TaskId}", id);
            return Result<TaskDto>.Failure($"Failed to update task: {ex.Message}");
        }
    }

    public async Task<Result<TaskDto>> UpdateTaskStatusAsync(Guid id, Models.TaskStatus newStatus)
    {
        try
        {
            _logger.LogInformation("Updating status for task {TaskId} to {NewStatus}", id, newStatus);

            var task = await _context.Tasks.FindAsync(id);
            if (task == null)
            {
                _logger.LogWarning("Task with ID {TaskId} not found for status update", id);
                return Result<TaskDto>.Failure($"Task with ID {id} not found");
            }

            var oldColumnId = task.ColumnId;
            task.Status = newStatus;

            // Update ColumnId based on TaskStatus enum value
            var newColumnId = MapStatusToColumnId(newStatus);
            task.ColumnId = newColumnId;

            task.UpdatedAt = DateTime.UtcNow;

            // Log column change if it changed
            if (oldColumnId != newColumnId)
            {
                var history = new TaskColumnHistory
                {
                    Id = Guid.NewGuid(),
                    TaskId = task.Id,
                    FromColumnId = oldColumnId,
                    ToColumnId = newColumnId,
                    ChangedAt = DateTime.UtcNow
                };
                _context.TaskColumnHistories.Add(history);
            }

            await _context.SaveChangesAsync();
            _logger.LogInformation("Task {TaskId} status updated successfully to {NewStatus}", id, newStatus);
            return Result<TaskDto>.Success(await MapToDtoAsync(task));
        }
        catch (DbUpdateConcurrencyException ex)
        {
            _logger.LogWarning(ex, "Concurrency conflict updating status for task {TaskId}", id);
            return Result<TaskDto>.Failure("The task was modified by another user. Please refresh and try again.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update status for task {TaskId}", id);
            return Result<TaskDto>.Failure($"Failed to update task status: {ex.Message}");
        }
    }

    private static Guid MapStatusToColumnId(Models.TaskStatus status)
    {
        // Map TaskStatus enum values to default system column GUIDs
        return status switch
        {
            Models.TaskStatus.ToDo => Guid.Parse("11111111-1111-1111-1111-111111111111"),
            Models.TaskStatus.InProgress => Guid.Parse("22222222-2222-2222-2222-222222222222"),
            Models.TaskStatus.Done => Guid.Parse("33333333-3333-3333-3333-333333333333"),
            _ => Guid.Parse("11111111-1111-1111-1111-111111111111") // Default to ToDo
        };
    }

    public async Task<Result<TaskDto>> UpdateTaskColumnAsync(Guid id, Guid newColumnId)
    {
        try
        {
            _logger.LogInformation("Updating column for task {TaskId} to column {ColumnId}", id, newColumnId);

            var task = await _context.Tasks.FindAsync(id);
            if (task == null)
            {
                _logger.LogWarning("Task with ID {TaskId} not found for column update", id);
                return Result<TaskDto>.Failure($"Task with ID {id} not found");
            }

            // Verify the target column exists
            var columnExists = await _context.Columns.AnyAsync(c => c.Id == newColumnId && c.IsActive);
            if (!columnExists)
            {
                _logger.LogWarning("Column with ID {ColumnId} not found or is not active", newColumnId);
                return Result<TaskDto>.Failure($"Column with ID {newColumnId} not found or is not active");
            }

            var oldColumnId = task.ColumnId;

            // Only update if column actually changed
            if (oldColumnId == newColumnId)
            {
                _logger.LogInformation("Task {TaskId} is already in column {ColumnId}, no update needed", id, newColumnId);
                return Result<TaskDto>.Success(await MapToDtoAsync(task));
            }

            task.ColumnId = newColumnId;

            // Sync Status enum if moving to a system column
            task.Status = MapColumnIdToStatus(newColumnId);

            task.UpdatedAt = DateTime.UtcNow;

            // Log column change
            var history = new TaskColumnHistory
            {
                Id = Guid.NewGuid(),
                TaskId = task.Id,
                FromColumnId = oldColumnId,
                ToColumnId = newColumnId,
                ChangedAt = DateTime.UtcNow
            };
            _context.TaskColumnHistories.Add(history);

            await _context.SaveChangesAsync();
            _logger.LogInformation("Task {TaskId} column updated successfully to column {ColumnId}", id, newColumnId);
            return Result<TaskDto>.Success(await MapToDtoAsync(task));
        }
        catch (DbUpdateConcurrencyException ex)
        {
            _logger.LogWarning(ex, "Concurrency conflict updating column for task {TaskId}", id);
            return Result<TaskDto>.Failure("The task was modified by another user. Please refresh and try again.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update column for task {TaskId}", id);
            return Result<TaskDto>.Failure($"Failed to update task column: {ex.Message}");
        }
    }

    private static Models.TaskStatus MapColumnIdToStatus(Guid columnId)
    {
        // Map system column GUIDs back to TaskStatus enum
        if (columnId == Guid.Parse("11111111-1111-1111-1111-111111111111"))
            return Models.TaskStatus.ToDo;
        if (columnId == Guid.Parse("22222222-2222-2222-2222-222222222222"))
            return Models.TaskStatus.InProgress;
        if (columnId == Guid.Parse("33333333-3333-3333-3333-333333333333"))
            return Models.TaskStatus.Done;

        // For custom columns, default to InProgress
        return Models.TaskStatus.InProgress;
    }

    public async Task<Result> DeleteTaskAsync(Guid id)
    {
        try
        {
            _logger.LogInformation("Soft deleting task {TaskId}", id);

            var task = await _context.Tasks.FindAsync(id);
            if (task == null)
            {
                _logger.LogWarning("Task with ID {TaskId} not found for deletion", id);
                return Result.Failure($"Task with ID {id} not found");
            }

            // Soft delete - set IsDeleted flag instead of removing
            task.IsDeleted = true;
            task.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            _logger.LogInformation("Task {TaskId} soft deleted successfully", id);
            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete task {TaskId}", id);
            return Result.Failure($"Failed to delete task: {ex.Message}");
        }
    }

    public async Task<Result<TaskDto>> HideTaskAsync(Guid id)
    {
        try
        {
            _logger.LogInformation("Hiding task {TaskId}", id);

            var task = await _context.Tasks.FindAsync(id);
            if (task == null)
            {
                _logger.LogWarning("Task with ID {TaskId} not found for hiding", id);
                return Result<TaskDto>.Failure($"Task with ID {id} not found");
            }

            // Only allow hiding tasks that are in Done status
            if (task.Status != Models.TaskStatus.Done)
            {
                _logger.LogWarning("Task with ID {TaskId} cannot be hidden - not in Done status", id);
                return Result<TaskDto>.Failure("Only tasks in 'Done' status can be hidden");
            }

            // Hide the task
            task.IsHidden = true;
            task.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            _logger.LogInformation("Task {TaskId} hidden successfully", id);
            return Result<TaskDto>.Success(await MapToDtoAsync(task));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to hide task {TaskId}", id);
            return Result<TaskDto>.Failure($"Failed to hide task: {ex.Message}");
        }
    }

    public async Task<Result<TaskDto>> UnhideTaskAsync(Guid id)
    {
        try
        {
            _logger.LogInformation("Unhiding task {TaskId}", id);

            var task = await _context.Tasks
                .Where(t => !t.IsDeleted)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (task == null)
            {
                _logger.LogWarning("Task with ID {TaskId} not found for unhiding", id);
                return Result<TaskDto>.Failure($"Task with ID {id} not found");
            }

            // Unhide the task
            task.IsHidden = false;
            task.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            _logger.LogInformation("Task {TaskId} unhidden successfully", id);
            return Result<TaskDto>.Success(await MapToDtoAsync(task));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to unhide task {TaskId}", id);
            return Result<TaskDto>.Failure($"Failed to unhide task: {ex.Message}");
        }
    }

    public async Task<List<TaskDto>> GetHiddenTasksAsync()
    {
        _logger.LogInformation("Retrieving all hidden tasks");

        var tasks = await _context.Tasks
            .AsNoTracking()
            .Include(t => t.Column)
                .ThenInclude(c => c!.Translations)
            .Where(t => !t.IsDeleted && t.IsHidden)
            .OrderByDescending(t => t.UpdatedAt)
            .ToListAsync();

        var taskDtos = new List<TaskDto>();
        foreach (var task in tasks)
        {
            taskDtos.Add(await MapToDtoAsync(task));
        }

        return taskDtos;
    }

    public async Task<List<TaskColumnHistoryDto>> GetTaskColumnHistoryAsync(Guid taskId)
    {
        _logger.LogInformation("Retrieving column history for task {TaskId}", taskId);

        var history = await _context.TaskColumnHistories
            .AsNoTracking()
            .Include(h => h.FromColumn)
                .ThenInclude(c => c!.Translations)
            .Include(h => h.ToColumn)
                .ThenInclude(c => c.Translations)
            .Where(h => h.TaskId == taskId)
            .OrderByDescending(h => h.ChangedAt)
            .ToListAsync();

        var culture = System.Globalization.CultureInfo.CurrentCulture.Name;

        return history.Select(h => new TaskColumnHistoryDto
        {
            Id = h.Id,
            TaskId = h.TaskId,
            FromColumnId = h.FromColumnId,
            FromColumnName = h.FromColumn?.Translations
                .FirstOrDefault(t => t.Culture == culture)?.Name
                ?? h.FromColumn?.Translations.FirstOrDefault()?.Name,
            ToColumnId = h.ToColumnId,
            ToColumnName = h.ToColumn.Translations
                .FirstOrDefault(t => t.Culture == culture)?.Name
                ?? h.ToColumn.Translations.FirstOrDefault()?.Name,
            ChangedAt = h.ChangedAt,
            ChangedBy = h.ChangedBy
        }).ToList();
    }

    public async Task<Result<TaskDto>> ChangeTaskOwnerAsync(Guid taskId, ChangeTaskOwnerRequest request, Guid changedBy)
    {
        try
        {
            _logger.LogInformation("Changing owner for task {TaskId} to user {NewOwnerId}", taskId, request.NewOwnerId);

            var task = await _context.Tasks.FindAsync(taskId);
            if (task == null)
            {
                _logger.LogWarning("Task with ID {TaskId} not found for owner change", taskId);
                return Result<TaskDto>.Failure($"Task with ID {taskId} not found");
            }

            // Record the current owner as previous owner
            var previousOwnerId = task.AssignedUserId;

            // Update task owner
            task.AssignedUserId = request.NewOwnerId;
            task.UpdatedAt = DateTime.UtcNow;

            // Create owner history record
            var ownerHistory = new TaskOwnerHistory
            {
                Id = Guid.NewGuid(),
                TaskId = taskId,
                PreviousOwnerId = previousOwnerId,
                NewOwnerId = request.NewOwnerId,
                ChangedBy = changedBy,
                ChangedAt = DateTime.UtcNow
            };
            _context.TaskOwnerHistories.Add(ownerHistory);

            await _context.SaveChangesAsync();

            _logger.LogInformation("Task {TaskId} owner changed successfully from {PreviousOwnerId} to {NewOwnerId}",
                taskId, previousOwnerId, request.NewOwnerId);
            return Result<TaskDto>.Success(await MapToDtoAsync(task));
        }
        catch (DbUpdateConcurrencyException ex)
        {
            _logger.LogWarning(ex, "Concurrency conflict changing owner for task {TaskId}", taskId);
            return Result<TaskDto>.Failure("The task was modified by another user. Please refresh and try again.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to change owner for task {TaskId}", taskId);
            return Result<TaskDto>.Failure($"Failed to change task owner: {ex.Message}");
        }
    }

    public async Task<List<TaskOwnerHistoryDto>> GetTaskOwnerHistoryAsync(Guid taskId)
    {
        _logger.LogInformation("Retrieving owner history for task {TaskId}", taskId);

        var history = await _context.TaskOwnerHistories
            .AsNoTracking()
            .Where(h => h.TaskId == taskId)
            .OrderByDescending(h => h.ChangedAt)
            .ToListAsync();

        // Get all users to map names
        var users = await _userService.GetUsersAsync();
        var userDict = users.ToDictionary(u => u.Id, u => u);

        return history.Select(h => new TaskOwnerHistoryDto
        {
            Id = h.Id,
            TaskId = h.TaskId,
            PreviousOwnerId = h.PreviousOwnerId,
            PreviousOwnerName = h.PreviousOwnerId.HasValue && userDict.ContainsKey(h.PreviousOwnerId.Value)
                ? userDict[h.PreviousOwnerId.Value].FullName
                : null,
            NewOwnerId = h.NewOwnerId,
            NewOwnerName = h.NewOwnerId.HasValue && userDict.ContainsKey(h.NewOwnerId.Value)
                ? userDict[h.NewOwnerId.Value].FullName
                : null,
            ChangedBy = h.ChangedBy,
            ChangedByName = userDict.ContainsKey(h.ChangedBy)
                ? userDict[h.ChangedBy].FullName
                : null,
            ChangedAt = h.ChangedAt
        }).ToList();
    }

    public async Task<List<string>> GetDistinctCustomersAsync()
    {
        _logger.LogInformation("Retrieving distinct customer names");

        return await _context.Tasks
            .AsNoTracking()
            .Where(t => !t.IsDeleted && !string.IsNullOrEmpty(t.Customer))
            .Select(t => t.Customer!)
            .Distinct()
            .OrderBy(c => c)
            .Take(100) // Limit for performance
            .ToListAsync();
    }

    private async Task<TaskDto> MapToDtoAsync(Models.Task task)
    {
        var dto = new TaskDto
        {
            Id = task.Id,
            Title = task.Title,
            Description = task.Description,
            Status = task.Status,
            Priority = task.Priority,
            Customer = task.Customer,
            ColumnId = task.ColumnId,
            AssignedUserId = task.AssignedUserId,
            CreatedAt = task.CreatedAt,
            UpdatedAt = task.UpdatedAt
        };

        // Get user information if task is assigned
        if (task.AssignedUserId.HasValue)
        {
            var users = await _userService.GetUsersAsync();
            var assignedUser = users.FirstOrDefault(u => u.Id == task.AssignedUserId.Value);
            if (assignedUser != null)
            {
                dto.AssignedUserName = assignedUser.FullName;
                dto.AssignedUserEmail = assignedUser.Email;
            }
        }

        return dto;
    }
}
