namespace Masterdom.Abstractions.Financial.Posting;

public sealed record FinancialPostingReference
{
    public string? EntityType { get; init; }

    public string? EntityId { get; init; }

    public string? DocumentType { get; init; }

    public string? DocumentNumber { get; init; }

    public string? LineId { get; init; }

    public string? ExternalReference { get; init; }

    public string? TenantId { get; init; }
}
