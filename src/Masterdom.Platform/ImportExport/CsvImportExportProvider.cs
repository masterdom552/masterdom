using CsvHelper;
using CsvHelper.Configuration;
using System.Globalization;
using System.Text;

namespace Masterdom.Platform.ImportExport;

public sealed class CsvImportExportProvider : IImportProvider, IExportProvider
{
    public ImportExportFormat Format => ImportExportFormat.Csv;

    public IReadOnlyCollection<IReadOnlyDictionary<string, string>> ReadRows(
        Stream content,
        ImportDefinition definition)
    {
        ArgumentNullException.ThrowIfNull(content);
        ArgumentNullException.ThrowIfNull(definition);

        var config = new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            Delimiter = string.IsNullOrWhiteSpace(definition.CsvDelimiter) ? "," : definition.CsvDelimiter,
            HasHeaderRecord = true,
            BadDataFound = null,
            MissingFieldFound = null,
            TrimOptions = TrimOptions.Trim
        };

        using var reader = new StreamReader(content, ResolveEncoding(definition.TextEncoding), leaveOpen: true);
        using var csv = new CsvReader(reader, config);

        var line = 1;
        while (line < Math.Max(1, definition.HeaderRow))
        {
            if (!csv.Read())
            {
                return [];
            }

            line++;
        }

        csv.Read();
        csv.ReadHeader();
        var headers = csv.HeaderRecord?.ToList() ?? [];
        var aliases = BuildAliasMap(definition.Columns);

        var rows = new List<IReadOnlyDictionary<string, string>>();
        var currentRow = definition.HeaderRow;
        while (csv.Read())
        {
            currentRow++;
            if (currentRow < Math.Max(definition.DataStartRow, definition.HeaderRow + 1))
            {
                continue;
            }

            var row = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            foreach (var header in headers)
            {
                if (string.IsNullOrWhiteSpace(header))
                {
                    continue;
                }

                var key = aliases.TryGetValue(header, out var canonical)
                    ? canonical
                    : header;

                row[key] = csv.GetField(header) ?? string.Empty;
            }

            if (ShouldStopForFooter(definition.FooterHandling, row))
            {
                break;
            }

            rows.Add(row);
        }

        return rows;
    }

    public ExportResult WriteRows(ExportRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        using var stream = new MemoryStream();
        using var writer = new StreamWriter(stream, ResolveEncoding(request.Definition.TextEncoding), leaveOpen: true);
        using var csv = new CsvWriter(writer, new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            Delimiter = string.IsNullOrWhiteSpace(request.Definition.CsvDelimiter) ? "," : request.Definition.CsvDelimiter
        });

        var headers = request.Definition.Columns.Select(x => x.CanonicalName).ToList();
        foreach (var header in headers)
        {
            csv.WriteField(header);
        }

        csv.NextRecord();

        foreach (var row in request.Rows)
        {
            foreach (var header in headers)
            {
                row.TryGetValue(header, out var value);
                csv.WriteField(value ?? string.Empty);
            }

            csv.NextRecord();
        }

        writer.Flush();

        return new ExportResult(
            "text/csv",
            $"{request.JobCode}.csv",
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

    private static Encoding ResolveEncoding(string encoding)
    {
        return string.IsNullOrWhiteSpace(encoding)
            ? Encoding.UTF8
            : Encoding.GetEncoding(encoding);
    }
}
