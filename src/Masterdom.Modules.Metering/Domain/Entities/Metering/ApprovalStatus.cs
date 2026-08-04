using Masterdom.Core.Primitives;

namespace Masterdom.Modules.Metering.Domain.Entities.Metering;

public sealed class ApprovalStatus : ValueObject
{
    public static readonly ApprovalStatus Pending = new("Pending");
    public static readonly ApprovalStatus Approved = new("Approved");
    public static readonly ApprovalStatus Rejected = new("Rejected");

    private ApprovalStatus(string value)
    {
        Value = value;
    }

    public string Value { get; }

    public static ApprovalStatus Create(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);

        return value.Trim().ToUpperInvariant() switch
        {
            "PENDING" => Pending,
            "APPROVED" => Approved,
            "REJECTED" => Rejected,
            _ => new ApprovalStatus(value.Trim())
        };
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value.ToUpperInvariant();
    }
}
