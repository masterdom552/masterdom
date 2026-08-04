namespace Masterdom.Platform.ImportExport;

public sealed record SchemaMappingDefinition(
    string MappingCode,
    int Version,
    string Worksheet,
    string Delimiter,
    string Encoding,
    string DateFormat,
    string NumberFormat,
    string DuplicateHandling,
    IReadOnlyCollection<ColumnDefinition> Columns);
