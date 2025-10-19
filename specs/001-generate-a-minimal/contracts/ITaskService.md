# Service Contract: ITaskService

**Feature**: 001-generate-a-minimal
**Service**: Task Management Service
**Purpose**: Provides business logic for CRUD operations on tasks

## Interface Definition

```csharp
public interface ITaskService
{
    /// <summary>
    /// Retrieves all tasks from the system
    /// </summary>
    /// <returns>List of all tasks ordered by creation date (newest first)</returns>
    Task<List<TaskDto>> GetAllTasksAsync();

    /// <summary>
    /// Retrieves tasks filtered by status
    /// </summary>
    /// <param name="status">The status to filter by</param>
    /// <returns>List of tasks with the specified status</returns>
    Task<List<TaskDto>> GetTasksByStatusAsync(TaskStatus status);

    /// <summary>
    /// Retrieves a single task by ID
    /// </summary>
    /// <param name="id">The unique identifier of the task</param>
    /// <returns>Task DTO if found, null otherwise</returns>
    Task<TaskDto?> GetTaskByIdAsync(Guid id);

    /// <summary>
    /// Creates a new task in the system
    /// </summary>
    /// <param name="request">The task creation request</param>
    /// <returns>Result containing created task or error message</returns>
    Task<Result<TaskDto>> CreateTaskAsync(CreateTaskRequest request);

    /// <summary>
    /// Updates an existing task
    /// </summary>
    /// <param name="id">The unique identifier of the task to update</param>
    /// <param name="request">The updated task data</param>
    /// <returns>Result containing updated task or error message</returns>
    Task<Result<TaskDto>> UpdateTaskAsync(Guid id, UpdateTaskRequest request);

    /// <summary>
    /// Updates only the status of a task
    /// </summary>
    /// <param name="id">The unique identifier of the task</param>
    /// <param name="newStatus">The new status to set</param>
    /// <returns>Result containing updated task or error message</returns>
    Task<Result<TaskDto>> UpdateTaskStatusAsync(Guid id, TaskStatus newStatus);

    /// <summary>
    /// Deletes a task from the system
    /// </summary>
    /// <param name="id">The unique identifier of the task to delete</param>
    /// <returns>Result indicating success or failure</returns>
    Task<Result> DeleteTaskAsync(Guid id);
}
```

## Data Transfer Objects (DTOs)

### TaskDto

```csharp
public class TaskDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public TaskStatus Status { get; set; }
    public TaskPriority Priority { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
```

### CreateTaskRequest

```csharp
public class CreateTaskRequest
{
    [Required(ErrorMessage = "Title is required")]
    [StringLength(200, MinimumLength = 1, ErrorMessage = "Title must be between 1 and 200 characters")]
    public string Title { get; set; } = string.Empty;

    [Required(ErrorMessage = "Description is required")]
    [StringLength(5000, MinimumLength = 1, ErrorMessage = "Description must be between 1 and 5000 characters")]
    public string Description { get; set; } = string.Empty;

    [Required]
    public TaskPriority Priority { get; set; } = TaskPriority.Medium;
}
```

### UpdateTaskRequest

```csharp
public class UpdateTaskRequest
{
    [Required(ErrorMessage = "Title is required")]
    [StringLength(200, MinimumLength = 1, ErrorMessage = "Title must be between 1 and 200 characters")]
    public string Title { get; set; } = string.Empty;

    [Required(ErrorMessage = "Description is required")]
    [StringLength(5000, MinimumLength = 1, ErrorMessage = "Description must be between 1 and 5000 characters")]
    public string Description { get; set; } = string.Empty;

    [Required]
    public TaskStatus Status { get; set; }

    [Required]
    public TaskPriority Priority { get; set; }
}
```

### Result<T>

```csharp
public class Result
{
    public bool IsSuccess { get; }
    public string ErrorMessage { get; }

    protected Result(bool isSuccess, string errorMessage = "")
    {
        IsSuccess = isSuccess;
        ErrorMessage = errorMessage;
    }

    public static Result Success() => new Result(true);
    public static Result Failure(string errorMessage) => new Result(false, errorMessage);
}

public class Result<T> : Result
{
    public T? Value { get; }

    private Result(bool isSuccess, T? value, string errorMessage = "")
        : base(isSuccess, errorMessage)
    {
        Value = value;
    }

    public static Result<T> Success(T value) => new Result<T>(true, value);
    public static new Result<T> Failure(string errorMessage) => new Result<T>(false, default, errorMessage);
}
```

## Contract Tests

### GetAllTasksAsync Tests

```csharp
[Fact]
public async Task GetAllTasksAsync_ShouldReturnEmptyList_WhenNoTasksExist()
{
    // Arrange
    var service = CreateService();

    // Act
    var result = await service.GetAllTasksAsync();

    // Assert
    Assert.NotNull(result);
    Assert.Empty(result);
}

[Fact]
public async Task GetAllTasksAsync_ShouldReturnTasks_OrderedByCreatedDateDesc()
{
    // Arrange
    var service = CreateService();
    var task1 = await service.CreateTaskAsync(new CreateTaskRequest { Title = "First", Description = "First task", Priority = TaskPriority.Medium });
    await Task.Delay(10); // Ensure different timestamps
    var task2 = await service.CreateTaskAsync(new CreateTaskRequest { Title = "Second", Description = "Second task", Priority = TaskPriority.Medium });

    // Act
    var result = await service.GetAllTasksAsync();

    // Assert
    Assert.Equal(2, result.Count);
    Assert.Equal("Second", result[0].Title); // Newest first
    Assert.Equal("First", result[1].Title);
}
```

### CreateTaskAsync Tests

```csharp
[Fact]
public async Task CreateTaskAsync_ShouldCreateTask_WithValidData()
{
    // Arrange
    var service = CreateService();
    var request = new CreateTaskRequest
    {
        Title = "Test Task",
        Description = "Test Description",
        Priority = TaskPriority.High
    };

    // Act
    var result = await service.CreateTaskAsync(request);

    // Assert
    Assert.True(result.IsSuccess);
    Assert.NotNull(result.Value);
    Assert.Equal("Test Task", result.Value.Title);
    Assert.Equal("Test Description", result.Value.Description);
    Assert.Equal(TaskStatus.ToDo, result.Value.Status); // Default
    Assert.Equal(TaskPriority.High, result.Value.Priority);
    Assert.NotEqual(Guid.Empty, result.Value.Id);
}

[Fact]
public async Task CreateTaskAsync_ShouldFail_WhenTitleExceedsMaxLength()
{
    // Arrange
    var service = CreateService();
    var request = new CreateTaskRequest
    {
        Title = new string('a', 201), // Exceeds 200 char limit
        Description = "Valid description",
        Priority = TaskPriority.Medium
    };

    // Act
    var result = await service.CreateTaskAsync(request);

    // Assert
    Assert.False(result.IsSuccess);
    Assert.Contains("Title", result.ErrorMessage);
}

[Fact]
public async Task CreateTaskAsync_ShouldFail_WhenTitleIsEmpty()
{
    // Arrange
    var service = CreateService();
    var request = new CreateTaskRequest
    {
        Title = "",
        Description = "Valid description",
        Priority = TaskPriority.Medium
    };

    // Act
    var result = await service.CreateTaskAsync(request);

    // Assert
    Assert.False(result.IsSuccess);
    Assert.Contains("Title is required", result.ErrorMessage);
}
```

### UpdateTaskStatusAsync Tests

```csharp
[Fact]
public async Task UpdateTaskStatusAsync_ShouldUpdateStatus_WhenTaskExists()
{
    // Arrange
    var service = CreateService();
    var createResult = await service.CreateTaskAsync(new CreateTaskRequest
    {
        Title = "Test",
        Description = "Test",
        Priority = TaskPriority.Medium
    });
    var taskId = createResult.Value!.Id;

    // Act
    var result = await service.UpdateTaskStatusAsync(taskId, TaskStatus.InProgress);

    // Assert
    Assert.True(result.IsSuccess);
    Assert.Equal(TaskStatus.InProgress, result.Value!.Status);
    Assert.True(result.Value.UpdatedAt > createResult.Value.CreatedAt);
}

[Fact]
public async Task UpdateTaskStatusAsync_ShouldFail_WhenTaskNotFound()
{
    // Arrange
    var service = CreateService();
    var nonExistentId = Guid.NewGuid();

    // Act
    var result = await service.UpdateTaskStatusAsync(nonExistentId, TaskStatus.Done);

    // Assert
    Assert.False(result.IsSuccess);
    Assert.Contains("not found", result.ErrorMessage.ToLower());
}
```

### GetTaskByIdAsync Tests

```csharp
[Fact]
public async Task GetTaskByIdAsync_ShouldReturnTask_WhenExists()
{
    // Arrange
    var service = CreateService();
    var createResult = await service.CreateTaskAsync(new CreateTaskRequest
    {
        Title = "Find Me",
        Description = "Test",
        Priority = TaskPriority.Low
    });

    // Act
    var result = await service.GetTaskByIdAsync(createResult.Value!.Id);

    // Assert
    Assert.NotNull(result);
    Assert.Equal("Find Me", result.Title);
}

[Fact]
public async Task GetTaskByIdAsync_ShouldReturnNull_WhenNotExists()
{
    // Arrange
    var service = CreateService();

    // Act
    var result = await service.GetTaskByIdAsync(Guid.NewGuid());

    // Assert
    Assert.Null(result);
}
```

## Error Scenarios

### Expected Error Messages

| Scenario | Error Message |
|----------|---------------|
| Title empty or whitespace | "Title is required and must be between 1 and 200 characters" |
| Title > 200 characters | "Title is required and must be between 1 and 200 characters" |
| Description empty | "Description is required and must be between 1 and 5000 characters" |
| Description > 5000 characters | "Description is required and must be between 1 and 5000 characters" |
| Task not found (get) | Returns null |
| Task not found (update/delete) | "Task with ID {id} not found" |
| Database error | "An error occurred while processing your request. Please try again." |
| Invalid status value | "Invalid status value" |

## Dependency Injection Registration

```csharp
// Program.cs
builder.Services.AddScoped<ITaskService, TaskService>();
```

## Usage Example in Blazor Component

```razor
@inject ITaskService TaskService

@code {
    private List<TaskDto> tasks = new();

    protected override async Task OnInitializedAsync()
    {
        tasks = await TaskService.GetAllTasksAsync();
    }

    private async Task CreateTask()
    {
        var request = new CreateTaskRequest
        {
            Title = "New Task",
            Description = "Description",
            Priority = TaskPriority.Medium
        };

        var result = await TaskService.CreateTaskAsync(request);
        if (result.IsSuccess)
        {
            tasks = await TaskService.GetAllTasksAsync();
        }
        else
        {
            // Show error to user
            Console.WriteLine(result.ErrorMessage);
        }
    }
}
```
