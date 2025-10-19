using Microsoft.AspNetCore.Components;
using MiniJira.Models;

namespace MiniJira.Components.Shared;

public partial class PriorityIcon
{
    [Parameter, EditorRequired]
    public TaskPriority Priority { get; set; }
}
