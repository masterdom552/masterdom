using Masterdom.Core.Primitives;

namespace Masterdom.Modules.Tenancy.Domain.Entities.Tenancy;

/// <summary>
/// Represents the lifecycle status of a tenancy.
/// </summary>
public sealed class TenancyStatus : ValueObject
{
    public static readonly TenancyStatus Active = new("Active");
    public static readonly TenancyStatus Closed = new("Closed");
    public static readonly TenancyStatus Archived = new("Archived");

    private TenancyStatus(string value)
    {
        Value = value;
    }

    public string Value { get; }

    public static TenancyStatus Create(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);

        return value.Trim().ToUpperInvariant() switch
        {
            "ACTIVE" => Active,
            "CLOSED" => Closed,
            "ARCHIVED" => Archived,
            _ => new TenancyStatus(value.Trim())
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
