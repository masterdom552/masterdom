using Masterdom.Core.Primitives;

namespace Masterdom.Modules.FinancialLedger.Domain.Entities.FinancialLedger;

public sealed class MoneyAmount : ValueObject
{
    private MoneyAmount(decimal value)
    {
        Value = value;
    }

    public decimal Value { get; }

    public static MoneyAmount Create(decimal value)
    {
        if (value < 0m)
        {
            throw new InvalidOperationException("Money amount cannot be negative.");
        }

        return new MoneyAmount(decimal.Round(value, 2, MidpointRounding.AwayFromZero));
    }

    public static MoneyAmount Zero()
    {
        return new MoneyAmount(0m);
    }

    public MoneyAmount Add(MoneyAmount other)
    {
        ArgumentNullException.ThrowIfNull(other);
        return Create(Value + other.Value);
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }
}
