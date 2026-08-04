namespace Masterdom.Platform.LanguageSupport;

public interface ILanguageSupportService
{
    LanguageSettings CurrentSettings { get; }

    void SwitchLanguage(LanguageResolutionRequest request);

    string ResolveText(string key, IReadOnlyDictionary<string, string>? parameters = null);

    string ResolvePluralText(string key, long count, IReadOnlyDictionary<string, string>? parameters = null);

    string FormatDate(DateTime value);

    string FormatTime(TimeOnly value);

    string FormatNumber(decimal value);

    string FormatCurrency(decimal value);

    DateTime ParseDate(string value);

    TimeOnly ParseTime(string value);

    decimal ParseNumber(string value);
}
