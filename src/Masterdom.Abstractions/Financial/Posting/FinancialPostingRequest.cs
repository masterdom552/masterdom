using System.Collections.Immutable;

namespace Masterdom.Abstractions.Financial.Posting;

public sealed record FinancialPostingRequest
{
    public required string RequestId { get; init; }

    public required string CorrelationId { get; init; }

    public string? CausationId { get; init; }

    public required string IdempotencyKey { get; init; }

    public required string TenantId { get; init; }

    public required string CurrencyCode { get; init; }

    public required DateTimeOffset OccurredAt { get; init; }

    public required FinancialTransactionType TransactionType { get; init; }

    public required FinancialDocumentType DocumentType { get; init; }

    public required PostingSource Source { get; init; }

    public FinancialPostingReference? Reference { get; init; }

    public FinancialPostingMetadata? Metadata { get; init; }

    public ImmutableArray<FinancialPostingLine> Lines { get; init; } = ImmutableArray<FinancialPostingLine>.Empty;

    public int ContractVersion { get; init; } = 1;
}
