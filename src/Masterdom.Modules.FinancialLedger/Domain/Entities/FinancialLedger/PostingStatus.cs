using Masterdom.Core.Primitives;

namespace Masterdom.Modules.FinancialLedger.Domain.Entities.FinancialLedger;

public sealed class PostingStatus : ValueObject
{
    public static readonly PostingStatus Prepared = new("Prepared");
    public static readonly PostingStatus Validated = new("Validated");
    public static readonly PostingStatus Draft = new("Draft");
    public static readonly PostingStatus Posted = new("Posted");
    public static readonly PostingStatus Reversed = new("Reversed");
    public static readonly PostingStatus Cancelled = new("Cancelled");
    public static readonly PostingStatus Completed = new("Completed");

    private PostingStatus(string value)
    {
        Value = value;
    }

    public string Value { get; }

    public static PostingStatus Create(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);

        return value.Trim().ToUpperInvariant() switch
        {
            "PREPARED" => Prepared,
            "VALIDATED" => Validated,
            "DRAFT" => Draft,
            "POSTED" => Posted,
            "REVERSED" => Reversed,
            "CANCELLED" => Cancelled,
            "COMPLETED" => Completed,
            _ => new PostingStatus(value.Trim())
        };
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value.ToUpperInvariant();
    }
}
