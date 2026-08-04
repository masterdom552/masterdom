using Masterdom.Core.Primitives;

namespace Masterdom.Modules.SubsidyOptimization.Domain.Entities.SubsidyOptimization;

public sealed record OptimizationRunId(Guid Value) : EntityId(Value)
{
    public static OptimizationRunId New()
    {
        return new(Guid.CreateVersion7());
    }

    public static OptimizationRunId From(Guid value)
    {
        if (value == Guid.Empty)
        {
            throw new ArgumentException("OptimizationRunId cannot be empty.", nameof(value));
        }

        return new(value);
    }
}
