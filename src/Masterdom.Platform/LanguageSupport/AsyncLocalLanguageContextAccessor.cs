using System.Threading;

namespace Masterdom.Platform.LanguageSupport;

public sealed class AsyncLocalLanguageContextAccessor : ILanguageContextAccessor
{
    private static readonly AsyncLocal<LanguageContext?> CurrentLanguage = new();

    public LanguageContext? Current
    {
        get => CurrentLanguage.Value;
        set => CurrentLanguage.Value = value;
    }
}
