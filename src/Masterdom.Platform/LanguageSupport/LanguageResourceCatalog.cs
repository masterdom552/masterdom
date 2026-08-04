namespace Masterdom.Platform.LanguageSupport;

public sealed record LanguageResourceCatalog(
    IReadOnlyCollection<LanguageResourceEntry> Entries);
