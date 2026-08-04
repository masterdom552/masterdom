using Masterdom.Core.Primitives;

namespace Masterdom.Modules.UtilityRating.Domain.Entities.UtilityRating;

public sealed class RatingBreakdown : ValueObject
{
    private RatingBreakdown(
        IReadOnlyList<RateComponent> components,
        RatedAmount subtotal,
        RatedAmount total)
    {
        Components = components;
        Subtotal = subtotal;
        Total = total;
    }

    public IReadOnlyList<RateComponent> Components { get; }

    public RatedAmount Subtotal { get; }

    public RatedAmount Total { get; }

    public static RatingBreakdown Calculate(RatedUnits ratedUnits, UtilityRate utilityRate)
    {
        ArgumentNullException.ThrowIfNull(ratedUnits);
        ArgumentNullException.ThrowIfNull(utilityRate);

        var variableAmount = decimal.Round(ratedUnits.Value * utilityRate.VariableCharge.RatePerUnit, 2, MidpointRounding.AwayFromZero);

        var variableComponent = VariableChargeAmount.Create(variableAmount);
        var fixedComponent = utilityRate.FixedCharge;
        var adjustmentComponent = utilityRate.AdjustmentComponent;

        var subtotalValue = fixedComponent.Amount + variableComponent.Amount + adjustmentComponent.Amount;
        var subtotal = RatedAmount.Create(subtotalValue);

        var totalValue = Math.Max(subtotalValue, utilityRate.MinimumCharge.Amount);
        var total = RatedAmount.Create(totalValue);

        var components = new List<RateComponent>
        {
            fixedComponent,
            variableComponent,
            adjustmentComponent,
            MinimumChargeApplied.Create(utilityRate.MinimumCharge.Amount)
        };

        return new RatingBreakdown(components, subtotal, total);
    }

    public static RatingBreakdown Create(
        IReadOnlyList<RateComponent> components,
        RatedAmount subtotal,
        RatedAmount total)
    {
        ArgumentNullException.ThrowIfNull(components);
        ArgumentNullException.ThrowIfNull(subtotal);
        ArgumentNullException.ThrowIfNull(total);

        if (components.Count == 0)
        {
            throw new InvalidOperationException("Rating breakdown must include at least one component.");
        }

        return new RatingBreakdown(components, subtotal, total);
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        foreach (var component in Components)
        {
            yield return component;
        }

        yield return Subtotal;
        yield return Total;
    }

    private sealed class VariableChargeAmount : RateComponent
    {
        private VariableChargeAmount(decimal amount)
            : base("VariableCharge", amount)
        {
        }

        public static VariableChargeAmount Create(decimal amount)
        {
            return new VariableChargeAmount(amount);
        }
    }

    private sealed class MinimumChargeApplied : RateComponent
    {
        private MinimumChargeApplied(decimal amount)
            : base("MinimumCharge", amount)
        {
        }

        public static MinimumChargeApplied Create(decimal amount)
        {
            return new MinimumChargeApplied(amount);
        }
    }
}
