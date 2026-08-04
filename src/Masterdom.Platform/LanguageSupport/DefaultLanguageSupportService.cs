using System.Globalization;
using System.Threading;

namespace Masterdom.Platform.LanguageSupport;

public sealed class DefaultLanguageSupportService : ILanguageSupportService
{
    private readonly IEnumerable<ILanguageResourceProvider> _providers;
    private readonly ILanguageSettingsResolver _settingsResolver;
    private readonly ILanguageContextAccessor _contextAccessor;
    private readonly ILanguageFormatterProvider _formatterProvider;

    public DefaultLanguageSupportService(
        IEnumerable<ILanguageResourceProvider> providers,
        ILanguageSettingsResolver settingsResolver,
        ILanguageContextAccessor contextAccessor,
        ILanguageFormatterProvider formatterProvider)
    {
        _providers = providers ?? throw new ArgumentNullException(nameof(providers));
        _settingsResolver = settingsResolver ?? throw new ArgumentNullException(nameof(settingsResolver));
        _contextAccessor = contextAccessor ?? throw new ArgumentNullException(nameof(contextAccessor));
        _formatterProvider = formatterProvider ?? throw new ArgumentNullException(nameof(formatterProvider));
    }

    public LanguageSettings CurrentSettings => _contextAccessor.Current?.Settings ?? LanguageSettings.EnglishBaseline();

    public void SwitchLanguage(LanguageResolutionRequest request)
    {
        var resolved = _settingsResolver.Resolve(request);
        _contextAccessor.Current = new LanguageContext(
            resolved,
            request.ConfigurationRequest,
            request.ResourceCatalogReference);
    }

    public string ResolveText(string key, IReadOnlyDictionary<string, string>? parameters = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);
        EnsureHierarchicalKey(key);

        var settings = CurrentSettings;
        foreach (var culture in BuildFallbackCultures(settings))
        {
            foreach (var provider in _providers)
            {
                if (provider.TryGet(culture, key, out var template))
                {
                    return _formatterProvider.Format(template, parameters, settings);
                }
            }
        }

        return _formatterProvider.Format(key, parameters, settings);
    }

    public string ResolvePluralText(string key, long count, IReadOnlyDictionary<string, string>? parameters = null)
    {
        var pluralKey = count == 1 ? $"{key}.one" : $"{key}.other";
        var mergedParameters = parameters is null
            ? new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            : new Dictionary<string, string>(parameters, StringComparer.OrdinalIgnoreCase);
        mergedParameters["count"] = count.ToString(CultureInfo.InvariantCulture);

        var resolved = ResolveText(pluralKey, mergedParameters);
        return string.Equals(resolved, pluralKey, StringComparison.OrdinalIgnoreCase)
            ? ResolveText(key, mergedParameters)
            : resolved;
    }

    public string FormatDate(DateTime value)
    {
        var culture = ResolveCulture(CurrentSettings.Locale);
        return value.ToString(CurrentSettings.DateFormat, culture);
    }

    public string FormatTime(TimeOnly value)
    {
        var culture = ResolveCulture(CurrentSettings.Locale);
        return value.ToString(CurrentSettings.TimeFormat, culture);
    }

    public string FormatNumber(decimal value)
    {
        var culture = ResolveCulture(CurrentSettings.Locale);
        return value.ToString(CurrentSettings.NumberFormat, culture);
    }

    public string FormatCurrency(decimal value)
    {
        var culture = ResolveCulture(CurrentSettings.Locale);
        return value.ToString(CurrentSettings.CurrencyFormat, culture);
    }

    public DateTime ParseDate(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);
        var culture = ResolveCulture(CurrentSettings.Locale);
        return DateTime.ParseExact(value, CurrentSettings.DateFormat, culture, DateTimeStyles.None);
    }

    public TimeOnly ParseTime(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);
        var culture = ResolveCulture(CurrentSettings.Locale);
        return TimeOnly.ParseExact(value, CurrentSettings.TimeFormat, culture);
    }

    public decimal ParseNumber(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);
        var culture = ResolveCulture(CurrentSettings.Locale);
        return decimal.Parse(value, NumberStyles.Any, culture);
    }

    private static CultureInfo ResolveCulture(string cultureCode)
    {
        return CultureInfo.GetCultureInfo(cultureCode);
    }

    private static IReadOnlyCollection<string> BuildFallbackCultures(LanguageSettings settings)
    {
        var chain = new List<string> { settings.Culture };
        chain.AddRange(settings.FallbackCultures);

        return chain
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    private static void EnsureHierarchicalKey(string key)
    {
        if (!key.Contains('.', StringComparison.Ordinal) ||
            key.StartsWith(".", StringComparison.Ordinal) ||
            key.EndsWith(".", StringComparison.Ordinal) ||
            key.Contains("..", StringComparison.Ordinal))
        {
            throw new InvalidOperationException($"Resource key '{key}' is not hierarchical.");
        }
    }
}
