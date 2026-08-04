using Masterdom.Core.Primitives;

namespace Masterdom.Modules.Lease.Domain.Entities.Lease;

/// <summary>
/// Represents lease effective date.
/// </summary>
public sealed class EffectiveDate : ValueObject
{
    private EffectiveDate(DateOnly value)
    {
        Value = value;
    }

    public DateOnly Value { get; }

    public static EffectiveDate Create(DateOnly value)
    {
        return new EffectiveDate(value);
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }
}
