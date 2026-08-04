using Masterdom.Core.Primitives;

namespace Masterdom.Modules.Lease.Domain.Entities.Lease;

/// <summary>
/// Represents lease classification.
/// </summary>
public sealed class LeaseType : ValueObject
{
    public static readonly LeaseType Residential = new("Residential");
    public static readonly LeaseType Commercial = new("Commercial");
    public static readonly LeaseType Corporate = new("Corporate");

    private LeaseType(string value)
    {
        Value = value;
    }

    public string Value { get; }

    public static LeaseType Create(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);

        return value.Trim().ToUpperInvariant() switch
        {
            "RESIDENTIAL" => Residential,
            "COMMERCIAL" => Commercial,
            "CORPORATE" => Corporate,
            _ => new LeaseType(value.Trim())
        };
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value.ToUpperInvariant();
    }

    public override string ToString()
    {
        return Value;
    }
}
