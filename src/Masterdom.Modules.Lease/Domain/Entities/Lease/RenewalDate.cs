using Masterdom.Core.Primitives;

namespace Masterdom.Modules.Lease.Domain.Entities.Lease;

/// <summary>
/// Represents the renewal decision date.
/// </summary>
public sealed class RenewalDate : ValueObject
{
    private RenewalDate(DateOnly value)
    {
        Value = value;
    }

    public DateOnly Value { get; }

    public static RenewalDate Create(DateOnly value)
    {
        return new RenewalDate(value);
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }
}
