using MiniJira.Services.DTOs;

namespace MiniJira.Components.Pages;

public partial class CreateTask
{
    private CreateTaskRequest model = new();
    private bool isSaving = false;
    private string? errorMessage = null;
    private List<string> existingCustomers = new();

    protected override async System.Threading.Tasks.Task OnInitializedAsync()
    {
        existingCustomers = await TaskService.GetDistinctCustomersAsync();
    }

    private async System.Threading.Tasks.Task HandleValidSubmit()
    {
        try
        {
            isSaving = true;
            errorMessage = null;

            var result = await TaskService.CreateTaskAsync(model);

            if (result.IsSuccess && result.Value != null)
            {
                NavigationManager.NavigateTo($"/?newTaskId={result.Value.Id}");
            }
            else
            {
                errorMessage = result.ErrorMessage;
            }
        }
        catch (Exception ex)
        {
            errorMessage = string.Format(Localizer["Validation.UnexpectedError"], ex.Message);
        }
        finally
        {
            isSaving = false;
        }
    }

    private void HandleInvalidSubmit()
    {
        errorMessage = Localizer["Validation.FixErrors"];
    }
}
