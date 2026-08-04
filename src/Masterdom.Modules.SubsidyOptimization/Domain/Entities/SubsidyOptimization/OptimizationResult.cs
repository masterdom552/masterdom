using Masterdom.Core.Primitives;

namespace Masterdom.Modules.SubsidyOptimization.Domain.Entities.SubsidyOptimization;

public sealed class OptimizationResult : ValueObject
{
    private OptimizationResult(decimal estimatedSavings, decimal estimatedCost, string summary)
    {
        EstimatedSavings = decimal.Round(estimatedSavings, 2, MidpointRounding.AwayFromZero);
        EstimatedCost = decimal.Round(estimatedCost, 2, MidpointRounding.AwayFromZero);
        Summary = summary;
    }

    public decimal EstimatedSavings { get; }

    public decimal EstimatedCost { get; }

    public string Summary { get; }

    public static OptimizationResult Create(decimal estimatedSavings, decimal estimatedCost, string summary)
    {
        if (estimatedSavings < 0)
        {
            throw new InvalidOperationException("Estimated savings cannot be negative.");
        }

        if (estimatedCost < 0)
        {
            throw new InvalidOperationException("Estimated cost cannot be negative.");
        }

        ArgumentException.ThrowIfNullOrWhiteSpace(summary);

        return new OptimizationResult(estimatedSavings, estimatedCost, summary.Trim());
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return EstimatedSavings;
        yield return EstimatedCost;
        yield return Summary;
    }
}
