using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MiniJira.Models;

/// <summary>
/// Represents a single data row in the work order table (rows 11-40 in Excel)
/// </summary>
public class WorkOrderRow
{
    public Guid Id { get; set; }

    /// <summary>
    /// Reference to the parent Work Order Header
    /// </summary>
    [Required]
    public Guid WorkOrderHeaderId { get; set; }
    public WorkOrderHeader? WorkOrderHeader { get; set; }

    /// <summary>
    /// Row number/sequence (1-30)
    /// </summary>
    [Required]
    public int RowNumber { get; set; }

    // Position Markers (Columns B-F: PZ, P, R, O, P)
    /// <summary>
    /// Position marker PZ (Column B)
    /// </summary>
    [StringLength(10)]
    public string? PositionPZ { get; set; }

    /// <summary>
    /// Position marker P1 (Column C)
    /// </summary>
    [StringLength(10)]
    public string? PositionP1 { get; set; }

    /// <summary>
    /// Position marker R (Column D)
    /// </summary>
    [StringLength(10)]
    public string? PositionR { get; set; }

    /// <summary>
    /// Position marker O (Column E)
    /// </summary>
    [StringLength(10)]
    public string? PositionO { get; set; }

    /// <summary>
    /// Position marker P2 (Column F)
    /// </summary>
    [StringLength(10)]
    public string? PositionP2 { get; set; }

    // Dimensions (Columns G-I)
    /// <summary>
    /// Length in mm (DUŽINA - Column G)
    /// </summary>
    [Column(TypeName = "decimal(18,2)")]
    public decimal? Length { get; set; }

    /// <summary>
    /// Width in mm (ŠIRINA - Column H)
    /// </summary>
    [Column(TypeName = "decimal(18,2)")]
    public decimal? Width { get; set; }

    /// <summary>
    /// Thickness in mm (DEB. - Column I)
    /// </summary>
    [Column(TypeName = "decimal(18,2)")]
    public decimal? Thickness { get; set; }

    // Quantities (Columns J-K)
    /// <summary>
    /// Number of pieces (KOM - Column J)
    /// </summary>
    public int? Pieces { get; set; }

    /// <summary>
    /// From pieces count (IZ KOM - Column K)
    /// </summary>
    public int? FromPieces { get; set; }

    // Text Fields (Columns L-O)
    /// <summary>
    /// Description (OPIS - Columns L-M)
    /// </summary>
    [StringLength(500)]
    public string? Description { get; set; }

    /// <summary>
    /// Processing notes (OBRADA - Columns N-O)
    /// </summary>
    [StringLength(500)]
    public string? ProcessingNotes { get; set; }

    // Calculated Fields (Columns P-T) - Stored for performance but can be recalculated
    /// <summary>
    /// Square meters (M² - Column P) - Calculated: Length × Width / 1,000,000
    /// </summary>
    [Column(TypeName = "decimal(18,4)")]
    public decimal? SquareMeters { get; set; }

    /// <summary>
    /// Total square meters (M² TOT - Column Q) - Running total
    /// </summary>
    [Column(TypeName = "decimal(18,4)")]
    public decimal? SquareMetersTot { get; set; }

    /// <summary>
    /// Linear meters (M¹ - Column R) - Calculated based on perimeter or length
    /// </summary>
    [Column(TypeName = "decimal(18,4)")]
    public decimal? LinearMeters { get; set; }

    /// <summary>
    /// Cubic meters total (M³ TOT - Column S) - Calculated: Length × Width × Thickness / 1,000,000,000
    /// </summary>
    [Column(TypeName = "decimal(18,6)")]
    public decimal? CubicMetersTot { get; set; }

    /// <summary>
    /// Weight in kilograms (KG - Column T) - Calculated: CubicMeters × Material Density
    /// </summary>
    [Column(TypeName = "decimal(18,3)")]
    public decimal? WeightKg { get; set; }

    // Material density for weight calculation (optional, can be set per row or inherited from header)
    /// <summary>
    /// Material density in kg/m³ (e.g., 2500 for MDF, 700 for wood)
    /// </summary>
    [Column(TypeName = "decimal(18,2)")]
    public decimal? MaterialDensity { get; set; }

    // Metadata
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    [Timestamp]
    public byte[]? RowVersion { get; set; }

    /// <summary>
    /// Recalculates all computed fields based on current dimensions and quantities
    /// </summary>
    public void RecalculateFields()
    {
        // Calculate Square Meters: Length × Width / 1,000,000 (convert mm² to m²)
        if (Length.HasValue && Width.HasValue && Pieces.HasValue)
        {
            var singlePieceArea = (Length.Value * Width.Value) / 1_000_000m;
            SquareMeters = singlePieceArea * Pieces.Value;
        }
        else
        {
            SquareMeters = null;
        }

        // Calculate Linear Meters: Perimeter = 2 × (Length + Width) / 1000 (convert mm to m)
        if (Length.HasValue && Width.HasValue && Pieces.HasValue)
        {
            var singlePiecePerimeter = 2 * (Length.Value + Width.Value) / 1000m;
            LinearMeters = singlePiecePerimeter * Pieces.Value;
        }
        else if (Length.HasValue && Pieces.HasValue)
        {
            // If only length is available, use it
            LinearMeters = (Length.Value / 1000m) * Pieces.Value;
        }
        else
        {
            LinearMeters = null;
        }

        // Calculate Cubic Meters: Length × Width × Thickness / 1,000,000,000 (convert mm³ to m³)
        if (Length.HasValue && Width.HasValue && Thickness.HasValue && Pieces.HasValue)
        {
            var singlePieceVolume = (Length.Value * Width.Value * Thickness.Value) / 1_000_000_000m;
            CubicMetersTot = singlePieceVolume * Pieces.Value;
        }
        else
        {
            CubicMetersTot = null;
        }

        // Calculate Weight: Volume × Density
        if (CubicMetersTot.HasValue && MaterialDensity.HasValue)
        {
            WeightKg = CubicMetersTot.Value * MaterialDensity.Value;
        }
        else
        {
            WeightKg = null;
        }

        // Note: SquareMetersTot (running total) is calculated by the service layer
        // as it requires knowledge of all previous rows
    }
}
