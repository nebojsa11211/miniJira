using Microsoft.AspNetCore.Components;
using MiniJira.Services;
using MiniJira.Services.DTOs;

namespace MiniJira.Components.Shared;

public partial class TaskOwnerHistory
{
    [Parameter, EditorRequired]
    public Guid TaskId { get; set; }

    [Inject]
    private ITaskService TaskService { get; set; } = null!;

    private List<TaskOwnerHistoryDto> history = new();
    private bool isLoading = true;

    protected override async System.Threading.Tasks.Task OnInitializedAsync()
    {
        await LoadHistory();
    }

    protected override async System.Threading.Tasks.Task OnParametersSetAsync()
    {
        await LoadHistory();
    }

    private async System.Threading.Tasks.Task LoadHistory()
    {
        try
        {
            isLoading = true;
            history = await TaskService.GetTaskOwnerHistoryAsync(TaskId);
        }
        catch (Exception)
        {
            history = new List<TaskOwnerHistoryDto>();
        }
        finally
        {
            isLoading = false;
        }
    }
}
