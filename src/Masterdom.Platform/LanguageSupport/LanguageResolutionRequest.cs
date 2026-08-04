using Masterdom.Platform.Configuration;

namespace Masterdom.Platform.LanguageSupport;

public sealed record LanguageResolutionRequest(
    ConfigurationResolutionRequest ConfigurationRequest,
    string? RequestedCulture,
    string? RequestedLocale,
    LanguageResourceCatalogReference? ResourceCatalogReference = null);
