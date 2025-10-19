using System.ComponentModel.DataAnnotations;

namespace MiniJira.Models;

/// <summary>
/// Represents the header/metadata section of a work order (Radni Nalog)
/// </summary>
public class WorkOrderHeader
{
    public Guid Id { get; set; }

    /// <summary>
    /// Reference to the parent Task
    /// </summary>
    [Required]
    public Guid TaskId { get; set; }
    public Task? Task { get; set; }

    // Header Fields (Row 2-3)
    /// <summary>
    /// Customer name (KUPAC)
    /// </summary>
    [StringLength(200)]
    public string? Customer { get; set; }

    /// <summary>
    /// Object/Project name (OBJEKT)
    /// </summary>
    [StringLength(200)]
    public string? Object { get; set; }

    /// <summary>
    /// Product name (PROIZVOD)
    /// </summary>
    [StringLength(200)]
    public string? Product { get; set; }

    // Document Info (Row 4)
    /// <summary>
    /// Work Order Number (RN.BR.)
    /// </summary>
    [StringLength(50)]
    public string? WorkOrderNumber { get; set; }

    /// <summary>
    /// Order reference (NARUDŽBA)
    /// </summary>
    [StringLength(100)]
    public string? OrderReference { get; set; }

    /// <summary>
    /// Delivery reference (ISPORUKA)
    /// </summary>
    [StringLength(100)]
    public string? DeliveryReference { get; set; }

    /// <summary>
    /// Order date
    /// </summary>
    public DateTime? OrderDate { get; set; }

    /// <summary>
    /// Delivery date
    /// </summary>
    public DateTime? DeliveryDate { get; set; }

    /// <summary>
    /// Page number (e.g., "1 - 1")
    /// </summary>
    [StringLength(20)]
    public string? PageNumber { get; set; }

    // Material and Processing Info (Row 5-6)
    /// <summary>
    /// Material specification (MAT. I OBRADA)
    /// </summary>
    [StringLength(200)]
    public string? Material { get; set; }

    /// <summary>
    /// Surface/Panel specification (PLOHA)
    /// </summary>
    [StringLength(100)]
    public string? Surface { get; set; }

    /// <summary>
    /// Cut specification (REZ)
    /// </summary>
    [StringLength(100)]
    public string? Cut { get; set; }

    /// <summary>
    /// Processing specification (OBRADA)
    /// </summary>
    [StringLength(100)]
    public string? Processing { get; set; }

    /// <summary>
    /// Packing specification (PAKIRANJE)
    /// </summary>
    [StringLength(100)]
    public string? Packing { get; set; }

    // Notes (Row 7-8)
    /// <summary>
    /// Notes/Comments (BILJEŠKA)
    /// </summary>
    [StringLength(1000)]
    public string? Notes { get; set; }

    // Metadata
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public Guid? CreatedBy { get; set; }
    public Guid? UpdatedBy { get; set; }

    // Navigation
    public virtual ICollection<WorkOrderRow> Rows { get; set; } = new List<WorkOrderRow>();

    [Timestamp]
    public byte[]? RowVersion { get; set; }
}
