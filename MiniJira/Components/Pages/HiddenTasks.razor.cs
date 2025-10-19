using Microsoft.AspNetCore.Components;
using MiniJira.Services;
using MiniJira.Services.DTOs;

namespace MiniJira.Components.Pages;

public partial class HiddenTasks
{
    [Inject]
    private ITaskService TaskService { get; set; } = null!;

    [Inject]
    private NavigationManager NavigationManager { get; set; } = null!;

    private List<TaskDto> hiddenTasks = new();
    private bool isLoading = true;
    private string? errorMessage = null;

    protected override async System.Threading.Tasks.Task OnInitializedAsync()
    {
        await LoadHiddenTasks();
    }

    private async System.Threading.Tasks.Task LoadHiddenTasks()
    {
        try
        {
            isLoading = true;
            errorMessage = null;
            hiddenTasks = await TaskService.GetHiddenTasksAsync();
        }
        catch (Exception ex)
        {
            errorMessage = $"Failed to load hidden tasks: {ex.Message}";
        }
        finally
        {
            isLoading = false;
        }
    }

    private async System.Threading.Tasks.Task UnhideTask(Guid taskId)
    {
        try
        {
            errorMessage = null;
            var result = await TaskService.UnhideTaskAsync(taskId);

            if (result.IsSuccess)
            {
                await LoadHiddenTasks();
                StateHasChanged();
            }
            else
            {
                errorMessage = result.ErrorMessage;
            }
        }
        catch (Exception ex)
        {
            errorMessage = $"Failed to unhide task: {ex.Message}";
        }
    }
}
