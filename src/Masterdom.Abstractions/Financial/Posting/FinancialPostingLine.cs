namespace Masterdom.Abstractions.Financial.Posting;

public sealed record FinancialPostingLine
{
    public required string LineId { get; init; }

    public required FinancialPostingDirection Direction { get; init; }

    public required decimal Amount { get; init; }

    public required string CurrencyCode { get; init; }

    public FinancialPostingReference? Reference { get; init; }

    public FinancialPostingMetadata? Metadata { get; init; }

    public string? Description { get; init; }
}
