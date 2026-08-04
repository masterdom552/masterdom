using Masterdom.Core.Primitives;

namespace Masterdom.Modules.PolicyFramework.Domain.Entities.PolicyFramework;

public sealed class PolicyStatus : ValueObject
{
    public static readonly PolicyStatus Draft = new("Draft");
    public static readonly PolicyStatus Active = new("Active");
    public static readonly PolicyStatus Expired = new("Expired");
    public static readonly PolicyStatus Archived = new("Archived");

    private PolicyStatus(string value)
    {
        Value = value;
    }

    public string Value { get; }

    public static PolicyStatus Create(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);

        return value.Trim().ToUpperInvariant() switch
        {
            "DRAFT" => Draft,
            "ACTIVE" => Active,
            "EXPIRED" => Expired,
            "ARCHIVED" => Archived,
            _ => new PolicyStatus(value.Trim())
        };
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value.ToUpperInvariant();
    }
}
