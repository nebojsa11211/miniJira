namespace MiniJira.Services.DTOs;

public class ColumnDto
{
    public Guid Id { get; set; }
    public int Order { get; set; }
    public string Color { get; set; } = string.Empty;
    public bool IsSystem { get; set; }
    public bool IsActive { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int TaskCount { get; set; }
    public Dictionary<string, ColumnTranslationDto> Translations { get; set; } = new();
}
