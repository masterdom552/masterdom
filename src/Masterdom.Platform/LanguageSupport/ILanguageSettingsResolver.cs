namespace Masterdom.Platform.LanguageSupport;

public interface ILanguageSettingsResolver
{
    LanguageSettings Resolve(LanguageResolutionRequest request);
}
