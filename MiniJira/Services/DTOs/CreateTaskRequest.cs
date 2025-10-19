using System.ComponentModel.DataAnnotations;
using MiniJira.Models;

namespace MiniJira.Services.DTOs;

public class CreateTaskRequest
{
    [Required(ErrorMessageResourceName = "Validation.TitleRequired", ErrorMessageResourceType = typeof(Resources.Localization))]
    [StringLength(200, MinimumLength = 1)]
    public string Title { get; set; } = string.Empty;

    [Required(ErrorMessageResourceName = "Validation.DescriptionRequired", ErrorMessageResourceType = typeof(Resources.Localization))]
    [StringLength(5000, MinimumLength = 1)]
    public string Description { get; set; } = string.Empty;

    [Required]
    public TaskPriority Priority { get; set; } = TaskPriority.Medium;

    [StringLength(200)]
    public string? Customer { get; set; }
}
