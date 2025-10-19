using System.ComponentModel.DataAnnotations;

namespace MiniJira.Services.DTOs;

public class ReorderColumnsRequest
{
    [Required]
    [MinLength(1, ErrorMessage = "At least one column ID is required")]
    public List<Guid> ColumnIds { get; set; } = new();
}
