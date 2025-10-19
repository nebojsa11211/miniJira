using System.ComponentModel.DataAnnotations;

namespace MiniJira.Models;

public class Column
{
    public Guid Id { get; set; }

    [Required]
    public int Order { get; set; }

    [StringLength(20)]
    public string Color { get; set; } = "#DFE1E6";

    public bool IsSystem { get; set; } = false;

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public ICollection<ColumnTranslation> Translations { get; set; } = new List<ColumnTranslation>();
    public ICollection<Task> Tasks { get; set; } = new List<Task>();
}
