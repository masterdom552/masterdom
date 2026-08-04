using System.Collections.Concurrent;
using Masterdom.Platform.Configuration;

namespace Masterdom.Platform.LanguageSupport;

public sealed class DefaultLanguageResourceProvider : ILanguageResourceProvider
{
    private readonly IReadOnlyDictionary<string, IReadOnlyDictionary<string, string>> _embeddedResources;
    private readonly ConcurrentDictionary<string, IReadOnlyDictionary<string, string>> _embeddedCache = new(StringComparer.OrdinalIgnoreCase);
    private readonly IBusinessConfigurationCatalog? _businessConfigurationCatalog;
    private readonly ILanguageContextAccessor? _contextAccessor;

    public DefaultLanguageResourceProvider(
        IReadOnlyDictionary<string, IReadOnlyDictionary<string, string>>? embeddedResources = null,
        IBusinessConfigurationCatalog? businessConfigurationCatalog = null,
        ILanguageContextAccessor? contextAccessor = null,
        string name = "default")
    {
        Name = name;
        _embeddedResources = embeddedResources ?? new Dictionary<string, IReadOnlyDictionary<string, string>>(StringComparer.OrdinalIgnoreCase);
        _businessConfigurationCatalog = businessConfigurationCatalog;
        _contextAccessor = contextAccessor;
    }

    public string Name { get; }

    public bool TryGet(string culture, string key, out string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(culture);
        ArgumentException.ThrowIfNullOrWhiteSpace(key);

        var embeddedBundle = _embeddedCache.GetOrAdd(culture, ResolveEmbeddedBundle);
        if (embeddedBundle.TryGetValue(key, out value!))
        {
            return true;
        }

        if (_businessConfigurationCatalog is null || _contextAccessor?.Current?.ResourceCatalogReference is null)
        {
            value = string.Empty;
            return false;
        }

        var reference = _contextAccessor.Current.ResourceCatalogReference;
        var asset = _businessConfigurationCatalog.Resolve<LanguageResourceCatalog>(
            reference.ConfigurationKey,
            reference.ResolutionRequest);

        var matched = asset.Payload.Entries.FirstOrDefault(x =>
            x.Culture.Equals(culture, StringComparison.OrdinalIgnoreCase) &&
            x.Key.Equals(key, StringComparison.OrdinalIgnoreCase));

        if (matched is null)
        {
            value = string.Empty;
            return false;
        }

        value = matched.Value;
        return true;
    }

    private IReadOnlyDictionary<string, string> ResolveEmbeddedBundle(string culture)
    {
        return _embeddedResources.TryGetValue(culture, out var bundle)
            ? bundle
            : new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
    }
}
