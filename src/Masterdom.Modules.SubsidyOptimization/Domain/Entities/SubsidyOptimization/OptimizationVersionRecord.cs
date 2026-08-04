using Masterdom.Core.Primitives;

namespace Masterdom.Modules.SubsidyOptimization.Domain.Entities.SubsidyOptimization;

public sealed class OptimizationVersionRecord : ValueObject
{
    private OptimizationVersionRecord(OptimizationVersion version, DateTime createdAtUtc)
    {
        Version = version;
        CreatedAtUtc = createdAtUtc;
    }

    public OptimizationVersion Version { get; }

    public DateTime CreatedAtUtc { get; }

    public static OptimizationVersionRecord Create(OptimizationVersion version, DateTime createdAtUtc)
    {
        ArgumentNullException.ThrowIfNull(version);

        if (createdAtUtc.Kind != DateTimeKind.Utc)
        {
            throw new InvalidOperationException("Version record timestamp must be UTC.");
        }

        return new OptimizationVersionRecord(version, createdAtUtc);
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Version;
        yield return CreatedAtUtc;
    }
}
