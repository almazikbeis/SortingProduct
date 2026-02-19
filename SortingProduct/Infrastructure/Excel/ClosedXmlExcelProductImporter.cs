using ClosedXML.Excel;
using SortingProduct.Application.Abstractions;
using SortingProduct.Application.Dtos;
using System.Globalization;

namespace SortingProduct.Infrastructure.Excel;

public sealed class ClosedXmlExcelProductImporter : IExcelProductImporter
{
    private static readonly string[] NameHeaders = ["наименование", "название", "товар"];
    private static readonly string[] UnitHeaders = ["единица измерения", "ед. изм.", "ед", "unit"];
    private static readonly string[] PriceHeaders = ["цена за единицу", "цена", "price", "цена за единицу, евро", "цена за единицу евро"];
    private static readonly string[] QuantityHeaders = ["количество", "кол-во", "qty", "количество, шт.", "количество шт"];

    public Task<IReadOnlyList<ImportProductRowDto>> ParseAsync(Stream xlsxStream, CancellationToken cancellationToken)
    {
        using var workbook = new XLWorkbook(xlsxStream);
        var worksheet = workbook.Worksheets.First();

        var headerRowNumber = FindHeaderRow(worksheet);
        if (headerRowNumber is null)
        {
            throw new InvalidOperationException("Header row was not found in xlsx.");
        }

        var headerRow = worksheet.Row(headerRowNumber.Value);
        var columnMap = BuildColumnMap(headerRow);

        var lastRow = worksheet.LastRowUsed()?.RowNumber() ?? headerRowNumber.Value;
        var result = new List<ImportProductRowDto>();

        for (var r = headerRowNumber.Value + 1; r <= lastRow; r++)
        {
            var row = worksheet.Row(r);

            var name = GetString(row.Cell(columnMap.NameColumn));
            var unit = GetString(row.Cell(columnMap.UnitColumn));
            var priceText = GetString(row.Cell(columnMap.PriceColumn));
            var qtyText = GetString(row.Cell(columnMap.QuantityColumn));

            if (string.IsNullOrWhiteSpace(name) && string.IsNullOrWhiteSpace(unit) && string.IsNullOrWhiteSpace(priceText) && string.IsNullOrWhiteSpace(qtyText))
            {
                continue;
            }

            if (string.IsNullOrWhiteSpace(name))
            {
                continue;
            }

            if (!TryParseDecimal(priceText, out var unitPrice))
            {
                continue;
            }

            if (!TryParseInt(qtyText, out var qty))
            {
                continue;
            }

            result.Add(new ImportProductRowDto(name, unit, unitPrice, qty));
        }

        return Task.FromResult<IReadOnlyList<ImportProductRowDto>>(result);
    }

    private static int? FindHeaderRow(IXLWorksheet sheet)
    {
        var lastRow = sheet.LastRowUsed()?.RowNumber() ?? 1;
        var lastCol = sheet.LastColumnUsed()?.ColumnNumber() ?? 1;

        for (var r = 1; r <= Math.Min(lastRow, 50); r++)
        {
            var row = sheet.Row(r);
            var texts = Enumerable.Range(1, lastCol)
                .Select(c => Normalize(GetString(row.Cell(c))))
                .Where(s => !string.IsNullOrWhiteSpace(s))
                .ToList();

            if (texts.Count == 0) continue;

            var hasName = texts.Any(t => NameHeaders.Contains(t));
            var hasUnit = texts.Any(t => UnitHeaders.Contains(t));
            var hasPrice = texts.Any(t => PriceHeaders.Contains(t));
            var hasQty = texts.Any(t => QuantityHeaders.Contains(t));

            if (hasName && hasUnit && hasPrice && hasQty)
            {
                return r;
            }
        }

        return null;
    }

    private static (int NameColumn, int UnitColumn, int PriceColumn, int QuantityColumn) BuildColumnMap(IXLRow headerRow)
    {
        var lastCol = headerRow.Worksheet.LastColumnUsed()?.ColumnNumber() ?? 1;

        int? nameCol = null, unitCol = null, priceCol = null, qtyCol = null;

        for (var c = 1; c <= lastCol; c++)
        {
            var t = Normalize(GetString(headerRow.Cell(c)));
            if (string.IsNullOrWhiteSpace(t)) continue;

            if (nameCol is null && NameHeaders.Contains(t)) nameCol = c;
            if (unitCol is null && UnitHeaders.Contains(t)) unitCol = c;
            if (priceCol is null && PriceHeaders.Contains(t)) priceCol = c;
            if (qtyCol is null && QuantityHeaders.Contains(t)) qtyCol = c;
        }

        if (nameCol is null || unitCol is null || priceCol is null || qtyCol is null)
        {
            throw new InvalidOperationException("Not all required columns were found in header.");
        }

        return (nameCol.Value, unitCol.Value, priceCol.Value, qtyCol.Value);
    }

    private static string GetString(IXLCell cell)
    {
        if (cell.IsEmpty()) return string.Empty;
        return cell.Value.ToString()?.Trim() ?? string.Empty;
    }

    private static string Normalize(string s) =>
        s.Trim().ToLowerInvariant().Replace("  ", " ");

    private static bool TryParseDecimal(string text, out decimal value)
    {
        text = (text ?? string.Empty)
            .Trim()
            .Replace("€", string.Empty)
            .Replace(" ", string.Empty);

        // support 1,5 and 1.5
        if (decimal.TryParse(text, NumberStyles.Number, CultureInfo.GetCultureInfo("ru-RU"), out value)) return true;
        if (decimal.TryParse(text, NumberStyles.Number, CultureInfo.InvariantCulture, out value)) return true;

        value = default;
        return false;
    }

    private static bool TryParseInt(string text, out int value)
    {
        text = (text ?? string.Empty)
            .Trim()
            .Replace(" ", string.Empty);

        if (int.TryParse(text, NumberStyles.Integer, CultureInfo.InvariantCulture, out value)) return true;
        if (int.TryParse(text, NumberStyles.Integer, CultureInfo.GetCultureInfo("ru-RU"), out value)) return true;

        value = default;
        return false;
    }
}
