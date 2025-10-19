using System.ComponentModel.DataAnnotations;

namespace MiniJira.Models;

public class ColumnTranslation
{
    public Guid Id { get; set; }

    [Required]
    public Guid ColumnId { get; set; }

    [Required]
    [StringLength(10)]
    public string Culture { get; set; } = string.Empty;

    [Required]
    [StringLength(200)]
    public string Name { get; set; } = string.Empty;

    [StringLength(500)]
    public string Description { get; set; } = string.Empty;

    // Navigation property
    public Column Column { get; set; } = null!;
}
