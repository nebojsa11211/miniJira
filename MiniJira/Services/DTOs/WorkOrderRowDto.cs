namespace MiniJira.Services.DTOs;

public class WorkOrderRowDto
{
    public Guid Id { get; set; }
    public Guid WorkOrderHeaderId { get; set; }
    public int RowNumber { get; set; }

    // Position Markers
    public string? PositionPZ { get; set; }
    public string? PositionP1 { get; set; }
    public string? PositionR { get; set; }
    public string? PositionO { get; set; }
    public string? PositionP2 { get; set; }

    // Dimensions
    public decimal? Length { get; set; }
    public decimal? Width { get; set; }
    public decimal? Thickness { get; set; }

    // Quantities
    public int? Pieces { get; set; }
    public int? FromPieces { get; set; }

    // Text Fields
    public string? Description { get; set; }
    public string? ProcessingNotes { get; set; }

    // Calculated Fields
    public decimal? SquareMeters { get; set; }
    public decimal? SquareMetersTot { get; set; }
    public decimal? LinearMeters { get; set; }
    public decimal? CubicMetersTot { get; set; }
    public decimal? WeightKg { get; set; }

    // Material
    public decimal? MaterialDensity { get; set; }

    // Metadata
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
