using Masterdom.Core.Primitives;

namespace Masterdom.Modules.Billing.Domain.Entities.Billing;

/// <summary>
/// Represents snapshot version number.
/// </summary>
public sealed class SnapshotVersion : ValueObject
{
    private SnapshotVersion(int value)
    {
        Value = value;
    }

    public int Value { get; }

    public static SnapshotVersion Create(int value)
    {
        if (value <= 0)
        {
            throw new InvalidOperationException("Snapshot version must be greater than zero.");
        }

        return new SnapshotVersion(value);
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }
}
