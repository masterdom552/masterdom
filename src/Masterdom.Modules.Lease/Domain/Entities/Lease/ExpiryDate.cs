using Masterdom.Core.Primitives;

namespace Masterdom.Modules.Lease.Domain.Entities.Lease;

/// <summary>
/// Represents lease expiry date.
/// </summary>
public sealed class ExpiryDate : ValueObject
{
    private ExpiryDate(DateOnly value)
    {
        Value = value;
    }

    public DateOnly Value { get; }

    public static ExpiryDate Create(DateOnly value)
    {
        return new ExpiryDate(value);
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }
}
