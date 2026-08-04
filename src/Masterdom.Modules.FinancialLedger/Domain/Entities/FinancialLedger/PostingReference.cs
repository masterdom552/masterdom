using Masterdom.Core.Primitives;

namespace Masterdom.Modules.FinancialLedger.Domain.Entities.FinancialLedger;

public sealed class PostingReference : ValueObject
{
    private PostingReference(string value)
    {
        Value = value;
    }

    public string Value { get; }

    public static PostingReference Create(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);
        return new PostingReference(value.Trim());
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value.ToUpperInvariant();
    }
}
