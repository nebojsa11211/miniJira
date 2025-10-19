#!/usr/bin/env dotnet-script
#r "nuget: EPPlus, 7.0.0"

using OfficeOpenXml;
using System;
using System.IO;
using System.Text;

// Set license context for EPPlus
ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

var filePath = Args.Count > 0 ? Args[0] : "RadniNalog.xlsx";

if (!File.Exists(filePath))
{
    Console.WriteLine($"File not found: {filePath}");
    return;
}

using var package = new ExcelPackage(new FileInfo(filePath));
var worksheet = package.Workbook.Worksheets[0];

Console.WriteLine("=== Excel File Analysis ===");
Console.WriteLine($"Worksheet Name: {worksheet.Name}");
Console.WriteLine($"Dimensions: {worksheet.Dimension?.Address ?? "Empty"}");
Console.WriteLine($"Rows: {worksheet.Dimension?.Rows ?? 0}");
Console.WriteLine($"Columns: {worksheet.Dimension?.Columns ?? 0}");
Console.WriteLine();

Console.WriteLine("=== Cell Contents (First 50 rows) ===");
var maxRow = Math.Min(worksheet.Dimension?.Rows ?? 0, 50);
var maxCol = worksheet.Dimension?.Columns ?? 0;

for (int row = 1; row <= maxRow; row++)
{
    var rowData = new StringBuilder();
    rowData.Append($"Row {row,3}: ");

    for (int col = 1; col <= maxCol; col++)
    {
        var cell = worksheet.Cells[row, col];
        var value = cell.Value?.ToString() ?? "";

        if (!string.IsNullOrWhiteSpace(value))
        {
            var colLetter = GetColumnLetter(col);
            rowData.Append($"[{colLetter}{row}='{value}'] ");
        }
    }

    var rowStr = rowData.ToString().Trim();
    if (rowStr != $"Row {row,3}:")
    {
        Console.WriteLine(rowStr);
    }
}

Console.WriteLine();
Console.WriteLine("=== Merged Cells ===");
foreach (var merged in worksheet.MergedCells)
{
    Console.WriteLine($"Merged: {merged}");
}

Console.WriteLine();
Console.WriteLine("=== Cell Formatting Info (Sample) ===");
for (int row = 1; row <= Math.Min(10, maxRow); row++)
{
    for (int col = 1; col <= Math.Min(10, maxCol); col++)
    {
        var cell = worksheet.Cells[row, col];
        if (cell.Value != null)
        {
            var colLetter = GetColumnLetter(col);
            Console.WriteLine($"{colLetter}{row}: " +
                $"Bold={cell.Style.Font.Bold}, " +
                $"BgColor={cell.Style.Fill.BackgroundColor.Rgb ?? "none"}, " +
                $"Format={cell.Style.Numberformat.Format}");
        }
    }
}

string GetColumnLetter(int column)
{
    string columnLetter = "";
    while (column > 0)
    {
        int modulo = (column - 1) % 26;
        columnLetter = Convert.ToChar('A' + modulo) + columnLetter;
        column = (column - modulo) / 26;
    }
    return columnLetter;
}
