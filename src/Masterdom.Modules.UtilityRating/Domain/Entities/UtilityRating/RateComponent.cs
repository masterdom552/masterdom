using Masterdom.Core.Primitives;

namespace Masterdom.Modules.UtilityRating.Domain.Entities.UtilityRating;

public abstract class RateComponent : ValueObject
{
    protected RateComponent(string name, decimal amount)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        if (amount < 0)
        {
            throw new InvalidOperationException("Rate component amount cannot be negative.");
        }

        Name = name.Trim();
        Amount = decimal.Round(amount, 2, MidpointRounding.AwayFromZero);
    }

    public string Name { get; }

    public decimal Amount { get; }

    public static RateComponent Create(string name, decimal amount)
    {
        return new StandardRateComponent(name, amount);
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Name;
        yield return Amount;
    }

    private sealed class StandardRateComponent : RateComponent
    {
        public StandardRateComponent(string name, decimal amount)
            : base(name, amount)
        {
        }
    }
}
