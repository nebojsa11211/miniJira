using System.ComponentModel.DataAnnotations;

namespace MiniJira.Services.DTOs;

public class UpdateColumnRequest
{
    [StringLength(20, MinimumLength = 4)]
    [RegularExpression(@"^#[0-9A-Fa-f]{6}$", ErrorMessage = "Color must be a valid hex color code (e.g., #DFE1E6)")]
    public string? Color { get; set; }

    public Dictionary<string, ColumnTranslationDto>? Translations { get; set; }
}
