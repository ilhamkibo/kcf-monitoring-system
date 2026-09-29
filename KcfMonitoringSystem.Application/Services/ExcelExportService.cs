using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using KcfMonitoringSystem.Application.Dtos;
using KcfMonitoringSystem.Application.Interfaces.Services;

namespace KcfMonitoringSystem.Application.Services;

public class ExcelExportService : IExcelExportService
{
    private readonly string _templatePath;

    public ExcelExportService()
    {
        _templatePath = Path.Combine(AppContext.BaseDirectory, "Templates", "report_template.xlsx");
    }

    public ExcelExportService(string templatePath)
    {
        _templatePath = templatePath;
    }

    public byte[] GenerateReport(
        DandoriSummaryDto dandori,
        List<ProductionRecordDto> productionRecords,
        int? year)
    {
        var targetYear = year ?? DateTime.Now.Year;

        // Copy template to memory stream
        using var templateStream = new FileStream(_templatePath, FileMode.Open, FileAccess.Read);
        using var memStream = new MemoryStream();
        templateStream.CopyTo(memStream);
        memStream.Position = 0;

        using (var doc = SpreadsheetDocument.Open(memStream, true))
        {
            var wbPart = doc.WorkbookPart!;
            var sharedStrings = wbPart.SharedStringTablePart!.SharedStringTable;

            // Process "Achievements Forging" sheet (production records)
            var wsPart = FindWorksheetPart(wbPart, "Achievements Forging");
            if (wsPart != null)
                FillAchievementsSheet(wbPart, wsPart, sharedStrings, productionRecords, targetYear);
        }

        return memStream.ToArray();
    }

    private static WorksheetPart? FindWorksheetPart(WorkbookPart wbPart, string sheetName)
    {
        var sheet = wbPart.Workbook.Sheets!
            .Elements<Sheet>()
            .FirstOrDefault(s => s.Name?.Value == sheetName);
        if (sheet?.Id?.Value == null) return null;
        return wbPart.GetPartById(sheet.Id.Value) as WorksheetPart;
    }

    private static void FillAchievementsSheet(
        WorkbookPart wbPart,
        WorksheetPart wsPart,
        SharedStringTable sharedStrings,
        List<ProductionRecordDto> records,
        int year)
    {
        var sheetData = wsPart.Worksheet.GetFirstChild<SheetData>()!;
        var rows = sheetData.Elements<Row>().ToList();

        // Find section markers {{#records}} and {{/records}}
        int startMarkerRow = -1, endMarkerRow = -1, templateRow = -1;
        foreach (var row in rows)
        {
            var rowNum = (int)row.RowIndex!.Value;
            foreach (var cell in row.Elements<Cell>())
            {
                var val = GetCellValue(cell, sharedStrings);
                if (val == "{{#records}}") { startMarkerRow = rowNum; templateRow = rowNum + 1; }
                else if (val == "{{/records}}") endMarkerRow = rowNum;
            }
        }

        if (startMarkerRow < 0 || endMarkerRow < 0)
            throw new InvalidOperationException("Template markers {{#records}} / {{/records}} not found");

        // Read template row cells (preserve StyleIndex for formatting)
        var templateRowObj = rows.FirstOrDefault(r => r.RowIndex!.Value == (uint)templateRow);
        if (templateRowObj == null)
            throw new InvalidOperationException($"Template row {templateRow} not found");

        var templateCells = new List<(string CellRef, string Value, CellValues? DataType, uint? StyleIndex)>();
        foreach (var cell in templateRowObj.Elements<Cell>())
        {
            templateCells.Add((
                cell.CellReference!.Value!,
                GetCellValue(cell, sharedStrings),
                cell.DataType?.Value,
                cell.StyleIndex?.Value
            ));
        }

        // Prepare alternating row styles (light blue fill for odd rows)
        var altStyleMap = PrepareAlternatingStyles(wbPart, templateCells);

        // Delete markers and template (bottom up to preserve indices)
        DeleteRow(sheetData, endMarkerRow);
        DeleteRow(sheetData, templateRow);
        DeleteRow(sheetData, startMarkerRow);

        // Insert data rows with alternating styles
        int insertIdx = startMarkerRow;
        int dataRowIdx = 0;
        foreach (var record in records)
        {
            var newRow = new Row { RowIndex = (uint)insertIdx };
            sheetData.InsertAt(newRow, insertIdx - 1);
            bool isOddRow = dataRowIdx % 2 == 1;

            WriteRecordCells(newRow, templateCells, record, sharedStrings, altStyleMap, isOddRow, insertIdx);

            dataRowIdx++;
            insertIdx++;
        }

        // Replace {{year}} in title
        ReplaceYearPlaceholder(rows, sharedStrings, year);
    }

    private static void DeleteRow(SheetData sheetData, int rowIndex)
    {
        var row = sheetData.Elements<Row>().FirstOrDefault(r => r.RowIndex!.Value == (uint)rowIndex);
        if (row != null) sheetData.RemoveChild(row);
    }

    private static void WriteRecordCells(
        Row newRow,
        List<(string CellRef, string Value, CellValues? DataType, uint? StyleIndex)> templateCells,
        ProductionRecordDto record,
        SharedStringTable sharedStrings,
        Dictionary<uint, uint> altStyleMap,
        bool isOddRow,
        int rowIdx)
    {
        foreach (var (cellRef, templateValue, dataType, styleIdx) in templateCells)
        {
            var colLetter = GetColumnFromRef(cellRef);
            var newRef = $"{colLetter}{rowIdx}";

            var value = templateValue
                .Replace("{{date}}", record.Date ?? "")
                .Replace("{{machine}}", record.Machine?.Code ?? "")
                .Replace("{{item_no}}", record.Item?.No ?? "")
                .Replace("{{item_name}}", record.Item?.Name ?? "")
                .Replace("{{rpm}}", record.Speed?.Minute.ToString() ?? "")
                .Replace("{{speed_hr}}", record.Speed?.Hour.ToString() ?? "")
                .Replace("{{operators}}", string.Join(", ", record.Operator?.Select(o => o.Name) ?? []))
                .Replace("{{dandori}}", record.Times?.Dandori.ToString() ?? "")
                .Replace("{{running}}", record.Times?.Running.ToString() ?? "")
                .Replace("{{quantity}}", record.ProductQuantity.ToString())
                .Replace("{{op_rate}}", record.OperatingRate.ToString("F2"));

            var newCell = new Cell { CellReference = newRef };

            // Apply alternating style or original style
            if (isOddRow && styleIdx.HasValue && altStyleMap.TryGetValue(styleIdx.Value, out var altIdx))
                newCell.StyleIndex = altIdx;
            else if (styleIdx.HasValue)
                newCell.StyleIndex = styleIdx.Value;

            if (colLetter != "B" && double.TryParse(value, out double numVal))
            {
                newCell.CellValue = new CellValue(numVal);
                newCell.DataType = new EnumValue<CellValues>(CellValues.Number);
            }
            else
            {
                var idx = sharedStrings.Elements<SharedStringItem>().Count();
                sharedStrings.AppendChild(new SharedStringItem(new Text(value)));
                sharedStrings.Count = (uint)(idx + 1);
                newCell.CellValue = new CellValue(idx.ToString());
                newCell.DataType = new EnumValue<CellValues>(CellValues.SharedString);
            }

            newRow.Append(newCell);
        }
    }

    private static Dictionary<uint, uint> PrepareAlternatingStyles(
        WorkbookPart wbPart,
        List<(string CellRef, string Value, CellValues? DataType, uint? StyleIndex)> templateCells)
    {
        var result = new Dictionary<uint, uint>();
        var stylesPart = wbPart.WorkbookStylesPart;
        if (stylesPart?.Stylesheet == null) return result;

        var stylesheet = stylesPart.Stylesheet;
        var fills = stylesheet.Fills!;
        var cellFormats = stylesheet.CellFormats!;

        // Add light blue fill for alternating rows
        var altFill = new Fill(
            new PatternFill(new ForegroundColor { Rgb = "FFD9E1F2" })
            { PatternType = PatternValues.Solid }
        );
        fills.Append(altFill);
        fills.Count = (uint)fills.Elements<Fill>().Count();

        uint altFillIdx = (uint)(fills.Elements<Fill>().Count() - 1);

        // For each unique style index, create an alternate with the new fill
        var origFormats = cellFormats.Elements<CellFormat>().ToList();
        foreach (var (_, _, _, styleIdx) in templateCells)
        {
            if (!styleIdx.HasValue || result.ContainsKey(styleIdx.Value)) continue;

            var origFormat = origFormats[(int)styleIdx.Value];
            var newFormat = (CellFormat)origFormat.CloneNode(true);
            newFormat.FillId = altFillIdx;
            newFormat.ApplyFill = true;

            cellFormats.Append(newFormat);
            uint newIdx = (uint)(cellFormats.Elements<CellFormat>().Count() - 1);
            result[styleIdx.Value] = newIdx;
        }

        cellFormats.Count = (uint)cellFormats.Elements<CellFormat>().Count();
        return result;
    }

    private static void ReplaceYearPlaceholder(List<Row> rows, SharedStringTable sharedStrings, int year)
    {
        foreach (var row in rows)
        {
            foreach (var cell in row.Elements<Cell>())
            {
                var val = GetCellValue(cell, sharedStrings);
                if (val.Contains("{{year}}"))
                {
                    var newVal = val.Replace("{{year}}", year.ToString());
                    var idx = sharedStrings.Elements<SharedStringItem>().Count();
                    sharedStrings.AppendChild(new SharedStringItem(new Text(newVal)));
                    sharedStrings.Count = (uint)(idx + 1);
                    cell.CellValue = new CellValue(idx.ToString());
                    cell.DataType = new EnumValue<CellValues>(CellValues.SharedString);
                }
            }
        }
    }

    private static string GetCellValue(Cell cell, SharedStringTable sharedStrings)
    {
        if (cell.DataType?.Value == CellValues.SharedString)
        {
            var idx = int.Parse(cell.CellValue!.Text);
            return sharedStrings.Elements<SharedStringItem>().ElementAt(idx).InnerText;
        }
        return cell.CellValue?.Text ?? "";
    }

    private static string GetColumnFromRef(string cellRef)
    {
        // Extract column letter(s) from cell reference like "B4" -> "B"
        return new string(cellRef.TakeWhile(c => char.IsLetter(c)).ToArray());
    }
}