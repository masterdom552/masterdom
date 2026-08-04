using Masterdom.Platform.Configuration;

namespace Masterdom.Platform.LanguageSupport;

public sealed record LanguageContext(
    LanguageSettings Settings,
    ConfigurationResolutionRequest ConfigurationRequest,
    LanguageResourceCatalogReference? ResourceCatalogReference);
