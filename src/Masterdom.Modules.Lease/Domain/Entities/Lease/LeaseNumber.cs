using Masterdom.Core.Primitives;

namespace Masterdom.Modules.Lease.Domain.Entities.Lease;

/// <summary>
/// Represents the business number of a lease.
/// </summary>
public sealed class LeaseNumber : ValueObject
{
    private LeaseNumber(string value)
    {
        Value = value;
    }

    public string Value { get; }

    public static LeaseNumber Create(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);

        var normalized = value.Trim().ToUpperInvariant();
        if (normalized.Length > 50)
        {
            throw new ArgumentException("Lease number cannot exceed 50 characters.", nameof(value));
        }

        return new LeaseNumber(normalized);
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString()
    {
        return Value;
    }
}
