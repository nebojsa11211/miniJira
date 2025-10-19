using System.ComponentModel.DataAnnotations;

namespace MiniJira.Services.DTOs;

public class CreateColumnRequest
{
    [Required]
    [StringLength(20, MinimumLength = 4)]
    [RegularExpression(@"^#[0-9A-Fa-f]{6}$", ErrorMessage = "Color must be a valid hex color code (e.g., #DFE1E6)")]
    public string Color { get; set; } = string.Empty;

    [Required]
    [MinLength(1, ErrorMessage = "At least one translation is required")]
    public Dictionary<string, ColumnTranslationDto> Translations { get; set; } = new();
}
