namespace Masterdom.Modules.FinancialLedger.Application.Posting;

internal sealed class DeterministicJournalNumberGenerator : IJournalNumberGenerator
{
    private readonly JournalNumberingOptions _options;

    public DeterministicJournalNumberGenerator(JournalNumberingOptions? options = null)
    {
        _options = options ?? new JournalNumberingOptions();
    }

    public string Generate(string sourceModule, DateOnly postingDate, string postingReference)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(sourceModule);
        ArgumentException.ThrowIfNullOrWhiteSpace(postingReference);

        var normalizedSource = sourceModule.Trim().ToUpperInvariant();
        var prefix = string.IsNullOrWhiteSpace(_options.Prefix)
            ? "JRN"
            : _options.Prefix.Trim().ToUpperInvariant();

        var format = string.IsNullOrWhiteSpace(_options.Format)
            ? "{prefix}-{source}-{date}-{sequence}"
            : _options.Format;

        var sequence = CreateSequenceToken(ResolveSequenceLength(_options.SequenceLength));

        return format
            .Replace("{prefix}", prefix, StringComparison.OrdinalIgnoreCase)
            .Replace("{source}", normalizedSource, StringComparison.OrdinalIgnoreCase)
            .Replace("{date}", postingDate.ToString("yyyyMMdd"), StringComparison.OrdinalIgnoreCase)
            .Replace("{sequence}", sequence, StringComparison.OrdinalIgnoreCase)
            .Trim();
    }

    private static int ResolveSequenceLength(int requestedLength)
    {
        if (requestedLength < 6)
        {
            return 6;
        }

        if (requestedLength > 32)
        {
            return 32;
        }

        return requestedLength;
    }

    private static string CreateSequenceToken(int sequenceLength)
    {
        var token = Guid.CreateVersion7().ToString("N").ToUpperInvariant();
        return token[^sequenceLength..];
    }
}
