using Masterdom.Core.Primitives;

namespace Masterdom.Modules.UtilityRating.Domain.Entities.UtilityRating;

public sealed class UtilityRate : ValueObject
{
    private UtilityRate(
        TariffReference tariffReference,
        FixedCharge fixedCharge,
        VariableCharge variableCharge,
        MinimumCharge minimumCharge,
        AdjustmentComponent adjustmentComponent)
    {
        TariffReference = tariffReference;
        FixedCharge = fixedCharge;
        VariableCharge = variableCharge;
        MinimumCharge = minimumCharge;
        AdjustmentComponent = adjustmentComponent;
    }

    public TariffReference TariffReference { get; }

    public FixedCharge FixedCharge { get; }

    public VariableCharge VariableCharge { get; }

    public MinimumCharge MinimumCharge { get; }

    public AdjustmentComponent AdjustmentComponent { get; }

    public static UtilityRate Create(
        TariffReference tariffReference,
        FixedCharge fixedCharge,
        VariableCharge variableCharge,
        MinimumCharge minimumCharge,
        AdjustmentComponent adjustmentComponent)
    {
        ArgumentNullException.ThrowIfNull(tariffReference);
        ArgumentNullException.ThrowIfNull(fixedCharge);
        ArgumentNullException.ThrowIfNull(variableCharge);
        ArgumentNullException.ThrowIfNull(minimumCharge);
        ArgumentNullException.ThrowIfNull(adjustmentComponent);

        return new UtilityRate(tariffReference, fixedCharge, variableCharge, minimumCharge, adjustmentComponent);
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return TariffReference;
        yield return FixedCharge;
        yield return VariableCharge;
        yield return MinimumCharge;
        yield return AdjustmentComponent;
    }
}
