using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using MiniJira.Models;
using MiniJira.Services;
using MiniJira.Services.DTOs;

namespace MiniJira.Components.Pages;

public partial class Index : IAsyncDisposable, IDisposable
{
    [Inject]
    private IColumnService ColumnService { get; set; } = default!;

    [Inject]
    private IUserService UserService { get; set; } = default!;

    private List<TaskDto> tasks = new();
    private List<ColumnDto> columns = new();
    private Dictionary<Guid, List<TaskDto>> tasksByColumn = new();
    private Dictionary<Guid, ColumnFilterSort> columnFilters = new();
    private List<User> availableAssignees = new();
    private bool isLoading = true;
    private string? errorMessage = null;
    private bool isUpdating = false;
    private DotNetObjectReference<Index>? dotNetHelper;

    protected override async System.Threading.Tasks.Task OnInitializedAsync()
    {
        LocalizationService.CultureChanged += OnCultureChanged;

        try
        {
            // Load columns, tasks, and users in parallel for better performance
            var columnsTask = ColumnService.GetAllColumnsAsync();
            var tasksTask = TaskService.GetAllTasksAsync();
            var usersTask = UserService.GetUsersAsync();

            await System.Threading.Tasks.Task.WhenAll(columnsTask, tasksTask, usersTask);

            columns = await columnsTask;
            tasks = await tasksTask;
            availableAssignees = await usersTask;

            // Initialize filter state for each column
            InitializeColumnFilters();

            // Organize tasks by column ID
            OrganizeTasksByColumn();

            isLoading = false;
        }
        catch (Exception ex)
        {
            errorMessage = string.Format(Localizer["Board.FailedToLoad"], ex.Message);
            isLoading = false;
        }
    }

    /// <summary>
    /// Initializes filter/sort state for each column
    /// </summary>
    private void InitializeColumnFilters()
    {
        columnFilters.Clear();
        foreach (var column in columns)
        {
            columnFilters[column.Id] = new ColumnFilterSort
            {
                ColumnId = column.Id
            };
        }
    }

    private void OrganizeTasksByColumn()
    {
        tasksByColumn.Clear();

        // Initialize empty lists for all columns
        foreach (var column in columns)
        {
            tasksByColumn[column.Id] = new List<TaskDto>();
        }

        // Distribute tasks to their respective columns
        foreach (var task in tasks)
        {
            // Tasks should have a ColumnId set by the backend
            // For backwards compatibility, fallback to mapping Status to default column GUIDs
            var columnId = task.ColumnId ?? MapStatusToDefaultColumnId(task.Status);

            if (tasksByColumn.ContainsKey(columnId))
            {
                tasksByColumn[columnId].Add(task);
            }
        }
    }

    private static Guid MapStatusToDefaultColumnId(Models.TaskStatus status)
    {
        // Map TaskStatus enum to default system column GUIDs (matches backend)
        return status switch
        {
            Models.TaskStatus.ToDo => Guid.Parse("11111111-1111-1111-1111-111111111111"),
            Models.TaskStatus.InProgress => Guid.Parse("22222222-2222-2222-2222-222222222222"),
            Models.TaskStatus.Done => Guid.Parse("33333333-3333-3333-3333-333333333333"),
            _ => Guid.Parse("11111111-1111-1111-1111-111111111111")
        };
    }

    private void OnCultureChanged(object? sender, EventArgs e)
    {
        // Use InvokeAsync to marshal the StateHasChanged call back to the UI thread
        // This is required because the CultureChanged event may be raised on a non-UI thread
        InvokeAsync(StateHasChanged);
    }

    protected override async System.Threading.Tasks.Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            // Create a reference to this component instance for JS callbacks
            dotNetHelper = DotNetObjectReference.Create(this);

            // Initialize the drag-drop system in JavaScript
            await JSRuntime.InvokeVoidAsync("DragDropInterop.initialize", dotNetHelper);
        }
        else if (!isLoading && !isUpdating)
        {
            // Refresh drag-drop listeners after re-render (e.g., after status update)
            await JSRuntime.InvokeVoidAsync("DragDropInterop.refresh");
        }
    }

    private void NavigateToTask(Guid taskId)
    {
        NavigationManager.NavigateTo($"/task/{taskId}");
    }

    /// <summary>
    /// Called from JavaScript when a task is dropped in a new column.
    /// This method is invoked via JS Interop.
    /// </summary>
    [JSInvokable]
    public async Task<bool> OnTaskDropped(string taskIdString, string columnIdString)
    {
        if (isUpdating)
        {
            return false;
        }

        // Parse the task ID
        if (!Guid.TryParse(taskIdString, out var taskId))
        {
            errorMessage = Localizer["Validation.InvalidTaskId"];
            StateHasChanged();
            return false;
        }

        // Parse the column ID
        if (!Guid.TryParse(columnIdString, out var columnId))
        {
            errorMessage = "Invalid column ID";
            StateHasChanged();
            return false;
        }

        // Find the task
        var task = tasks.FirstOrDefault(t => t.Id == taskId);
        if (task == null)
        {
            errorMessage = Localizer["TaskDetail.TaskNotFound"];
            StateHasChanged();
            return false;
        }

        // Find the target column
        var targetColumn = columns.FirstOrDefault(c => c.Id == columnId);
        if (targetColumn == null)
        {
            errorMessage = "Target column not found";
            StateHasChanged();
            return false;
        }

        // Get current column ID
        var currentColumnId = task.ColumnId ?? MapStatusToDefaultColumnId(task.Status);

        // Don't update if already in the target column
        if (currentColumnId == columnId)
        {
            return true;
        }

        isUpdating = true;
        var originalColumnId = currentColumnId;

        try
        {
            // Optimistic UI update
            UpdateTaskColumnOptimistically(task, columnId);
            StateHasChanged();

            // Determine the new status based on column
            // For system columns, map to the appropriate status
            var newStatus = MapColumnIdToStatus(columnId);

            // Call backend API to update status (which will also update ColumnId)
            var result = await TaskService.UpdateTaskStatusAsync(task.Id, newStatus);

            if (!result.IsSuccess)
            {
                // Rollback on failure
                UpdateTaskColumnOptimistically(task, originalColumnId);
                errorMessage = string.Format(Localizer["TaskDetail.FailedToUpdate"], result.ErrorMessage ?? "");
                StateHasChanged();
                return false;
            }
            else
            {
                // Update the task DTO with new status, column, and updated timestamp
                task.Status = newStatus;
                task.ColumnId = columnId;
                task.UpdatedAt = DateTime.UtcNow;
                errorMessage = null;
                StateHasChanged();
                return true;
            }
        }
        catch (Exception ex)
        {
            // Rollback on exception
            UpdateTaskColumnOptimistically(task, originalColumnId);
            errorMessage = string.Format(Localizer["Validation.UnexpectedError"], ex.Message);
            StateHasChanged();
            return false;
        }
        finally
        {
            isUpdating = false;
        }
    }

    private Models.TaskStatus MapColumnIdToStatus(Guid columnId)
    {
        // Map column GUID to TaskStatus enum for default system columns
        if (columnId == Guid.Parse("11111111-1111-1111-1111-111111111111"))
            return Models.TaskStatus.ToDo;
        if (columnId == Guid.Parse("22222222-2222-2222-2222-222222222222"))
            return Models.TaskStatus.InProgress;
        if (columnId == Guid.Parse("33333333-3333-3333-3333-333333333333"))
            return Models.TaskStatus.Done;

        // For custom columns, default to ToDo
        return Models.TaskStatus.ToDo;
    }

    private void UpdateTaskColumnOptimistically(TaskDto task, Guid newColumnId)
    {
        // Remove task from all column lists
        foreach (var columnList in tasksByColumn.Values)
        {
            columnList.Remove(task);
        }

        // Add task to the new column list
        if (tasksByColumn.ContainsKey(newColumnId))
        {
            tasksByColumn[newColumnId].Add(task);
        }

        // Update the task's ColumnId
        task.ColumnId = newColumnId;
    }

    public async ValueTask DisposeAsync()
    {
        // Cleanup JavaScript interop
        if (dotNetHelper != null)
        {
            await JSRuntime.InvokeVoidAsync("DragDropInterop.dispose");
            dotNetHelper.Dispose();
        }
    }

    public void Dispose()
    {
        LocalizationService.CultureChanged -= OnCultureChanged;
    }

    private void HandleLogout()
    {
        CurrentUserService.Logout();
        NavigationManager.NavigateTo("/", forceLoad: true);
    }

    private string GetUserInitials()
    {
        var currentUser = CurrentUserService.CurrentUser;
        if (currentUser == null || string.IsNullOrWhiteSpace(currentUser.FullName))
            return "?";

        var words = currentUser.FullName.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (words.Length >= 2)
            return $"{words[0][0]}{words[^1][0]}".ToUpper();
        if (words.Length == 1 && words[0].Length >= 2)
            return words[0].Substring(0, 2).ToUpper();
        return words[0][0].ToString().ToUpper();
    }

    /// <summary>
    /// Gets the filter state for a specific column
    /// </summary>
    private ColumnFilterSort GetFilterStateForColumn(Guid columnId)
    {
        if (!columnFilters.ContainsKey(columnId))
        {
            columnFilters[columnId] = new ColumnFilterSort { ColumnId = columnId };
        }
        return columnFilters[columnId];
    }

    /// <summary>
    /// Gets filtered and sorted tasks for a specific column
    /// </summary>
    private IEnumerable<TaskDto> GetFilteredTasksForColumn(Guid columnId)
    {
        // Get all tasks in this column
        var columnTasks = tasksByColumn.ContainsKey(columnId)
            ? tasksByColumn[columnId]
            : new List<TaskDto>();

        // Get filter state for this column
        var filterState = GetFilterStateForColumn(columnId);

        // Apply filters
        var filteredTasks = ApplyFilters(columnTasks, filterState);

        // Apply sort
        var sortedTasks = ApplySort(filteredTasks, filterState.SortOption);

        return sortedTasks;
    }

    /// <summary>
    /// Applies filter criteria to a list of tasks
    /// </summary>
    private IEnumerable<TaskDto> ApplyFilters(IEnumerable<TaskDto> tasks, ColumnFilterSort filterState)
    {
        var result = tasks;

        // Filter by priority if any priorities are selected
        if (filterState.SelectedPriorities.Count > 0)
        {
            result = result.Where(t => filterState.SelectedPriorities.Contains(t.Priority));
        }

        // Filter by assignee if any assignees are selected
        if (filterState.SelectedAssignees.Count > 0)
        {
            result = result.Where(t => t.AssignedUserId.HasValue &&
                                      filterState.SelectedAssignees.Contains(t.AssignedUserId.Value));
        }

        return result;
    }

    /// <summary>
    /// Applies sort option to a list of tasks
    /// </summary>
    private IEnumerable<TaskDto> ApplySort(IEnumerable<TaskDto> tasks, SortOption sortOption)
    {
        return sortOption switch
        {
            SortOption.PriorityHighToLow => tasks.OrderByDescending(t => t.Priority),
            SortOption.PriorityLowToHigh => tasks.OrderBy(t => t.Priority),
            SortOption.AssigneeAZ => tasks.OrderBy(t => t.AssignedUserName ?? string.Empty),
            SortOption.CreatedNewest => tasks.OrderByDescending(t => t.CreatedAt),
            SortOption.CreatedOldest => tasks.OrderBy(t => t.CreatedAt),
            SortOption.TitleAZ => tasks.OrderBy(t => t.Title),
            _ => tasks // SortOption.None - no sorting applied
        };
    }

    /// <summary>
    /// Handles filter/sort changes for a column
    /// </summary>
    private void HandleFilterChanged(Guid columnId)
    {
        // Trigger re-render to apply new filters/sort
        StateHasChanged();
    }
}
