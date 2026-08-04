namespace Masterdom.Platform.LanguageSupport;

public interface ILanguageContextAccessor
{
    LanguageContext? Current { get; set; }
}
