using ClosedXML.Excel;

namespace Masterdom.Platform.ImportExport;

public sealed class ExcelImportExportProvider : IImportProvider, IExportProvider
{
    public ImportExportFormat Format => ImportExportFormat.ExcelXlsx;

    public IReadOnlyCollection<IReadOnlyDictionary<string, string>> ReadRows(
        Stream content,
        ImportDefinition definition)
    {
        ArgumentNullException.ThrowIfNull(content);
        ArgumentNullException.ThrowIfNull(definition);

        using var workbook = new XLWorkbook(content);
        var worksheet = string.IsNullOrWhiteSpace(definition.Worksheet)
            ? workbook.Worksheets.First()
            : workbook.Worksheet(definition.Worksheet);

        var headerRow = worksheet.Row(Math.Max(1, definition.HeaderRow));
        if (headerRow.IsEmpty())
        {
            return [];
        }

        if (headerRow is null)
        {
            return [];
        }

        var headerCells = headerRow.CellsUsed().ToList();
        var headers = headerCells.Select(x => x.GetString()).ToList();
        var aliases = BuildAliasMap(definition.Columns);

        var rows = new List<IReadOnlyDictionary<string, string>>();
        foreach (var row in worksheet.RowsUsed().Where(x => x.RowNumber() >= Math.Max(definition.DataStartRow, definition.HeaderRow + 1)))
        {
            var dictionary = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            for (var i = 0; i < headers.Count; i++)
            {
                var header = headers[i];
                if (string.IsNullOrWhiteSpace(header))
                {
                    continue;
                }

                var key = aliases.TryGetValue(header, out var canonical)
                    ? canonical
                    : header;

                dictionary[key] = row.Cell(i + 1).GetFormattedString();
            }

            if (ShouldStopForFooter(definition.FooterHandling, dictionary))
            {
                break;
            }

            rows.Add(dictionary);
        }

        return rows;
    }

    public ExportResult WriteRows(ExportRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        using var workbook = new XLWorkbook();
        var worksheetName = string.IsNullOrWhiteSpace(request.Definition.Worksheet)
            ? "Sheet1"
            : request.Definition.Worksheet;
        var worksheet = workbook.AddWorksheet(worksheetName);

        var headers = request.Definition.Columns.Select(x => x.CanonicalName).ToList();
        for (var i = 0; i < headers.Count; i++)
        {
            worksheet.Cell(1, i + 1).Value = headers[i];
        }

        var rowIndex = 2;
        foreach (var row in request.Rows)
        {
            for (var i = 0; i < headers.Count; i++)
            {
                row.TryGetValue(headers[i], out var value);
                worksheet.Cell(rowIndex, i + 1).Value = value ?? string.Empty;
            }

            rowIndex++;
        }

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);

        return new ExportResult(
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            $"{request.JobCode}.xlsx",
            stream.ToArray());
    }

    private static bool ShouldStopForFooter(string footerHandling, IReadOnlyDictionary<string, string> row)
    {
        if (string.IsNullOrWhiteSpace(footerHandling) || footerHandling.Equals("none", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        if (footerHandling.Equals("stop-at-empty-row", StringComparison.OrdinalIgnoreCase))
        {
            return row.Values.All(string.IsNullOrWhiteSpace);
        }

        if (footerHandling.Equals("stop-at-empty-first-column", StringComparison.OrdinalIgnoreCase))
        {
            var first = row.Values.FirstOrDefault();
            return string.IsNullOrWhiteSpace(first);
        }

        return false;
    }

    private static Dictionary<string, string> BuildAliasMap(IReadOnlyCollection<ColumnDefinition> columns)
    {
        var map = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        foreach (var column in columns)
        {
            map[column.CanonicalName] = column.CanonicalName;
            if (!string.IsNullOrWhiteSpace(column.SourceColumn))
            {
                map[column.SourceColumn] = column.CanonicalName;
            }

            foreach (var alias in column.HeaderAliases)
            {
                map[alias] = column.CanonicalName;
            }
        }

        return map;
    }
}
