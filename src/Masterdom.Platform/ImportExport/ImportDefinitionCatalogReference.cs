using Masterdom.Platform.Configuration;

namespace Masterdom.Platform.ImportExport;

public sealed record ImportDefinitionCatalogReference(
    ConfigurationKey ConfigurationKey,
    ConfigurationResolutionRequest ResolutionRequest);
