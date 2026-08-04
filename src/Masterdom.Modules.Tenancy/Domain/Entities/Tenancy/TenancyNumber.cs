using Masterdom.Core.Primitives;

namespace Masterdom.Modules.Tenancy.Domain.Entities.Tenancy;

/// <summary>
/// Represents the business number of a tenancy.
/// </summary>
public sealed class TenancyNumber : ValueObject
{
    private TenancyNumber(string value)
    {
        Value = value;
    }

    public string Value { get; }

    public static TenancyNumber Create(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);

        var normalized = value.Trim().ToUpperInvariant();
        if (normalized.Length > 50)
        {
            throw new ArgumentException("Tenancy number cannot exceed 50 characters.", nameof(value));
        }

        return new TenancyNumber(normalized);
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
