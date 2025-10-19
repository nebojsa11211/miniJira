namespace MiniJira.Services.DTOs;

public class UpdateWorkOrderRowRequest
{
    public Guid? Id { get; set; }  // Null for new rows
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

    // Material
    public decimal? MaterialDensity { get; set; }
}
