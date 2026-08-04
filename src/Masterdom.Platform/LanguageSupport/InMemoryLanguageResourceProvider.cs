using System.Collections.Concurrent;
using Masterdom.Platform.Configuration;

namespace Masterdom.Platform.LanguageSupport;

[Obsolete("Use DefaultLanguageResourceProvider.")]
public sealed class InMemoryLanguageResourceProvider : ILanguageResourceProvider
{
    private readonly DefaultLanguageResourceProvider _inner;

    public InMemoryLanguageResourceProvider(
        IReadOnlyDictionary<string, IReadOnlyDictionary<string, string>>? resources = null,
        IBusinessConfigurationCatalog? businessConfigurationCatalog = null,
        ILanguageContextAccessor? contextAccessor = null,
        string name = "default")
    {
        _inner = new DefaultLanguageResourceProvider(resources, businessConfigurationCatalog, contextAccessor, name);
    }

    public string Name => _inner.Name;

    public bool TryGet(string culture, string key, out string value)
    {
        return _inner.TryGet(culture, key, out value);
    }
}
