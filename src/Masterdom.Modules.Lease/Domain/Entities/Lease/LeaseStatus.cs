using Masterdom.Core.Primitives;

namespace Masterdom.Modules.Lease.Domain.Entities.Lease;

/// <summary>
/// Represents lease lifecycle status.
/// </summary>
public sealed class LeaseStatus : ValueObject
{
    public static readonly LeaseStatus Draft = new("Draft");
    public static readonly LeaseStatus Active = new("Active");
    public static readonly LeaseStatus Terminated = new("Terminated");
    public static readonly LeaseStatus Expired = new("Expired");
    public static readonly LeaseStatus Closed = new("Closed");

    private LeaseStatus(string value)
    {
        Value = value;
    }

    public string Value { get; }

    public static LeaseStatus Create(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);

        return value.Trim().ToUpperInvariant() switch
        {
            "DRAFT" => Draft,
            "ACTIVE" => Active,
            "TERMINATED" => Terminated,
            "EXPIRED" => Expired,
            "CLOSED" => Closed,
            _ => new LeaseStatus(value.Trim())
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
