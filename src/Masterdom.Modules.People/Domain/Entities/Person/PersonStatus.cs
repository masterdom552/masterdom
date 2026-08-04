using Masterdom.Core.Primitives;

namespace Masterdom.Modules.People.Domain.Entities.Person;

/// <summary>
/// Represents the lifecycle status of a person.
/// </summary>
public sealed class PersonStatus : ValueObject
{
    public static readonly PersonStatus Active = new("Active");
    public static readonly PersonStatus Inactive = new("Inactive");
    public static readonly PersonStatus Archived = new("Archived");

    private PersonStatus(string value)
    {
        Value = value;
    }

    /// <summary>
    /// Gets the status value.
    /// </summary>
    public string Value { get; }

    /// <summary>
    /// Creates a person status.
    /// </summary>
    public static PersonStatus Create(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);

        value = value.Trim();

        return value.ToUpperInvariant() switch
        {
            "ACTIVE" => Active,
            "INACTIVE" => Inactive,
            "ARCHIVED" => Archived,
            _ => new PersonStatus(value)
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
