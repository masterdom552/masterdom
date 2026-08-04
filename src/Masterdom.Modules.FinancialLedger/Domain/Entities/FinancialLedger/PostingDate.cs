using Masterdom.Core.Primitives;

namespace Masterdom.Modules.FinancialLedger.Domain.Entities.FinancialLedger;

public sealed class PostingDate : ValueObject
{
    private PostingDate(DateOnly value)
    {
        Value = value;
    }

    public DateOnly Value { get; }

    public static PostingDate Create(DateOnly value)
    {
        return new PostingDate(value);
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }
}
