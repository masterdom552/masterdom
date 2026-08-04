namespace Masterdom.Platform.ReadModels;

public sealed class ReadModelProjectionRequest
{
    public ReadModelProjectionRequest(IReadOnlyDictionary<string, string> filters, DateTime asOfUtc)
    {
        Filters = filters;
        AsOfUtc = asOfUtc;
    }

    public IReadOnlyDictionary<string, string> Filters { get; }

    public DateTime AsOfUtc { get; }
}
