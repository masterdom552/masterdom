using System.Collections.Immutable;

namespace Masterdom.Abstractions.Financial.Posting;

public sealed record FinancialPostingResult
{
    public required string RequestId { get; init; }

    public required string CorrelationId { get; init; }

    public string? CausationId { get; init; }

    public required string IdempotencyKey { get; init; }

    public required string TenantId { get; init; }

    public required string CurrencyCode { get; init; }

    public required DateTimeOffset ProcessedAt { get; init; }

    public required FinancialPostingStatus Status { get; init; }

    public string? PostingId { get; init; }

    public string? BatchId { get; init; }

    public string? Message { get; init; }

    public FinancialPostingReference? Reference { get; init; }

    public FinancialPostingMetadata? Metadata { get; init; }

    public ImmutableArray<FinancialPostingLine> Lines { get; init; } = ImmutableArray<FinancialPostingLine>.Empty;

    public int ContractVersion { get; init; } = 1;
}
