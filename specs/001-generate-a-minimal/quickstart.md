# Quickstart Guide: MiniJira Blazor Application

**Feature**: 001-generate-a-minimal
**Purpose**: Step-by-step guide to set up, run, and validate the MiniJira application

## Prerequisites

- .NET 8.0 SDK or .NET 9.0 SDK
- Visual Studio 2022 / VS Code / Rider (any IDE with C# support)
- Git (for version control)
- Terminal/PowerShell/Command Prompt

**Verify Prerequisites**:
```bash
dotnet --version  # Should show 8.0.x or 9.0.x
```

---

## Step 1: Project Setup

### Create Blazor Server Project

```bash
# Create new Blazor Server application
dotnet new blazorserver -n MiniJira -o MiniJira

# Navigate to project directory
cd MiniJira

# Add required NuGet packages
dotnet add package Microsoft.EntityFrameworkCore.Sqlite
dotnet add package Microsoft.EntityFrameworkCore.Design
dotnet add package Microsoft.EntityFrameworkCore.Tools

# Create test project
dotnet new xunit -n MiniJira.Tests -o ../MiniJira.Tests

# Add test dependencies
cd ../MiniJira.Tests
dotnet add package bUnit
dotnet add package bUnit.web
dotnet add package NSubstitute
dotnet add package FluentAssertions

# Add reference to main project
dotnet add reference ../MiniJira/MiniJira.csproj

# Return to solution root
cd ..
```

### Create Solution (Optional but Recommended)

```bash
dotnet new sln -n MiniJira
dotnet sln add MiniJira/MiniJira.csproj
dotnet sln add MiniJira.Tests/MiniJira.Tests.csproj
```

**Expected Structure**:
```
MiniJira/
├── MiniJira/
│   ├── MiniJira.csproj
│   ├── Program.cs
│   └── Components/
├── MiniJira.Tests/
│   └── MiniJira.Tests.csproj
└── MiniJira.sln
```

---

## Step 2: Create Data Model

### Create Models Directory and Entities

```bash
cd MiniJira
mkdir Models
```

**Create `Models/TaskStatus.cs`**:
```csharp
namespace MiniJira.Models;

public enum TaskStatus
{
    ToDo = 0,
    InProgress = 1,
    Done = 2
}
```

**Create `Models/TaskPriority.cs`**:
```csharp
namespace MiniJira.Models;

public enum TaskPriority
{
    Low = 0,
    Medium = 1,
    High = 2
}
```

**Create `Models/Task.cs`**:
```csharp
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

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
```

---

## Step 3: Create Database Context

### Create Data Directory

```bash
mkdir Data
```

**Create `Data/ApplicationDbContext.cs`**:
```csharp
using Microsoft.EntityFrameworkCore;
using MiniJira.Models;

namespace MiniJira.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Models.Task> Tasks { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Models.Task>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Title)
                .IsRequired()
                .HasMaxLength(200);

            entity.Property(e => e.Description)
                .IsRequired()
                .HasMaxLength(5000);

            entity.Property(e => e.Status)
                .IsRequired()
                .HasDefaultValue(TaskStatus.ToDo);

            entity.Property(e => e.Priority)
                .IsRequired()
                .HasDefaultValue(TaskPriority.Medium);

            entity.HasIndex(e => e.Status);
            entity.HasIndex(e => e.CreatedAt);
        });
    }
}
```

---

## Step 4: Create Service Layer

### Create Services Directory and DTOs

```bash
mkdir Services
mkdir Services/DTOs
```

**Create `Services/DTOs/TaskDto.cs`**:
```csharp
using MiniJira.Models;

namespace MiniJira.Services.DTOs;

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

**Create `Services/DTOs/CreateTaskRequest.cs`**:
```csharp
using System.ComponentModel.DataAnnotations;
using MiniJira.Models;

namespace MiniJira.Services.DTOs;

public class CreateTaskRequest
{
    [Required(ErrorMessage = "Title is required")]
    [StringLength(200, MinimumLength = 1)]
    public string Title { get; set; } = string.Empty;

    [Required(ErrorMessage = "Description is required")]
    [StringLength(5000, MinimumLength = 1)]
    public string Description { get; set; } = string.Empty;

    [Required]
    public TaskPriority Priority { get; set; } = TaskPriority.Medium;
}
```

**Create `Services/DTOs/UpdateTaskRequest.cs`**:
```csharp
using System.ComponentModel.DataAnnotations;
using MiniJira.Models;

namespace MiniJira.Services.DTOs;

public class UpdateTaskRequest
{
    [Required]
    [StringLength(200, MinimumLength = 1)]
    public string Title { get; set; } = string.Empty;

    [Required]
    [StringLength(5000, MinimumLength = 1)]
    public string Description { get; set; } = string.Empty;

    [Required]
    public TaskStatus Status { get; set; }

    [Required]
    public TaskPriority Priority { get; set; }
}
```

**Create `Services/DTOs/Result.cs`**:
```csharp
namespace MiniJira.Services.DTOs;

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

**Create `Services/ITaskService.cs`**:
```csharp
using MiniJira.Models;
using MiniJira.Services.DTOs;

namespace MiniJira.Services;

public interface ITaskService
{
    Task<List<TaskDto>> GetAllTasksAsync();
    Task<List<TaskDto>> GetTasksByStatusAsync(TaskStatus status);
    Task<TaskDto?> GetTaskByIdAsync(Guid id);
    Task<Result<TaskDto>> CreateTaskAsync(CreateTaskRequest request);
    Task<Result<TaskDto>> UpdateTaskAsync(Guid id, UpdateTaskRequest request);
    Task<Result<TaskDto>> UpdateTaskStatusAsync(Guid id, TaskStatus newStatus);
    Task<Result> DeleteTaskAsync(Guid id);
}
```

**Create `Services/TaskService.cs`**:
```csharp
using Microsoft.EntityFrameworkCore;
using MiniJira.Data;
using MiniJira.Models;
using MiniJira.Services.DTOs;

namespace MiniJira.Services;

public class TaskService : ITaskService
{
    private readonly ApplicationDbContext _context;

    public TaskService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<TaskDto>> GetAllTasksAsync()
    {
        return await _context.Tasks
            .AsNoTracking()
            .OrderByDescending(t => t.CreatedAt)
            .Select(t => MapToDto(t))
            .ToListAsync();
    }

    public async Task<List<TaskDto>> GetTasksByStatusAsync(TaskStatus status)
    {
        return await _context.Tasks
            .AsNoTracking()
            .Where(t => t.Status == status)
            .OrderByDescending(t => t.CreatedAt)
            .Select(t => MapToDto(t))
            .ToListAsync();
    }

    public async Task<TaskDto?> GetTaskByIdAsync(Guid id)
    {
        var task = await _context.Tasks
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.Id == id);

        return task == null ? null : MapToDto(task);
    }

    public async Task<Result<TaskDto>> CreateTaskAsync(CreateTaskRequest request)
    {
        try
        {
            var task = new Models.Task
            {
                Id = Guid.NewGuid(),
                Title = request.Title.Trim(),
                Description = request.Description.Trim(),
                Priority = request.Priority,
                Status = TaskStatus.ToDo,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.Tasks.Add(task);
            await _context.SaveChangesAsync();

            return Result<TaskDto>.Success(MapToDto(task));
        }
        catch (Exception ex)
        {
            return Result<TaskDto>.Failure($"Failed to create task: {ex.Message}");
        }
    }

    public async Task<Result<TaskDto>> UpdateTaskAsync(Guid id, UpdateTaskRequest request)
    {
        try
        {
            var task = await _context.Tasks.FindAsync(id);
            if (task == null)
                return Result<TaskDto>.Failure($"Task with ID {id} not found");

            task.Title = request.Title.Trim();
            task.Description = request.Description.Trim();
            task.Status = request.Status;
            task.Priority = request.Priority;
            task.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return Result<TaskDto>.Success(MapToDto(task));
        }
        catch (Exception ex)
        {
            return Result<TaskDto>.Failure($"Failed to update task: {ex.Message}");
        }
    }

    public async Task<Result<TaskDto>> UpdateTaskStatusAsync(Guid id, TaskStatus newStatus)
    {
        try
        {
            var task = await _context.Tasks.FindAsync(id);
            if (task == null)
                return Result<TaskDto>.Failure($"Task with ID {id} not found");

            task.Status = newStatus;
            task.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return Result<TaskDto>.Success(MapToDto(task));
        }
        catch (Exception ex)
        {
            return Result<TaskDto>.Failure($"Failed to update task status: {ex.Message}");
        }
    }

    public async Task<Result> DeleteTaskAsync(Guid id)
    {
        try
        {
            var task = await _context.Tasks.FindAsync(id);
            if (task == null)
                return Result.Failure($"Task with ID {id} not found");

            _context.Tasks.Remove(task);
            await _context.SaveChangesAsync();

            return Result.Success();
        }
        catch (Exception ex)
        {
            return Result.Failure($"Failed to delete task: {ex.Message}");
        }
    }

    private static TaskDto MapToDto(Models.Task task) => new()
    {
        Id = task.Id,
        Title = task.Title,
        Description = task.Description,
        Status = task.Status,
        Priority = task.Priority,
        CreatedAt = task.CreatedAt,
        UpdatedAt = task.UpdatedAt
    };
}
```

---

## Step 5: Configure Services in Program.cs

**Update `Program.cs`**:
```csharp
using Microsoft.EntityFrameworkCore;
using MiniJira.Data;
using MiniJira.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// Add DbContext with SQLite
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite("Data Source=minijira.db"));

// Add custom services
builder.Services.AddScoped<ITaskService, TaskService>();

var app = builder.Build();

// Ensure database is created
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    dbContext.Database.EnsureCreated();
}

// Configure the HTTP request pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
```

---

## Step 6: Run Database Migration

```bash
# Install EF Core tools (if not already installed)
dotnet tool install --global dotnet-ef

# Create initial migration
dotnet ef migrations add InitialCreate --project MiniJira

# Apply migration (create database)
dotnet ef database update --project MiniJira
```

**Expected Output**: `minijira.db` file created in MiniJira project directory

---

## Step 7: Build and Run

```bash
# Build solution
dotnet build

# Run application
dotnet run --project MiniJira

# Expected output:
# info: Microsoft.Hosting.Lifetime[14]
#       Now listening on: https://localhost:5001
#       Now listening on: http://localhost:5000
```

Open browser to: `https://localhost:5001`

---

## Step 8: Validation Tests

### Manual Validation Checklist

**✅ Test 1: View Empty Task List**
1. Navigate to `https://localhost:5001`
2. Should see empty task list or "No tasks found" message
3. Should see "Create Task" button

**✅ Test 2: Create a Task**
1. Click "Create Task" button
2. Enter:
   - Title: "Setup project"
   - Description: "Initialize Blazor project and database"
   - Priority: High
3. Click "Save"
4. Should redirect to task list
5. Should see the created task

**✅ Test 3: View Task Details**
1. Click on the task card
2. Should navigate to task detail page
3. Should display:
   - Title
   - Full description
   - Status badge (ToDo)
   - Priority badge (High)
   - Created/Updated timestamps

**✅ Test 4: Update Task Status**
1. On task detail page, click "In Progress" button
2. Status badge should update to "In Progress"
3. Updated timestamp should change

**✅ Test 5: Edit Task**
1. Click "Edit" button
2. Modify title and description
3. Click "Save"
4. Should see updated information
5. Click "Cancel" without saving, changes should revert

**✅ Test 6: Form Validation**
1. Go to Create Task page
2. Leave Title empty, try to submit
3. Should see validation error: "Title is required"
4. Enter title > 200 characters
5. Should see validation error about max length

**✅ Test 7: Multiple Tasks**
1. Create 3-5 tasks with different statuses
2. All should appear in task list
3. Should be ordered by creation date (newest first)

**✅ Test 8: Task Persistence**
1. Create several tasks
2. Stop the application (Ctrl+C)
3. Restart: `dotnet run --project MiniJira`
4. Navigate to homepage
5. All tasks should still be present (FR-004 validation)

---

## Step 9: Run Automated Tests

```bash
# Run all tests
dotnet test

# Run with detailed output
dotnet test --logger "console;verbosity=detailed"

# Run specific test
dotnet test --filter "TaskServiceTests"
```

**Expected**: All tests should pass (initially some will fail - this is expected TDD)

---

## Troubleshooting

### Issue: Database not created
**Solution**:
```bash
dotnet ef database drop --project MiniJira  # Remove old database
dotnet ef database update --project MiniJira  # Recreate
```

### Issue: Port already in use
**Solution**: Update `launchSettings.json` to use different ports:
```json
"applicationUrl": "https://localhost:7001;http://localhost:6000"
```

### Issue: NuGet packages not restoring
**Solution**:
```bash
dotnet restore
dotnet clean
dotnet build
```

### Issue: Hot reload not working
**Solution**: Run with `--no-hot-reload` flag:
```bash
dotnet watch run --project MiniJira --no-hot-reload
```

---

## Success Criteria

✅ **All checks must pass**:
- [ ] Application builds without errors
- [ ] Database created successfully (minijira.db exists)
- [ ] Application runs on https://localhost:5001
- [ ] Can create tasks via UI
- [ ] Can view task list
- [ ] Can view individual task details
- [ ] Can update task status
- [ ] Can edit task details
- [ ] Form validation works (empty fields rejected)
- [ ] Data persists across application restarts
- [ ] All automated tests pass

---

## Next Steps

After validation:
1. Implement UI components (Pages and Shared components)
2. Add styling with Bootstrap
3. Implement Virtualize component for large lists
4. Add delete functionality with confirmation
5. Enhance error handling and user feedback
6. Add status filtering on task list page

---

## Performance Validation (1000 Tasks)

To test the 1000 task requirement:

**Create seed script** (`SeedData.cs`):
```csharp
public static void SeedTasks(ApplicationDbContext context, int count = 1000)
{
    var random = new Random();
    var statuses = Enum.GetValues<TaskStatus>();
    var priorities = Enum.GetValues<TaskPriority>();

    for (int i = 0; i < count; i++)
    {
        context.Tasks.Add(new Models.Task
        {
            Id = Guid.NewGuid(),
            Title = $"Task {i + 1}",
            Description = $"Description for task {i + 1}",
            Status = statuses[random.Next(statuses.Length)],
            Priority = priorities[random.Next(priorities.Length)],
            CreatedAt = DateTime.UtcNow.AddMinutes(-random.Next(10000)),
            UpdatedAt = DateTime.UtcNow
        });
    }

    context.SaveChanges();
}
```

**Run seed and test**:
```bash
dotnet run --project MiniJira -- seed 1000
```

**Validate**:
- Task list loads in < 2 seconds
- Scrolling is smooth (virtualization working)
- No browser lag or freezing

---

## Resources

- [Blazor Documentation](https://learn.microsoft.com/en-us/aspnet/core/blazor/)
- [Entity Framework Core](https://learn.microsoft.com/en-us/ef/core/)
- [bUnit Documentation](https://bunit.dev/)
- [xUnit Documentation](https://xunit.net/)
