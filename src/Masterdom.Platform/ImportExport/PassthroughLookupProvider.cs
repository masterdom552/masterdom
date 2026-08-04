namespace Masterdom.Platform.ImportExport;

public sealed class PassthroughLookupProvider : ILookupProvider
{
    public const string ProviderName = "passthrough";

    public string Name => ProviderName;

    public string Resolve(string value, string lookupRule, IReadOnlyDictionary<string, string> row)
    {
        _ = lookupRule;
        _ = row;
        return value;
    }
}
