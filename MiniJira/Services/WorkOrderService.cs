using Microsoft.EntityFrameworkCore;
using MiniJira.Data;
using MiniJira.Models;
using MiniJira.Services.DTOs;
using System.Text;
using ClosedXML.Excel;

namespace MiniJira.Services;

public class WorkOrderService : IWorkOrderService
{
    private readonly ApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<WorkOrderService> _logger;

    public WorkOrderService(
        ApplicationDbContext context,
        ICurrentUserService currentUserService,
        ILogger<WorkOrderService> logger)
    {
        _context = context;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    public async Task<Result<WorkOrderHeaderDto>> GetByTaskIdAsync(Guid taskId)
    {
        try
        {
            var workOrder = await _context.WorkOrderHeaders
                .Include(w => w.Rows.OrderBy(r => r.RowNumber))
                .AsNoTracking()
                .FirstOrDefaultAsync(w => w.TaskId == taskId);

            if (workOrder == null)
            {
                return Result<WorkOrderHeaderDto>.Failure("Work order not found for this task");
            }

            var dto = MapToDto(workOrder);
            return Result<WorkOrderHeaderDto>.Success(dto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving work order for task {TaskId}", taskId);
            return Result<WorkOrderHeaderDto>.Failure("An error occurred while retrieving the work order");
        }
    }

    public async Task<Result<WorkOrderHeaderDto>> CreateAsync(CreateWorkOrderRequest request)
    {
        try
        {
            // Check if task exists
            var taskExists = await _context.Tasks.AnyAsync(t => t.Id == request.TaskId);
            if (!taskExists)
            {
                return Result<WorkOrderHeaderDto>.Failure("Task not found");
            }

            // Check if work order already exists for this task
            var existingWorkOrder = await _context.WorkOrderHeaders
                .AnyAsync(w => w.TaskId == request.TaskId);

            if (existingWorkOrder)
            {
                return Result<WorkOrderHeaderDto>.Failure("Work order already exists for this task");
            }

            var currentUserId = _currentUserService.CurrentUser?.Id;

            var workOrderHeader = new WorkOrderHeader
            {
                Id = Guid.NewGuid(),
                TaskId = request.TaskId,
                Customer = request.Customer,
                Object = request.Object,
                Product = request.Product,
                WorkOrderNumber = request.WorkOrderNumber,
                OrderReference = request.OrderReference,
                DeliveryReference = request.DeliveryReference,
                OrderDate = request.OrderDate,
                DeliveryDate = request.DeliveryDate,
                PageNumber = request.PageNumber ?? "1 - 1",
                Material = request.Material,
                Surface = request.Surface,
                Cut = request.Cut,
                Processing = request.Processing,
                Packing = request.Packing,
                Notes = request.Notes,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                CreatedBy = currentUserId,
                UpdatedBy = currentUserId
            };

            // Create 30 empty rows
            for (int i = 1; i <= 30; i++)
            {
                workOrderHeader.Rows.Add(new WorkOrderRow
                {
                    Id = Guid.NewGuid(),
                    WorkOrderHeaderId = workOrderHeader.Id,
                    RowNumber = i,
                    MaterialDensity = request.DefaultMaterialDensity,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                });
            }

            _context.WorkOrderHeaders.Add(workOrderHeader);
            await _context.SaveChangesAsync();

            var dto = MapToDto(workOrderHeader);
            return Result<WorkOrderHeaderDto>.Success(dto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating work order for task {TaskId}", request.TaskId);
            return Result<WorkOrderHeaderDto>.Failure("An error occurred while creating the work order");
        }
    }

    public async Task<Result<WorkOrderHeaderDto>> UpdateHeaderAsync(Guid headerId, UpdateWorkOrderHeaderRequest request)
    {
        try
        {
            var workOrder = await _context.WorkOrderHeaders
                .Include(w => w.Rows.OrderBy(r => r.RowNumber))
                .FirstOrDefaultAsync(w => w.Id == headerId);

            if (workOrder == null)
            {
                return Result<WorkOrderHeaderDto>.Failure("Work order not found");
            }

            var currentUserId = _currentUserService.CurrentUser?.Id;

            // Update header fields
            workOrder.Customer = request.Customer;
            workOrder.Object = request.Object;
            workOrder.Product = request.Product;
            workOrder.WorkOrderNumber = request.WorkOrderNumber;
            workOrder.OrderReference = request.OrderReference;
            workOrder.DeliveryReference = request.DeliveryReference;
            workOrder.OrderDate = request.OrderDate;
            workOrder.DeliveryDate = request.DeliveryDate;
            workOrder.PageNumber = request.PageNumber;
            workOrder.Material = request.Material;
            workOrder.Surface = request.Surface;
            workOrder.Cut = request.Cut;
            workOrder.Processing = request.Processing;
            workOrder.Packing = request.Packing;
            workOrder.Notes = request.Notes;
            workOrder.UpdatedAt = DateTime.UtcNow;
            workOrder.UpdatedBy = currentUserId;

            await _context.SaveChangesAsync();

            var dto = MapToDto(workOrder);
            return Result<WorkOrderHeaderDto>.Success(dto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating work order header {HeaderId}", headerId);
            return Result<WorkOrderHeaderDto>.Failure("An error occurred while updating the work order");
        }
    }

    public async Task<Result<WorkOrderRowDto>> UpdateRowAsync(Guid headerId, UpdateWorkOrderRowRequest request)
    {
        try
        {
            WorkOrderRow? row;

            if (request.Id.HasValue)
            {
                // Update existing row
                row = await _context.WorkOrderRows
                    .FirstOrDefaultAsync(r => r.Id == request.Id.Value && r.WorkOrderHeaderId == headerId);

                if (row == null)
                {
                    return Result<WorkOrderRowDto>.Failure("Row not found");
                }
            }
            else
            {
                // Create new row
                row = new WorkOrderRow
                {
                    Id = Guid.NewGuid(),
                    WorkOrderHeaderId = headerId,
                    CreatedAt = DateTime.UtcNow
                };
                _context.WorkOrderRows.Add(row);
            }

            // Update row fields
            row.RowNumber = request.RowNumber;
            row.PositionPZ = request.PositionPZ;
            row.PositionP1 = request.PositionP1;
            row.PositionR = request.PositionR;
            row.PositionO = request.PositionO;
            row.PositionP2 = request.PositionP2;
            row.Length = request.Length;
            row.Width = request.Width;
            row.Thickness = request.Thickness;
            row.Pieces = request.Pieces;
            row.FromPieces = request.FromPieces;
            row.Description = request.Description;
            row.ProcessingNotes = request.ProcessingNotes;
            row.MaterialDensity = request.MaterialDensity;
            row.UpdatedAt = DateTime.UtcNow;

            // Recalculate fields
            row.RecalculateFields();

            await _context.SaveChangesAsync();

            // Recalculate running totals for all rows in this work order
            await RecalculateRunningTotalsAsync(headerId);

            var dto = MapRowToDto(row);
            return Result<WorkOrderRowDto>.Success(dto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating work order row");
            return Result<WorkOrderRowDto>.Failure("An error occurred while updating the row");
        }
    }

    public async Task<Result<List<WorkOrderRowDto>>> UpdateMultipleRowsAsync(Guid headerId, List<UpdateWorkOrderRowRequest> rows)
    {
        try
        {
            var workOrder = await _context.WorkOrderHeaders
                .Include(w => w.Rows)
                .FirstOrDefaultAsync(w => w.Id == headerId);

            if (workOrder == null)
            {
                return Result<List<WorkOrderRowDto>>.Failure("Work order not found");
            }

            var updatedRows = new List<WorkOrderRow>();

            foreach (var rowRequest in rows)
            {
                WorkOrderRow? row;

                if (rowRequest.Id.HasValue)
                {
                    row = workOrder.Rows.FirstOrDefault(r => r.Id == rowRequest.Id.Value);
                    if (row == null) continue;
                }
                else
                {
                    row = new WorkOrderRow
                    {
                        Id = Guid.NewGuid(),
                        WorkOrderHeaderId = headerId,
                        CreatedAt = DateTime.UtcNow
                    };
                    _context.WorkOrderRows.Add(row);
                    workOrder.Rows.Add(row);
                }

                // Update row fields
                row.RowNumber = rowRequest.RowNumber;
                row.PositionPZ = rowRequest.PositionPZ;
                row.PositionP1 = rowRequest.PositionP1;
                row.PositionR = rowRequest.PositionR;
                row.PositionO = rowRequest.PositionO;
                row.PositionP2 = rowRequest.PositionP2;
                row.Length = rowRequest.Length;
                row.Width = rowRequest.Width;
                row.Thickness = rowRequest.Thickness;
                row.Pieces = rowRequest.Pieces;
                row.FromPieces = rowRequest.FromPieces;
                row.Description = rowRequest.Description;
                row.ProcessingNotes = rowRequest.ProcessingNotes;
                row.MaterialDensity = rowRequest.MaterialDensity;
                row.UpdatedAt = DateTime.UtcNow;

                // Recalculate fields
                row.RecalculateFields();

                updatedRows.Add(row);
            }

            // Update work order header timestamp
            workOrder.UpdatedAt = DateTime.UtcNow;
            workOrder.UpdatedBy = _currentUserService.CurrentUser?.Id;

            await _context.SaveChangesAsync();

            // Recalculate running totals
            await RecalculateRunningTotalsAsync(headerId);

            var dtos = updatedRows.Select(MapRowToDto).ToList();
            return Result<List<WorkOrderRowDto>>.Success(dtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating multiple work order rows");
            return Result<List<WorkOrderRowDto>>.Failure("An error occurred while updating the rows");
        }
    }

    public async Task<Result> DeleteAsync(Guid headerId)
    {
        try
        {
            var workOrder = await _context.WorkOrderHeaders
                .FirstOrDefaultAsync(w => w.Id == headerId);

            if (workOrder == null)
            {
                return Result.Failure("Work order not found");
            }

            _context.WorkOrderHeaders.Remove(workOrder);
            await _context.SaveChangesAsync();

            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting work order {HeaderId}", headerId);
            return Result.Failure("An error occurred while deleting the work order");
        }
    }

    public async Task<Result<byte[]>> ExportToExcelAsync(Guid headerId)
    {
        try
        {
            var workOrder = await _context.WorkOrderHeaders
                .Include(w => w.Rows.OrderBy(r => r.RowNumber))
                .AsNoTracking()
                .FirstOrDefaultAsync(w => w.Id == headerId);

            if (workOrder == null)
            {
                return Result<byte[]>.Failure("Work order not found");
            }

            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Work Order");

            // Set default font and styling
            worksheet.Style.Font.FontName = "Arial";
            worksheet.Style.Font.FontSize = 10;

            // Row 1: Title
            worksheet.Cell("A1").Value = "RADNI NALOG";
            worksheet.Range("A1:U1").Merge().Style
                .Font.SetBold()
                .Font.SetFontSize(14)
                .Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);

            // Row 2: Customer and Object
            worksheet.Cell("A2").Value = "KUPAC:";
            worksheet.Cell("A2").Style.Font.SetBold();
            worksheet.Cell("B2").Value = workOrder.Customer;
            worksheet.Range("B2:K2").Merge();

            worksheet.Cell("L2").Value = "OBJEKAT:";
            worksheet.Cell("L2").Style.Font.SetBold();
            worksheet.Cell("M2").Value = workOrder.Object;
            worksheet.Range("M2:U2").Merge();

            // Row 3: Product
            worksheet.Cell("A3").Value = "PROIZVOD:";
            worksheet.Cell("A3").Style.Font.SetBold();
            worksheet.Cell("B3").Value = workOrder.Product;
            worksheet.Range("B3:U3").Merge();

            // Row 4: Column headers for order details
            worksheet.Cell("A4").Value = "BR. RADNOG NALOGA";
            worksheet.Cell("D4").Value = "NARUDŽBA";
            worksheet.Cell("G4").Value = "OTPREMNICA";
            worksheet.Cell("J4").Value = "DATUM NARUDŽBE";
            worksheet.Cell("M4").Value = "DATUM OTPREME";
            worksheet.Cell("P4").Value = "STRANA";
            worksheet.Range("A4:U4").Style.Font.SetBold().Fill.SetBackgroundColor(XLColor.LightGray);

            // Row 5: Order details values
            worksheet.Cell("A5").Value = workOrder.WorkOrderNumber;
            worksheet.Range("A5:C5").Merge();
            worksheet.Cell("D5").Value = workOrder.OrderReference;
            worksheet.Range("D5:F5").Merge();
            worksheet.Cell("G5").Value = workOrder.DeliveryReference;
            worksheet.Range("G5:I5").Merge();
            worksheet.Cell("J5").Value = workOrder.OrderDate?.ToString("dd.MM.yyyy");
            worksheet.Range("J5:L5").Merge();
            worksheet.Cell("M5").Value = workOrder.DeliveryDate?.ToString("dd.MM.yyyy");
            worksheet.Range("M5:O5").Merge();
            worksheet.Cell("P5").Value = workOrder.PageNumber;
            worksheet.Range("P5:U5").Merge();

            // Row 6: Material, Surface, Cut
            worksheet.Cell("A6").Value = "MATERIJAL";
            worksheet.Cell("H6").Value = "POVRŠINA";
            worksheet.Cell("O6").Value = "REZ";
            worksheet.Range("A6:U6").Style.Font.SetBold().Fill.SetBackgroundColor(XLColor.LightGray);

            // Row 7: Material, Surface, Cut values
            worksheet.Cell("A7").Value = workOrder.Material;
            worksheet.Range("A7:G7").Merge();
            worksheet.Cell("H7").Value = workOrder.Surface;
            worksheet.Range("H7:N7").Merge();
            worksheet.Cell("O7").Value = workOrder.Cut;
            worksheet.Range("O7:U7").Merge();

            // Row 8: Processing and Packing
            worksheet.Cell("A8").Value = "DORADA";
            worksheet.Cell("H8").Value = "PAKOVANJE";
            worksheet.Range("A8:U8").Style.Font.SetBold().Fill.SetBackgroundColor(XLColor.LightGray);

            // Row 9: Processing and Packing values
            worksheet.Cell("A9").Value = workOrder.Processing;
            worksheet.Range("A9:G9").Merge();
            worksheet.Cell("H9").Value = workOrder.Packing;
            worksheet.Range("H9:U9").Merge();

            // Row 10: Notes
            worksheet.Cell("A10").Value = "NAPOMENA";
            worksheet.Range("A10:U10").Style.Font.SetBold().Fill.SetBackgroundColor(XLColor.LightGray);

            // Row 11: Notes value
            worksheet.Cell("A11").Value = workOrder.Notes;
            worksheet.Range("A11:U11").Merge();

            // Row 12: Data table headers
            int headerRow = 12;
            var headers = new[] {
                ("A", "R.BR", 4),
                ("B", "PZ", 5),
                ("C", "P1", 5),
                ("D", "R", 5),
                ("E", "O", 5),
                ("F", "P2", 5),
                ("G", "DUŽINA", 8),
                ("H", "ŠIRINA", 8),
                ("I", "DEBLJINA", 8),
                ("J", "KOM", 6),
                ("K", "OD KOM", 6),
                ("L", "OPIS", 25),
                ("M", "DORADA", 20),
                ("N", "M²", 10),
                ("O", "M² TOT", 10),
                ("P", "M1", 10),
                ("Q", "M³", 10),
                ("R", "TEŽINA kg", 10)
            };

            foreach (var (col, header, width) in headers)
            {
                var cell = worksheet.Cell($"{col}{headerRow}");
                cell.Value = header;
                cell.Style.Font.SetBold()
                    .Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center)
                    .Alignment.SetVertical(XLAlignmentVerticalValues.Center)
                    .Fill.SetBackgroundColor(XLColor.LightBlue)
                    .Border.SetOutsideBorder(XLBorderStyleValues.Thin);
                worksheet.Column(col).Width = width;
            }

            // Data rows (13-42 = 30 rows)
            int currentRow = headerRow + 1;
            foreach (var row in workOrder.Rows.OrderBy(r => r.RowNumber))
            {
                worksheet.Cell($"A{currentRow}").Value = row.RowNumber;
                worksheet.Cell($"A{currentRow}").Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);

                worksheet.Cell($"B{currentRow}").Value = row.PositionPZ;
                worksheet.Cell($"C{currentRow}").Value = row.PositionP1;
                worksheet.Cell($"D{currentRow}").Value = row.PositionR;
                worksheet.Cell($"E{currentRow}").Value = row.PositionO;
                worksheet.Cell($"F{currentRow}").Value = row.PositionP2;

                if (row.Length.HasValue)
                    worksheet.Cell($"G{currentRow}").Value = row.Length.Value;
                if (row.Width.HasValue)
                    worksheet.Cell($"H{currentRow}").Value = row.Width.Value;
                if (row.Thickness.HasValue)
                    worksheet.Cell($"I{currentRow}").Value = row.Thickness.Value;
                if (row.Pieces.HasValue)
                    worksheet.Cell($"J{currentRow}").Value = row.Pieces.Value;
                if (row.FromPieces.HasValue)
                    worksheet.Cell($"K{currentRow}").Value = row.FromPieces.Value;

                worksheet.Cell($"L{currentRow}").Value = row.Description;
                worksheet.Cell($"M{currentRow}").Value = row.ProcessingNotes;

                // Calculated fields with light blue background
                if (row.SquareMeters.HasValue)
                {
                    worksheet.Cell($"N{currentRow}").Value = row.SquareMeters.Value;
                    worksheet.Cell($"N{currentRow}").Style.NumberFormat.Format = "0.00";
                    worksheet.Cell($"N{currentRow}").Style.Fill.SetBackgroundColor(XLColor.LightCyan);
                }
                if (row.SquareMetersTot.HasValue)
                {
                    worksheet.Cell($"O{currentRow}").Value = row.SquareMetersTot.Value;
                    worksheet.Cell($"O{currentRow}").Style.NumberFormat.Format = "0.00";
                    worksheet.Cell($"O{currentRow}").Style.Fill.SetBackgroundColor(XLColor.LightCyan);
                }
                if (row.LinearMeters.HasValue)
                {
                    worksheet.Cell($"P{currentRow}").Value = row.LinearMeters.Value;
                    worksheet.Cell($"P{currentRow}").Style.NumberFormat.Format = "0.00";
                    worksheet.Cell($"P{currentRow}").Style.Fill.SetBackgroundColor(XLColor.LightCyan);
                }
                if (row.CubicMetersTot.HasValue)
                {
                    worksheet.Cell($"Q{currentRow}").Value = row.CubicMetersTot.Value;
                    worksheet.Cell($"Q{currentRow}").Style.NumberFormat.Format = "0.000";
                    worksheet.Cell($"Q{currentRow}").Style.Fill.SetBackgroundColor(XLColor.LightCyan);
                }
                if (row.WeightKg.HasValue)
                {
                    worksheet.Cell($"R{currentRow}").Value = row.WeightKg.Value;
                    worksheet.Cell($"R{currentRow}").Style.NumberFormat.Format = "0.00";
                    worksheet.Cell($"R{currentRow}").Style.Fill.SetBackgroundColor(XLColor.LightCyan);
                }

                // Add borders to all cells in the row
                worksheet.Range($"A{currentRow}:R{currentRow}").Style.Border.SetOutsideBorder(XLBorderStyleValues.Thin);

                currentRow++;
            }

            // Summary section
            currentRow += 1; // Skip a row

            // Group by thickness and calculate totals
            var summaryByThickness = workOrder.Rows
                .Where(r => r.Thickness.HasValue && r.Pieces.HasValue && r.Pieces.Value > 0)
                .GroupBy(r => r.Thickness!.Value)
                .OrderBy(g => g.Key)
                .Select(g => new
                {
                    Thickness = g.Key,
                    TotalPieces = g.Sum(r => r.Pieces!.Value),
                    TotalM2 = g.Sum(r => r.SquareMeters ?? 0),
                    TotalM3 = g.Sum(r => r.CubicMetersTot ?? 0)
                })
                .ToList();

            if (summaryByThickness.Any())
            {
                worksheet.Cell($"A{currentRow}").Value = "UKUPNO PO DEBLJINI";
                worksheet.Range($"A{currentRow}:R{currentRow}").Merge().Style
                    .Font.SetBold()
                    .Fill.SetBackgroundColor(XLColor.LightGray)
                    .Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
                currentRow++;

                worksheet.Cell($"A{currentRow}").Value = "DEBLJINA (mm)";
                worksheet.Cell($"B{currentRow}").Value = "KOMADA";
                worksheet.Cell($"C{currentRow}").Value = "M²";
                worksheet.Cell($"D{currentRow}").Value = "M³";
                worksheet.Range($"A{currentRow}:D{currentRow}").Style.Font.SetBold().Fill.SetBackgroundColor(XLColor.LightBlue);
                currentRow++;

                foreach (var summary in summaryByThickness)
                {
                    worksheet.Cell($"A{currentRow}").Value = summary.Thickness;
                    worksheet.Cell($"B{currentRow}").Value = summary.TotalPieces;
                    worksheet.Cell($"C{currentRow}").Value = summary.TotalM2;
                    worksheet.Cell($"C{currentRow}").Style.NumberFormat.Format = "0.00";
                    worksheet.Cell($"D{currentRow}").Value = summary.TotalM3;
                    worksheet.Cell($"D{currentRow}").Style.NumberFormat.Format = "0.000";
                    currentRow++;
                }
            }

            // Grand totals
            currentRow++;
            worksheet.Cell($"A{currentRow}").Value = "UKUPNO";
            worksheet.Cell($"A{currentRow}").Style.Font.SetBold().Fill.SetBackgroundColor(XLColor.Yellow);
            worksheet.Cell($"B{currentRow}").Value = workOrder.Rows.Sum(r => r.Pieces ?? 0);
            worksheet.Cell($"C{currentRow}").Value = workOrder.Rows.Sum(r => r.SquareMeters ?? 0);
            worksheet.Cell($"C{currentRow}").Style.NumberFormat.Format = "0.00";
            worksheet.Cell($"D{currentRow}").Value = workOrder.Rows.Sum(r => r.LinearMeters ?? 0);
            worksheet.Cell($"D{currentRow}").Style.NumberFormat.Format = "0.00";
            worksheet.Cell($"E{currentRow}").Value = workOrder.Rows.Sum(r => r.CubicMetersTot ?? 0);
            worksheet.Cell($"E{currentRow}").Style.NumberFormat.Format = "0.000";
            worksheet.Cell($"F{currentRow}").Value = workOrder.Rows.Sum(r => r.WeightKg ?? 0);
            worksheet.Cell($"F{currentRow}").Style.NumberFormat.Format = "0.00";

            // Save to memory stream
            using var memoryStream = new MemoryStream();
            workbook.SaveAs(memoryStream);
            var bytes = memoryStream.ToArray();

            return Result<byte[]>.Success(bytes);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error exporting work order to Excel");
            return Result<byte[]>.Failure("An error occurred while exporting to Excel");
        }
    }

    public async Task<Result<string>> ExportToCsvAsync(Guid headerId)
    {
        try
        {
            var workOrder = await _context.WorkOrderHeaders
                .Include(w => w.Rows.OrderBy(r => r.RowNumber))
                .AsNoTracking()
                .FirstOrDefaultAsync(w => w.Id == headerId);

            if (workOrder == null)
            {
                return Result<string>.Failure("Work order not found");
            }

            var csv = new StringBuilder();

            // Header row
            csv.AppendLine("RowNumber,PZ,P1,R,O,P2,Length,Width,Thickness,Pieces,FromPieces,Description,ProcessingNotes,SquareMeters,SquareMetersTot,LinearMeters,CubicMetersTot,WeightKg");

            // Data rows
            foreach (var row in workOrder.Rows)
            {
                csv.AppendLine($"{row.RowNumber}," +
                    $"\"{row.PositionPZ}\"," +
                    $"\"{row.PositionP1}\"," +
                    $"\"{row.PositionR}\"," +
                    $"\"{row.PositionO}\"," +
                    $"\"{row.PositionP2}\"," +
                    $"{row.Length}," +
                    $"{row.Width}," +
                    $"{row.Thickness}," +
                    $"{row.Pieces}," +
                    $"{row.FromPieces}," +
                    $"\"{row.Description?.Replace("\"", "\"\"")}\"," +
                    $"\"{row.ProcessingNotes?.Replace("\"", "\"\"")}\"," +
                    $"{row.SquareMeters}," +
                    $"{row.SquareMetersTot}," +
                    $"{row.LinearMeters}," +
                    $"{row.CubicMetersTot}," +
                    $"{row.WeightKg}");
            }

            return Result<string>.Success(csv.ToString());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error exporting work order to CSV");
            return Result<string>.Failure("An error occurred while exporting to CSV");
        }
    }

    public async Task<Result<WorkOrderHeaderDto>> ImportFromExcelAsync(Guid headerId, Stream excelFileStream)
    {
        try
        {
            // Load the existing work order
            var workOrder = await _context.WorkOrderHeaders
                .Include(w => w.Rows.OrderBy(r => r.RowNumber))
                .FirstOrDefaultAsync(w => w.Id == headerId);

            if (workOrder == null)
            {
                return Result<WorkOrderHeaderDto>.Failure("Work order not found");
            }

            using var workbook = new XLWorkbook(excelFileStream);
            var worksheet = workbook.Worksheets.First();

            var currentUserId = _currentUserService.CurrentUser?.Id;

            // Import header data from the Excel file
            // Row 2: Customer (B2) and Object (M2)
            workOrder.Customer = worksheet.Cell("B2").GetString();
            workOrder.Object = worksheet.Cell("M2").GetString();

            // Row 3: Product (B3)
            workOrder.Product = worksheet.Cell("B3").GetString();

            // Row 5: Order details
            workOrder.WorkOrderNumber = worksheet.Cell("A5").GetString();
            workOrder.OrderReference = worksheet.Cell("D5").GetString();
            workOrder.DeliveryReference = worksheet.Cell("G5").GetString();

            // Parse dates
            var orderDateStr = worksheet.Cell("J5").GetString();
            if (!string.IsNullOrWhiteSpace(orderDateStr) && DateTime.TryParse(orderDateStr, out var orderDate))
            {
                workOrder.OrderDate = orderDate;
            }

            var deliveryDateStr = worksheet.Cell("M5").GetString();
            if (!string.IsNullOrWhiteSpace(deliveryDateStr) && DateTime.TryParse(deliveryDateStr, out var deliveryDate))
            {
                workOrder.DeliveryDate = deliveryDate;
            }

            workOrder.PageNumber = worksheet.Cell("P5").GetString();

            // Row 7: Material (A7), Surface (H7), Cut (O7)
            workOrder.Material = worksheet.Cell("A7").GetString();
            workOrder.Surface = worksheet.Cell("H7").GetString();
            workOrder.Cut = worksheet.Cell("O7").GetString();

            // Row 9: Processing (A9), Packing (H9)
            workOrder.Processing = worksheet.Cell("A9").GetString();
            workOrder.Packing = worksheet.Cell("H9").GetString();

            // Row 11: Notes (A11)
            workOrder.Notes = worksheet.Cell("A11").GetString();

            // Import data rows starting from row 13
            int excelRow = 13;
            var maxRows = Math.Min(30, workOrder.Rows.Count);

            for (int i = 0; i < maxRows; i++)
            {
                var row = workOrder.Rows.OrderBy(r => r.RowNumber).Skip(i).FirstOrDefault();
                if (row == null) break;

                // Read values from Excel
                row.PositionPZ = worksheet.Cell($"B{excelRow}").GetString();
                row.PositionP1 = worksheet.Cell($"C{excelRow}").GetString();
                row.PositionR = worksheet.Cell($"D{excelRow}").GetString();
                row.PositionO = worksheet.Cell($"E{excelRow}").GetString();
                row.PositionP2 = worksheet.Cell($"F{excelRow}").GetString();

                // Parse numeric values
                var lengthStr = worksheet.Cell($"G{excelRow}").GetString();
                row.Length = !string.IsNullOrWhiteSpace(lengthStr) && decimal.TryParse(lengthStr, out var length) ? length : null;

                var widthStr = worksheet.Cell($"H{excelRow}").GetString();
                row.Width = !string.IsNullOrWhiteSpace(widthStr) && decimal.TryParse(widthStr, out var width) ? width : null;

                var thicknessStr = worksheet.Cell($"I{excelRow}").GetString();
                row.Thickness = !string.IsNullOrWhiteSpace(thicknessStr) && decimal.TryParse(thicknessStr, out var thickness) ? thickness : null;

                var piecesStr = worksheet.Cell($"J{excelRow}").GetString();
                row.Pieces = !string.IsNullOrWhiteSpace(piecesStr) && int.TryParse(piecesStr, out var pieces) ? pieces : null;

                var fromPiecesStr = worksheet.Cell($"K{excelRow}").GetString();
                row.FromPieces = !string.IsNullOrWhiteSpace(fromPiecesStr) && int.TryParse(fromPiecesStr, out var fromPieces) ? fromPieces : null;

                row.Description = worksheet.Cell($"L{excelRow}").GetString();
                row.ProcessingNotes = worksheet.Cell($"M{excelRow}").GetString();

                // Recalculate fields
                row.RecalculateFields();
                row.UpdatedAt = DateTime.UtcNow;

                excelRow++;
            }

            // Update work order header timestamp
            workOrder.UpdatedAt = DateTime.UtcNow;
            workOrder.UpdatedBy = currentUserId;

            await _context.SaveChangesAsync();

            // Recalculate running totals
            await RecalculateRunningTotalsAsync(headerId);

            var dto = MapToDto(workOrder);
            return Result<WorkOrderHeaderDto>.Success(dto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error importing work order from Excel");
            return Result<WorkOrderHeaderDto>.Failure("An error occurred while importing from Excel. Please ensure the file matches the expected format.");
        }
    }

    public async Task<bool> ExistsForTaskAsync(Guid taskId)
    {
        return await _context.WorkOrderHeaders.AnyAsync(w => w.TaskId == taskId);
    }

    // Private helper methods

    private async System.Threading.Tasks.Task RecalculateRunningTotalsAsync(Guid headerId)
    {
        var rows = await _context.WorkOrderRows
            .Where(r => r.WorkOrderHeaderId == headerId)
            .OrderBy(r => r.RowNumber)
            .ToListAsync();

        decimal runningM2Total = 0;

        foreach (var row in rows)
        {
            if (row.SquareMeters.HasValue)
            {
                runningM2Total += row.SquareMeters.Value;
            }
            row.SquareMetersTot = runningM2Total;
        }

        await _context.SaveChangesAsync();
    }

    private static WorkOrderHeaderDto MapToDto(WorkOrderHeader workOrder)
    {
        return new WorkOrderHeaderDto
        {
            Id = workOrder.Id,
            TaskId = workOrder.TaskId,
            Customer = workOrder.Customer,
            Object = workOrder.Object,
            Product = workOrder.Product,
            WorkOrderNumber = workOrder.WorkOrderNumber,
            OrderReference = workOrder.OrderReference,
            DeliveryReference = workOrder.DeliveryReference,
            OrderDate = workOrder.OrderDate,
            DeliveryDate = workOrder.DeliveryDate,
            PageNumber = workOrder.PageNumber,
            Material = workOrder.Material,
            Surface = workOrder.Surface,
            Cut = workOrder.Cut,
            Processing = workOrder.Processing,
            Packing = workOrder.Packing,
            Notes = workOrder.Notes,
            CreatedAt = workOrder.CreatedAt,
            UpdatedAt = workOrder.UpdatedAt,
            CreatedBy = workOrder.CreatedBy,
            UpdatedBy = workOrder.UpdatedBy,
            Rows = workOrder.Rows.Select(MapRowToDto).ToList()
        };
    }

    private static WorkOrderRowDto MapRowToDto(WorkOrderRow row)
    {
        return new WorkOrderRowDto
        {
            Id = row.Id,
            WorkOrderHeaderId = row.WorkOrderHeaderId,
            RowNumber = row.RowNumber,
            PositionPZ = row.PositionPZ,
            PositionP1 = row.PositionP1,
            PositionR = row.PositionR,
            PositionO = row.PositionO,
            PositionP2 = row.PositionP2,
            Length = row.Length,
            Width = row.Width,
            Thickness = row.Thickness,
            Pieces = row.Pieces,
            FromPieces = row.FromPieces,
            Description = row.Description,
            ProcessingNotes = row.ProcessingNotes,
            SquareMeters = row.SquareMeters,
            SquareMetersTot = row.SquareMetersTot,
            LinearMeters = row.LinearMeters,
            CubicMetersTot = row.CubicMetersTot,
            WeightKg = row.WeightKg,
            MaterialDensity = row.MaterialDensity,
            CreatedAt = row.CreatedAt,
            UpdatedAt = row.UpdatedAt
        };
    }
}
