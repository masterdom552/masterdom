using Masterdom.Core.Primitives;

namespace Masterdom.Modules.Tenancy.Domain.Entities.Tenancy;

/// <summary>
/// Represents an effective date used in tenancy lifecycle transitions.
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
