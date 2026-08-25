using Masterdom.Core.Primitives;

namespace Masterdom.Modules.Payment.Domain.Entities.Payment;

public sealed class PaymentStatus : ValueObject
{
    public static readonly PaymentStatus Received = new("Received");
    public static readonly PaymentStatus PartiallyAllocated = new("PartiallyAllocated");
    public static readonly PaymentStatus Allocated = new("Allocated");
    public static readonly PaymentStatus Reversed = new("Reversed");
    public static readonly PaymentStatus Voided = new("Voided");

    private PaymentStatus(string value)
    {
        Value = value;
    }

    public string Value { get; }

    public static PaymentStatus Create(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);

        return value.Trim().ToUpperInvariant() switch
        {
            "RECEIVED" => Received,
            "PARTIALLYALLOCATED" => PartiallyAllocated,
            "ALLOCATED" => Allocated,
            "REVERSED" => Reversed,
            "VOIDED" => Voided,
            _ => new PaymentStatus(value.Trim())
        };
    }

    public override string ToString() => Value;

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value.ToUpperInvariant();
    }
}
