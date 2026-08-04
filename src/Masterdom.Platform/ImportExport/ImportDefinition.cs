namespace Masterdom.Platform.ImportExport;

public sealed record ImportDefinition(
    string Worksheet,
    int HeaderRow,
    int DataStartRow,
    string FooterHandling,
    IReadOnlyCollection<ColumnDefinition> Columns,
    IReadOnlyDictionary<string, string> DefaultValues,
    string TextEncoding,
    string CsvDelimiter,
    string DateFormat,
    string NumberFormat,
    string DuplicateHandlingStrategy,
    IReadOnlyDictionary<string, string> ValidationRules,
    IReadOnlyDictionary<string, string> TransformationRules,
    IReadOnlyDictionary<string, string> LookupRules,
    string ErrorHandlingPolicy);
