using Masterdom.Core.Primitives;

namespace Masterdom.Modules.People.Domain.Entities.Person;

/// <summary>
/// Represents a person's marital status.
/// </summary>
public sealed class MaritalStatus : ValueObject
{
    public static readonly MaritalStatus Single = new("Single");
    public static readonly MaritalStatus Married = new("Married");
    public static readonly MaritalStatus Divorced = new("Divorced");
    public static readonly MaritalStatus Widowed = new("Widowed");
    public static readonly MaritalStatus Separated = new("Separated");
    public static readonly MaritalStatus Other = new("Other");

    private MaritalStatus(string value)
    {
        Value = value;
    }

    /// <summary>
    /// Gets the marital status.
    /// </summary>
    public string Value { get; }

    /// <summary>
    /// Creates a marital status.
    /// </summary>
    public static MaritalStatus Create(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);

        value = value.Trim();

        return value.ToUpperInvariant() switch
        {
            "SINGLE" => Single,
            "MARRIED" => Married,
            "DIVORCED" => Divorced,
            "WIDOWED" => Widowed,
            "SEPARATED" => Separated,
            "OTHER" => Other,
            _ => new MaritalStatus(value)
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
