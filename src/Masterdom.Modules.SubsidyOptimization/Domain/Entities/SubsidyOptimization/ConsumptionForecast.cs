using Masterdom.Core.Primitives;

namespace Masterdom.Modules.SubsidyOptimization.Domain.Entities.SubsidyOptimization;

public sealed class ConsumptionForecast : ValueObject
{
    private ConsumptionForecast(decimal baselineConsumption, decimal projectedConsumption, string assumptions)
    {
        BaselineConsumption = baselineConsumption;
        ProjectedConsumption = projectedConsumption;
        Assumptions = assumptions;
    }

    public decimal BaselineConsumption { get; }

    public decimal ProjectedConsumption { get; }

    public string Assumptions { get; }

    public static ConsumptionForecast Create(decimal baselineConsumption, decimal projectedConsumption, string assumptions)
    {
        if (baselineConsumption < 0)
        {
            throw new InvalidOperationException("Baseline consumption cannot be negative.");
        }

        if (projectedConsumption < 0)
        {
            throw new InvalidOperationException("Projected consumption cannot be negative.");
        }

        ArgumentException.ThrowIfNullOrWhiteSpace(assumptions);

        return new ConsumptionForecast(baselineConsumption, projectedConsumption, assumptions.Trim());
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return BaselineConsumption;
        yield return ProjectedConsumption;
        yield return Assumptions;
    }
}
