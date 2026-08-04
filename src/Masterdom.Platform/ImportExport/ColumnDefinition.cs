namespace Masterdom.Platform.ImportExport;

public sealed record ColumnDefinition(
    string CanonicalName,
    string SourceColumn,
    bool IsRequired,
    IReadOnlyCollection<string> HeaderAliases,
    bool IsOptional,
    string DefaultValue,
    string DataType,
    string DateFormat,
    string NumberFormat,
    string ValidationRule,
    string TransformationRule,
    string LookupRule);
