namespace Masterdom.Platform.LanguageSupport;

public interface ILanguageFormatterProvider
{
    string Name { get; }

    string Format(
        string template,
        IReadOnlyDictionary<string, string>? parameters,
        LanguageSettings settings);
}
