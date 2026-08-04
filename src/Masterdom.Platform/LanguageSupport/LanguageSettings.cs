namespace Masterdom.Platform.LanguageSupport;

public sealed record LanguageSettings(
    string Culture,
    string Locale,
    IReadOnlyCollection<string> FallbackCultures,
    string DateFormat,
    string TimeFormat,
    string NumberFormat,
    string CurrencyFormat,
    bool UseTwentyFourHourTime)
{
    public static LanguageSettings EnglishBaseline() => new(
        Culture: "en-US",
        Locale: "en-US",
        FallbackCultures: ["en-US"],
        DateFormat: "yyyy-MM-dd",
        TimeFormat: "HH:mm",
        NumberFormat: "N2",
        CurrencyFormat: "C2",
        UseTwentyFourHourTime: true);
}
