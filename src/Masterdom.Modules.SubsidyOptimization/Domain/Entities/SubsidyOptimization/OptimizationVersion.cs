using Masterdom.Core.Primitives;

namespace Masterdom.Modules.SubsidyOptimization.Domain.Entities.SubsidyOptimization;

public sealed class OptimizationVersion : ValueObject
{
    private OptimizationVersion(int value)
    {
        Value = value;
    }

    public int Value { get; }

    public static OptimizationVersion Initial => new(1);

    public static OptimizationVersion Create(int value)
    {
        if (value <= 0)
        {
            throw new InvalidOperationException("Optimization version must be greater than zero.");
        }

        return new OptimizationVersion(value);
    }

    public OptimizationVersion Next()
    {
        return new OptimizationVersion(Value + 1);
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }
}
