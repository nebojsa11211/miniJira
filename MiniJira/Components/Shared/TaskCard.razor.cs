using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using MiniJira.Models;
using MiniJira.Services.DTOs;

namespace MiniJira.Components.Shared;

public partial class TaskCard
{
    [Parameter, EditorRequired]
    public TaskDto Task { get; set; } = null!;

    [Parameter]
    public EventCallback<Guid> OnClick { get; set; }

    [Parameter]
    public bool IsRecentlyUpdated { get; set; }

    [Parameter]
    public string? Class { get; set; }

    private async System.Threading.Tasks.Task HandleClick()
    {
        await OnClick.InvokeAsync(Task.Id);
    }

    private async System.Threading.Tasks.Task HandleKeyDown(KeyboardEventArgs e)
    {
        if (e.Key == "Enter" || e.Key == " ")
        {
            await OnClick.InvokeAsync(Task.Id);
        }
    }

    private string GetCategoryName()
    {
        // Map priority to category for demo purposes
        // In a real app, this would come from the task data
        return Task.Priority switch
        {
            TaskPriority.High => "BILLING",
            TaskPriority.Medium => "ACCOUNTS",
            TaskPriority.Low => "FORMS",
            _ => "GENERAL"
        };
    }

    private string GetCategoryClass()
    {
        return Task.Priority switch
        {
            TaskPriority.High => "billing",
            TaskPriority.Medium => "accounts",
            TaskPriority.Low => "forms",
            _ => ""
        };
    }

    private string GetTaskIdDisplay()
    {
        // Format: NUC-XXX (using first 3 chars of GUID as number)
        var shortId = Math.Abs(Task.Id.GetHashCode()) % 1000;
        return $"NUC-{shortId}";
    }

    private string GetInitials()
    {
        // If no user is assigned, show placeholder
        if (string.IsNullOrWhiteSpace(Task.AssignedUserName))
            return "?";

        // Generate initials from assigned user's name
        var words = Task.AssignedUserName.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (words.Length >= 2)
            return $"{words[0][0]}{words[^1][0]}".ToUpper();
        if (words.Length == 1 && words[0].Length >= 2)
            return words[0].Substring(0, 2).ToUpper();
        return words[0][0].ToString().ToUpper();
    }

    private string FormatDate(DateTime date)
    {
        return date.ToString("dd/MM/yy");
    }

    private string GetCreatedDate()
    {
        return FormatDate(Task.CreatedAt);
    }

    private string GetUpdatedDate()
    {
        return FormatDate(Task.UpdatedAt);
    }

    private string GetPriorityClass()
    {
        return Task.Priority switch
        {
            TaskPriority.High => "priority-high",
            TaskPriority.Medium => "priority-medium",
            TaskPriority.Low => "priority-low",
            _ => ""
        };
    }
}
