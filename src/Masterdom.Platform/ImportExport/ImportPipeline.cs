namespace Masterdom.Platform.ImportExport;

using Masterdom.Platform.Configuration;

public sealed class ImportPipeline : IImportPipeline
{
    private readonly IImportExportRegistry _registry;
    private readonly IBusinessConfigurationCatalog _configurationCatalog;
    private readonly ILookupProviderRegistry _lookupProviderRegistry;

    public ImportPipeline(
        IImportExportRegistry registry,
        IBusinessConfigurationCatalog configurationCatalog,
        ILookupProviderRegistry? lookupProviderRegistry = null)
    {
        _registry = registry ?? throw new ArgumentNullException(nameof(registry));
        _configurationCatalog = configurationCatalog ?? throw new ArgumentNullException(nameof(configurationCatalog));
        _lookupProviderRegistry = lookupProviderRegistry ?? new LookupProviderRegistry();
    }

    public ImportResult Execute(ImportRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        var definitionAsset = ResolveDefinition(request.DefinitionReference);
        var definition = definitionAsset.Payload;

        var provider = _registry.ResolveImportProvider(request.Format);
        var rows = provider.ReadRows(request.Content, definition).ToList();

        rows = ApplyDefaultValues(rows, definition).ToList();
        rows = ApplyConverters(rows, definition).ToList();
        rows = ApplyTransformationRules(rows, definition).ToList();
        rows = ApplyLookups(rows, definition).ToList();

        var errors = ValidateRows(rows, definition);
        if (!request.ContinueOnRecoverableErrors && errors.Any(x => !x.IsRecoverable))
        {
            rows = [];
        }

        var progress = new ImportProgress(
            TotalRows: rows.Count + errors.Select(x => x.RowNumber).Distinct().Count(),
            ProcessedRows: rows.Count + errors.Count,
            SuccessfulRows: rows.Count,
            FailedRows: errors.Count);

        return new ImportResult(rows, errors, progress);
    }

    private BusinessConfigurationAsset<ImportDefinition> ResolveDefinition(ImportDefinitionCatalogReference reference)
    {
        ArgumentNullException.ThrowIfNull(reference);

        return _configurationCatalog.Resolve<ImportDefinition>(reference.ConfigurationKey, reference.ResolutionRequest);
    }

    private static IReadOnlyCollection<ImportError> ValidateRows(
        IReadOnlyCollection<IReadOnlyDictionary<string, string>> rows,
        ImportDefinition definition)
    {
        var errors = new List<ImportError>();
        var rowNumber = 1;

        foreach (var row in rows)
        {
            foreach (var column in definition.Columns)
            {
                row.TryGetValue(column.CanonicalName, out var value);

                if (column.IsRequired && string.IsNullOrWhiteSpace(value))
                {
                    errors.Add(new ImportError(
                        rowNumber,
                        column.CanonicalName,
                        value ?? string.Empty,
                        $"Required column '{column.CanonicalName}' is missing.",
                        ImportExportSeverity.Error,
                        false));
                    continue;
                }

                if (!string.IsNullOrWhiteSpace(column.ValidationRule) && !string.IsNullOrWhiteSpace(value))
                {
                    var isValid = column.ValidationRule switch
                    {
                        "number" => decimal.TryParse(value, out _),
                        "date" => DateTime.TryParse(value, out _),
                        "non-empty" => !string.IsNullOrWhiteSpace(value),
                        _ => true
                    };

                    if (!isValid)
                    {
                        errors.Add(new ImportError(
                            rowNumber,
                            column.CanonicalName,
                            value,
                            $"Validation rule '{column.ValidationRule}' failed for value '{value}'.",
                            ImportExportSeverity.Error,
                            true));
                    }
                }
            }

            rowNumber++;
        }

        return errors;
    }

    private static IReadOnlyCollection<IReadOnlyDictionary<string, string>> ApplyDefaultValues(
        IReadOnlyCollection<IReadOnlyDictionary<string, string>> rows,
        ImportDefinition definition)
    {
        var updated = new List<IReadOnlyDictionary<string, string>>();

        foreach (var row in rows)
        {
            var buffer = new Dictionary<string, string>(row, StringComparer.OrdinalIgnoreCase);

            foreach (var column in definition.Columns)
            {
                if (!buffer.TryGetValue(column.CanonicalName, out var existing) || string.IsNullOrWhiteSpace(existing))
                {
                    if (!string.IsNullOrWhiteSpace(column.DefaultValue))
                    {
                        buffer[column.CanonicalName] = column.DefaultValue;
                    }
                    else if (definition.DefaultValues.TryGetValue(column.CanonicalName, out var defaultValue))
                    {
                        buffer[column.CanonicalName] = defaultValue;
                    }
                }
            }

            updated.Add(buffer);
        }

        return updated;
    }

    private static IReadOnlyCollection<IReadOnlyDictionary<string, string>> ApplyTransformationRules(
        IReadOnlyCollection<IReadOnlyDictionary<string, string>> rows,
        ImportDefinition definition)
    {
        var updated = new List<IReadOnlyDictionary<string, string>>();

        foreach (var row in rows)
        {
            var buffer = new Dictionary<string, string>(row, StringComparer.OrdinalIgnoreCase);

            foreach (var column in definition.Columns)
            {
                if (!buffer.TryGetValue(column.CanonicalName, out var value) || string.IsNullOrWhiteSpace(value))
                {
                    continue;
                }

                var rule = string.IsNullOrWhiteSpace(column.TransformationRule)
                    ? (definition.TransformationRules.TryGetValue(column.CanonicalName, out var configured) ? configured : string.Empty)
                    : column.TransformationRule;

                if (string.IsNullOrWhiteSpace(rule))
                {
                    continue;
                }

                buffer[column.CanonicalName] = rule switch
                {
                    "trim" => value.Trim(),
                    "upper" => value.Trim().ToUpperInvariant(),
                    "lower" => value.Trim().ToLowerInvariant(),
                    _ => value
                };
            }

            updated.Add(buffer);
        }

        return updated;
    }

    private static IReadOnlyCollection<IReadOnlyDictionary<string, string>> ApplyConverters(
        IReadOnlyCollection<IReadOnlyDictionary<string, string>> rows,
        ImportDefinition definition)
    {
        var updated = new List<IReadOnlyDictionary<string, string>>();

        foreach (var row in rows)
        {
            var buffer = new Dictionary<string, string>(row, StringComparer.OrdinalIgnoreCase);

            foreach (var column in definition.Columns)
            {
                if (!buffer.TryGetValue(column.CanonicalName, out var value) || string.IsNullOrWhiteSpace(value))
                {
                    continue;
                }

                buffer[column.CanonicalName] = column.DataType.ToLowerInvariant() switch
                {
                    "date" => ConvertDate(value, column.DateFormat, definition.DateFormat),
                    "number" => ConvertNumber(value, column.NumberFormat, definition.NumberFormat),
                    "boolean" => ConvertBoolean(value),
                    _ => value
                };
            }

            updated.Add(buffer);
        }

        return updated;
    }

    private IReadOnlyCollection<IReadOnlyDictionary<string, string>> ApplyLookups(
        IReadOnlyCollection<IReadOnlyDictionary<string, string>> rows,
        ImportDefinition definition)
    {
        var updated = new List<IReadOnlyDictionary<string, string>>();

        foreach (var row in rows)
        {
            var buffer = new Dictionary<string, string>(row, StringComparer.OrdinalIgnoreCase);

            foreach (var column in definition.Columns)
            {
                if (!buffer.TryGetValue(column.CanonicalName, out var value) || string.IsNullOrWhiteSpace(value))
                {
                    continue;
                }

                var lookupRule = string.IsNullOrWhiteSpace(column.LookupRule)
                    ? (definition.LookupRules.TryGetValue(column.CanonicalName, out var configured) ? configured : string.Empty)
                    : column.LookupRule;

                if (string.IsNullOrWhiteSpace(lookupRule))
                {
                    continue;
                }

                var providerName = lookupRule.Contains(':', StringComparison.Ordinal)
                    ? lookupRule.Split(':', 2, StringSplitOptions.TrimEntries)[0]
                    : PassthroughLookupProvider.ProviderName;

                var provider = _lookupProviderRegistry.Resolve(providerName);
                buffer[column.CanonicalName] = provider.Resolve(value, lookupRule, buffer);
            }

            updated.Add(buffer);
        }

        return updated;
    }

    private static string ConvertDate(string value, string columnFormat, string defaultFormat)
    {
        var format = string.IsNullOrWhiteSpace(columnFormat) ? defaultFormat : columnFormat;
        if (!string.IsNullOrWhiteSpace(format) && DateTime.TryParseExact(value, format, null, System.Globalization.DateTimeStyles.None, out var parsed))
        {
            return parsed.ToString("yyyy-MM-dd");
        }

        return DateTime.TryParse(value, out parsed)
            ? parsed.ToString("yyyy-MM-dd")
            : value;
    }

    private static string ConvertNumber(string value, string columnFormat, string defaultFormat)
    {
        _ = columnFormat;
        _ = defaultFormat;

        return decimal.TryParse(value, out var parsed)
            ? parsed.ToString(System.Globalization.CultureInfo.InvariantCulture)
            : value;
    }

    private static string ConvertBoolean(string value)
    {
        if (bool.TryParse(value, out var parsed))
        {
            return parsed ? "true" : "false";
        }

        if (value.Equals("1", StringComparison.OrdinalIgnoreCase) || value.Equals("yes", StringComparison.OrdinalIgnoreCase))
        {
            return "true";
        }

        if (value.Equals("0", StringComparison.OrdinalIgnoreCase) || value.Equals("no", StringComparison.OrdinalIgnoreCase))
        {
            return "false";
        }

        return value;
    }
}
