namespace Masterdom.Modules.FinancialLedger.Application.Posting;

internal sealed class JournalNumberingOptions
{
    public string Prefix { get; init; } = "JRN";

    public string Format { get; init; } = "{prefix}-{source}-{date}-{sequence}";

    public int SequenceLength { get; init; } = 12;
}
