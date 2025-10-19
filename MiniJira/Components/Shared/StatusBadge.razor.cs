using Microsoft.AspNetCore.Components;

namespace MiniJira.Components.Shared;

public partial class StatusBadge
{
    [Parameter, EditorRequired]
    public Models.TaskStatus Status { get; set; }

    private string GetStatusText()
    {
        return Status switch
        {
            Models.TaskStatus.ToDo => "To Do",
            Models.TaskStatus.InProgress => "In Progress",
            Models.TaskStatus.Done => "Done",
            _ => Status.ToString()
        };
    }
}
