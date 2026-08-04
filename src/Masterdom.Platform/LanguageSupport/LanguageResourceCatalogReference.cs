using Masterdom.Platform.Configuration;

namespace Masterdom.Platform.LanguageSupport;

public sealed record LanguageResourceCatalogReference(
    ConfigurationKey ConfigurationKey,
    ConfigurationResolutionRequest ResolutionRequest);
