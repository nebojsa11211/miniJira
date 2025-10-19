namespace MiniJira.Services.DTOs;

public class CreateWorkOrderRequest
{
    public Guid TaskId { get; set; }

    // Header Fields
    public string? Customer { get; set; }
    public string? Object { get; set; }
    public string? Product { get; set; }

    // Document Info
    public string? WorkOrderNumber { get; set; }
    public string? OrderReference { get; set; }
    public string? DeliveryReference { get; set; }
    public DateTime? OrderDate { get; set; }
    public DateTime? DeliveryDate { get; set; }
    public string? PageNumber { get; set; }

    // Material and Processing
    public string? Material { get; set; }
    public string? Surface { get; set; }
    public string? Cut { get; set; }
    public string? Processing { get; set; }
    public string? Packing { get; set; }

    // Notes
    public string? Notes { get; set; }

    // Default material density (optional, can be overridden per row)
    public decimal? DefaultMaterialDensity { get; set; }
}
