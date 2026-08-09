namespace Masterdom.Modules.SubsidyOptimization.Domain.Entities.SubsidyOptimization;

internal static class OptimizationAllocationInvariant
{
    public static decimal CalculateMovementBudget(decimal projectedPropertyUnits, decimal maximumMovementFraction)
    {
        if (projectedPropertyUnits < 0m)
        {
            throw new InvalidOperationException("Projected property consumption cannot be negative.");
        }

        if (maximumMovementFraction is < 0m or > 1m)
        {
            throw new InvalidOperationException("Maximum movement fraction must be between zero and one.");
        }

        return projectedPropertyUnits * maximumMovementFraction;
    }

    public static decimal CalculateTransferredUnits(IEnumerable<decimal> movementUnits)
    {
        ArgumentNullException.ThrowIfNull(movementUnits);
        return movementUnits.Where(x => x > 0m).Sum();
    }

    public static bool IsConserved(
        decimal projectedPropertyUnits,
        IEnumerable<decimal> allocatedUnits,
        decimal tolerance)
    {
        ArgumentNullException.ThrowIfNull(allocatedUnits);
        return decimal.Abs(allocatedUnits.Sum() - projectedPropertyUnits) <= tolerance;
    }

    public static bool IsMovementConserved(IEnumerable<decimal> movementUnits, decimal tolerance)
    {
        ArgumentNullException.ThrowIfNull(movementUnits);
        return decimal.Abs(movementUnits.Sum()) <= tolerance;
    }

    public static bool IsWithinMovementBudget(
        decimal projectedPropertyUnits,
        decimal maximumMovementFraction,
        IEnumerable<decimal> movementUnits,
        decimal tolerance)
    {
        var budget = CalculateMovementBudget(projectedPropertyUnits, maximumMovementFraction);
        return CalculateTransferredUnits(movementUnits) <= budget + tolerance;
    }
}
