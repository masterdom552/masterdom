using Masterdom.Core.Primitives;

namespace Masterdom.Modules.SubsidyOptimization.Domain.Entities.SubsidyOptimization;

public sealed class MeterGroupReference : ValueObject
{
    private MeterGroupReference(string meterGroupCode, IReadOnlyList<Guid> meterIds)
    {
        MeterGroupCode = meterGroupCode;
        MeterIds = meterIds;
    }

    public string MeterGroupCode { get; }

    public IReadOnlyList<Guid> MeterIds { get; }

    public static MeterGroupReference Create(string meterGroupCode, IReadOnlyList<Guid> meterIds)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(meterGroupCode);
        ArgumentNullException.ThrowIfNull(meterIds);

        if (meterIds.Count == 0)
        {
            throw new InvalidOperationException("Meter group must contain at least one meter reference.");
        }

        if (meterIds.Any(x => x == Guid.Empty))
        {
            throw new InvalidOperationException("Meter group contains an empty meter identifier.");
        }

        if (meterIds.Distinct().Count() != meterIds.Count)
        {
            throw new InvalidOperationException("Meter group cannot contain duplicate meter identifiers.");
        }

        return new MeterGroupReference(meterGroupCode.Trim().ToUpperInvariant(), meterIds.ToArray());
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return MeterGroupCode;

        foreach (var meterId in MeterIds.OrderBy(x => x))
        {
            yield return meterId;
        }
    }
}
