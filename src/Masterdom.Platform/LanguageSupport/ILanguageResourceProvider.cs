namespace Masterdom.Platform.LanguageSupport;

public interface ILanguageResourceProvider
{
    string Name { get; }

    bool TryGet(string culture, string key, out string value);
}
