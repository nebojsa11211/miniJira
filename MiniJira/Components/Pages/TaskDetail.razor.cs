using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.Localization;
using MiniJira.Models;
using MiniJira.Services;
using MiniJira.Services.DTOs;
using MiniJira.Resources;

namespace MiniJira.Components.Pages;

public partial class TaskDetail
{
    [Parameter]
    public Guid Id { get; set; }

    [Inject]
    private ITaskService TaskService { get; set; } = null!;

    [Inject]
    private IColumnService ColumnService { get; set; } = null!;

    [Inject]
    private IUserService UserService { get; set; } = null!;

    [Inject]
    private ICurrentUserService CurrentUserService { get; set; } = null!;

    [Inject]
    private NavigationManager NavigationManager { get; set; } = null!;

    [Inject]
    private IStringLocalizer<Localization> Localizer { get; set; } = null!;

    private TaskDto? task = null;
    private List<ColumnDto> columns = new();
    private List<User> availableUsers = new();
    private List<string> existingCustomers = new();
    private UpdateTaskRequest updateModel = new();
    private bool isEditing = false;
    private bool isSaving = false;
    private bool isLoading = true;
    private bool showDeleteConfirmation = false;
    private string? errorMessage = null;
    private string activeTab = "details";

    private void SetActiveTab(string tab)
    {
        activeTab = tab;
    }

    protected override async System.Threading.Tasks.Task OnInitializedAsync()
    {
        await System.Threading.Tasks.Task.WhenAll(LoadTask(), LoadColumns(), LoadUsers(), LoadCustomers());
    }

    protected override async System.Threading.Tasks.Task OnParametersSetAsync()
    {
        if (task == null || task.Id != Id)
        {
            await LoadTask();
        }
    }

    private async System.Threading.Tasks.Task LoadTask()
    {
        try
        {
            isLoading = true;
            errorMessage = null;

            task = await TaskService.GetTaskByIdAsync(Id);

            if (task == null)
            {
                errorMessage = Localizer["TaskDetail.TaskNotFound"];
            }

            isLoading = false;
        }
        catch (Exception ex)
        {
            errorMessage = string.Format(Localizer["TaskDetail.FailedToLoad"], ex.Message);
            isLoading = false;
        }
    }

    private void EnableEditMode()
    {
        if (task == null) return;

        updateModel = new UpdateTaskRequest
        {
            Title = task.Title,
            Description = task.Description,
            Status = task.Status,
            Priority = task.Priority,
            Customer = task.Customer
        };
        isEditing = true;
    }

    private void CancelEdit()
    {
        isEditing = false;
        updateModel = new();
    }

    private async System.Threading.Tasks.Task HandleSave()
    {
        if (task == null) return;

        try
        {
            isSaving = true;
            errorMessage = null;

            var result = await TaskService.UpdateTaskAsync(task.Id, updateModel);

            if (result.IsSuccess)
            {
                task = result.Value;
                isEditing = false;
            }
            else
            {
                errorMessage = result.ErrorMessage;
            }
        }
        catch (Exception ex)
        {
            errorMessage = string.Format(Localizer["TaskDetail.FailedToSave"], ex.Message);
        }
        finally
        {
            isSaving = false;
        }
    }

    private async System.Threading.Tasks.Task LoadColumns()
    {
        try
        {
            columns = await ColumnService.GetAllColumnsAsync();
        }
        catch (Exception)
        {
            columns = new List<ColumnDto>();
        }
    }

    private async System.Threading.Tasks.Task ChangeToColumn(Guid columnId)
    {
        if (task == null || task.ColumnId == columnId) return;

        try
        {
            errorMessage = null;

            var result = await TaskService.UpdateTaskColumnAsync(task.Id, columnId);

            if (result.IsSuccess)
            {
                task = result.Value;
            }
            else
            {
                errorMessage = string.Format(Localizer["TaskDetail.FailedToUpdate"], result.ErrorMessage ?? "");
            }

            StateHasChanged();
        }
        catch (Exception ex)
        {
            errorMessage = string.Format(Localizer["TaskDetail.FailedToUpdateStatus"], ex.Message);
            StateHasChanged();
        }
    }

    private async System.Threading.Tasks.Task ChangeStatus(Models.TaskStatus newStatus)
    {
        if (task == null || task.Status == newStatus) return;

        try
        {
            errorMessage = null;

            var result = await TaskService.UpdateTaskStatusAsync(task.Id, newStatus);

            if (result.IsSuccess)
            {
                task = result.Value;
            }
            else
            {
                errorMessage = string.Format(Localizer["TaskDetail.FailedToUpdate"], result.ErrorMessage ?? "");
            }

            StateHasChanged();
        }
        catch (Exception ex)
        {
            errorMessage = string.Format(Localizer["TaskDetail.FailedToUpdateStatus"], ex.Message);
            StateHasChanged();
        }
    }

    private void ShowDeleteConfirmation()
    {
        showDeleteConfirmation = true;
    }

    private void HideDeleteConfirmation()
    {
        showDeleteConfirmation = false;
    }

    private async System.Threading.Tasks.Task HandleDelete()
    {
        if (task == null) return;

        try
        {
            errorMessage = null;

            var result = await TaskService.DeleteTaskAsync(task.Id);

            if (result.IsSuccess)
            {
                NavigationManager.NavigateTo("/");
            }
            else
            {
                errorMessage = result.ErrorMessage;
                showDeleteConfirmation = false;
            }
        }
        catch (Exception ex)
        {
            errorMessage = string.Format(Localizer["TaskDetail.FailedToDelete"], ex.Message);
            showDeleteConfirmation = false;
        }
    }

    private async System.Threading.Tasks.Task HandleHide()
    {
        if (task == null) return;

        try
        {
            errorMessage = null;

            var result = await TaskService.HideTaskAsync(task.Id);

            if (result.IsSuccess)
            {
                NavigationManager.NavigateTo("/");
            }
            else
            {
                errorMessage = result.ErrorMessage;
            }
        }
        catch (Exception ex)
        {
            errorMessage = $"Failed to hide task: {ex.Message}";
        }
    }

    private string GetPriorityText(TaskPriority priority)
    {
        return priority switch
        {
            TaskPriority.Low => Localizer["TaskPriority.Low"],
            TaskPriority.Medium => Localizer["TaskPriority.Medium"],
            TaskPriority.High => Localizer["TaskPriority.High"],
            _ => priority.ToString()
        };
    }

    private string GetPriorityBadgeClass()
    {
        if (task == null) return "bg-secondary";

        return task.Priority switch
        {
            TaskPriority.Low => "bg-info",
            TaskPriority.Medium => "bg-warning",
            TaskPriority.High => "bg-danger",
            _ => "bg-secondary"
        };
    }

    private async System.Threading.Tasks.Task LoadUsers()
    {
        try
        {
            availableUsers = await UserService.GetUsersAsync();
        }
        catch (Exception)
        {
            availableUsers = new List<User>();
        }
    }

    private async System.Threading.Tasks.Task LoadCustomers()
    {
        try
        {
            existingCustomers = await TaskService.GetDistinctCustomersAsync();
        }
        catch (Exception)
        {
            existingCustomers = new List<string>();
        }
    }

    private async System.Threading.Tasks.Task HandleOwnerChange(ChangeEventArgs e)
    {
        if (task == null) return;

        try
        {
            errorMessage = null;
            var newOwnerIdString = e.Value?.ToString();
            Guid? newOwnerId = string.IsNullOrWhiteSpace(newOwnerIdString) ? null : Guid.Parse(newOwnerIdString);

            var currentUser = CurrentUserService.CurrentUser;
            if (currentUser == null)
            {
                errorMessage = "Current user not found";
                return;
            }

            var request = new ChangeTaskOwnerRequest { NewOwnerId = newOwnerId };
            var result = await TaskService.ChangeTaskOwnerAsync(task.Id, request, currentUser.Id);

            if (result.IsSuccess)
            {
                task = result.Value;
                StateHasChanged();
            }
            else
            {
                errorMessage = result.ErrorMessage;
                StateHasChanged();
            }
        }
        catch (Exception ex)
        {
            errorMessage = $"Failed to change owner: {ex.Message}";
            StateHasChanged();
        }
    }

    private string GetUserInitials(string fullName)
    {
        if (string.IsNullOrWhiteSpace(fullName))
            return "?";

        var words = fullName.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (words.Length >= 2)
            return $"{words[0][0]}{words[^1][0]}".ToUpper();
        if (words.Length == 1 && words[0].Length >= 2)
            return words[0].Substring(0, 2).ToUpper();
        return words[0][0].ToString().ToUpper();
    }
}
