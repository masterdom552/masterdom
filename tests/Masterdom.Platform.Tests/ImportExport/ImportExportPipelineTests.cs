using Masterdom.Platform.ImportExport;
using Masterdom.Platform.Configuration;
using System.Text.Json;

namespace Masterdom.Platform.Tests.ImportExport;

public sealed class ImportExportPipelineTests
{
    [Fact]
    public void CsvProvider_ShouldImportAndExport_WithDefinitionAliases()
    {
        var definition = BuildDefinition();
        var configurationResolver = new FixedConfigurationResolver(
            BuildConfigurationAsset(definition, definitionId: "generic", version: 1));
        var configurationCatalog = new BusinessConfigurationCatalog(configurationResolver);

        var definitionReference = new ImportDefinitionCatalogReference(
            new ConfigurationKey("import-definitions.generic.v1"),
            new ConfigurationResolutionRequest
            {
                ModuleId = "subsidy-optimization",
                AsOfUtc = DateTime.UtcNow
            });

        var csv = "Field A,Field B,Amount\nA-1,B-1,1200\n";
        using var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(csv));

        var provider = new CsvImportExportProvider();
        var registry = new ImportExportRegistry([provider], [provider]);
        var importPipeline = new ImportPipeline(registry, configurationCatalog);
        var exportPipeline = new ExportPipeline(registry);

        var importResult = importPipeline.Execute(new ImportRequest("generic-import", ImportExportFormat.Csv, stream, definitionReference, true));

        Assert.Single(importResult.Rows);
        Assert.Equal("A-1", importResult.Rows.First()["field_a"]);
        Assert.Equal("B-1", importResult.Rows.First()["field_b"]);
        Assert.Empty(importResult.Errors);

        var exportResult = exportPipeline.Execute(new ExportRequest("generic-export", ImportExportFormat.Csv, definition, importResult.Rows));

        Assert.Equal("text/csv", exportResult.MimeType);
        Assert.True(exportResult.Content.Length > 0);
    }

    [Fact]
    public void ExcelProvider_ShouldImportAndExport_WithDefinitionAliases()
    {
        var definition = BuildDefinition() with { Worksheet = "SheetA" };
        var configurationResolver = new FixedConfigurationResolver(
            BuildConfigurationAsset(definition, definitionId: "generic", version: 2));
        var configurationCatalog = new BusinessConfigurationCatalog(configurationResolver);

        var definitionReference = new ImportDefinitionCatalogReference(
            new ConfigurationKey("import-definitions.generic.v2"),
            new ConfigurationResolutionRequest
            {
                ModuleId = "documents",
                AsOfUtc = DateTime.UtcNow
            });

        var provider = new ExcelImportExportProvider();
        var registry = new ImportExportRegistry([provider], [provider]);
        var importPipeline = new ImportPipeline(registry, configurationCatalog);
        var exportPipeline = new ExportPipeline(registry);

        var rows = new List<IReadOnlyDictionary<string, string>>
        {
            new Dictionary<string, string>
            {
                ["field_a"] = "A-200",
                ["field_b"] = "B-200",
                ["amount"] = "1500"
            }
        };

        var export = exportPipeline.Execute(new ExportRequest("generic-export-xlsx", ImportExportFormat.ExcelXlsx, definition, rows));
        Assert.Equal("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", export.MimeType);

        using var stream = new MemoryStream(export.Content);
        var import = importPipeline.Execute(new ImportRequest("generic-import-xlsx", ImportExportFormat.ExcelXlsx, stream, definitionReference, true));

        Assert.Single(import.Rows);
        Assert.Equal("A-200", import.Rows.First()["field_a"]);
        Assert.Empty(import.Errors);
    }

    [Fact]
    public void ImportPipeline_ShouldReportRowColumnValueValidationError()
    {
        var definition = BuildDefinition();
        var configurationResolver = new FixedConfigurationResolver(
            BuildConfigurationAsset(definition, definitionId: "generic", version: 3));
        var configurationCatalog = new BusinessConfigurationCatalog(configurationResolver);

        var definitionReference = new ImportDefinitionCatalogReference(
            new ConfigurationKey("import-definitions.generic.v3"),
            new ConfigurationResolutionRequest
            {
                ModuleId = "reporting",
                AsOfUtc = DateTime.UtcNow
            });

        var csv = "Field A,Field B,Amount\nA-1,B-1,not-a-number\n";
        using var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(csv));

        var provider = new CsvImportExportProvider();
        var pipeline = new ImportPipeline(new ImportExportRegistry([provider], [provider]), configurationCatalog);

        var result = pipeline.Execute(new ImportRequest("validation-import", ImportExportFormat.Csv, stream, definitionReference, true));

        Assert.Single(result.Errors);
        Assert.Equal(1, result.Errors.First().RowNumber);
        Assert.Equal("amount", result.Errors.First().Column);
        Assert.Equal("not-a-number", result.Errors.First().OffendingValue);
        Assert.Equal(ImportExportSeverity.Error, result.Errors.First().Severity);
        Assert.True(result.Errors.First().IsRecoverable);
    }

    [Fact]
    public void ImportPipeline_ShouldExecuteHistoricalVersionFromConfiguration()
    {
        var v1 = BuildDefinition() with
        {
            Columns =
            [
                new ColumnDefinition("field_a", "Field A", true, ["Field A"], false, string.Empty, "text", string.Empty, string.Empty, "non-empty", "upper", string.Empty),
                new ColumnDefinition("field_b", "Field B", true, ["Field B"], false, string.Empty, "text", string.Empty, string.Empty, "non-empty", "trim", string.Empty),
                new ColumnDefinition("amount", "Amount", true, ["Amount"], false, "0", "number", string.Empty, "0.##", "number", string.Empty, string.Empty)
            ]
        };

        var resolver = new FixedConfigurationResolver(BuildConfigurationAsset(v1, definitionId: "historical", version: 1));
        var configurationCatalog = new BusinessConfigurationCatalog(resolver);
        var reference = new ImportDefinitionCatalogReference(
            new ConfigurationKey("import-definitions.historical.v1"),
            new ConfigurationResolutionRequest
            {
                ModuleId = "metering",
                AsOfUtc = DateTime.UtcNow.AddYears(-1)
            });

        var csv = "Field A,Field B,Amount\na-1,b-1,1200\n";
        using var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(csv));

        var provider = new CsvImportExportProvider();
        var pipeline = new ImportPipeline(new ImportExportRegistry([provider], [provider]), configurationCatalog);

        var result = pipeline.Execute(new ImportRequest("historical-import", ImportExportFormat.Csv, stream, reference, true));

        Assert.Single(result.Rows);
        Assert.Equal("A-1", result.Rows.First()["field_a"]);
    }

    private static ImportDefinition BuildDefinition()
    {
        return new ImportDefinition(
            "Sheet1",
            1,
            2,
            "none",
            [
                new ColumnDefinition("field_a", "Field A", true, ["Field A", "A"], false, string.Empty, "text", string.Empty, string.Empty, "non-empty", "trim", string.Empty),
                new ColumnDefinition("field_b", "Field B", true, ["Field B", "B"], false, string.Empty, "text", string.Empty, string.Empty, "non-empty", "trim", string.Empty),
                new ColumnDefinition("amount", "Amount", true, ["Amount"], false, "0", "number", string.Empty, "0.##", "number", string.Empty, string.Empty),
                new ColumnDefinition("optional_tag", "Optional Tag", false, ["Optional Tag"], true, "N/A", "text", string.Empty, string.Empty, string.Empty, string.Empty, string.Empty)
            ],
            new Dictionary<string, string> { ["optional_tag"] = "N/A" },
            "utf-8",
            ",",
            "yyyy-MM-dd",
            "0.##",
            "merge",
            new Dictionary<string, string>(),
            new Dictionary<string, string> { ["field_a"] = "trim", ["field_b"] = "trim" },
            new Dictionary<string, string>(),
            "continue-recoverable");
    }

    private static BusinessConfigurationAsset<ImportDefinition> BuildConfigurationAsset(ImportDefinition definition, string definitionId, int version)
    {
        return new BusinessConfigurationAsset<ImportDefinition>(
            new BusinessConfigurationMetadata(
                definitionId,
                "Generic Import Definition",
                version,
                BusinessConfigurationStatus.Active,
                "Generic import definition for platform tests.",
                DateTime.UtcNow.AddYears(-2),
                null,
                "superuser",
                "superuser",
                DateTime.UtcNow.AddYears(-2),
                DateTime.UtcNow,
                new Dictionary<string, string> { ["audit"] = "test" }),
            definition);
    }

    private sealed class FixedConfigurationResolver : IConfigurationResolver
    {
        private readonly string _serialized;

        public FixedConfigurationResolver(BusinessConfigurationAsset<ImportDefinition> asset)
        {
            _serialized = JsonSerializer.Serialize(asset);
        }

        public ConfigurationResolutionResult Resolve(ConfigurationKey key, ConfigurationResolutionRequest request)
        {
            _ = key;
            _ = request;

            return new ConfigurationResolutionResult
            {
                IsDefault = false,
                Record = new ConfigurationRecord(
                    new ConfigurationId(Guid.NewGuid()),
                    new ConfigurationKey("import-definitions.test"),
                    ConfigurationScope.Module("test"),
                    new ConfigurationVersion(1),
                    new ConfigurationValue(_serialized),
                    new EffectivePeriod(DateTime.UnixEpoch, null),
                    "test",
                    "test",
                    DateTime.UtcNow)
            };
        }
    }
}
