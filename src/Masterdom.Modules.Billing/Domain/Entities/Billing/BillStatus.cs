using Masterdom.Core.Primitives;

namespace Masterdom.Modules.Billing.Domain.Entities.Billing;

/// <summary>
/// Represents bill lifecycle status.
/// </summary>
public sealed class BillStatus : ValueObject
{
    public static readonly BillStatus Draft = new("Draft");
    public static readonly BillStatus Generated = new("Generated");
    public static readonly BillStatus Finalized = new("Finalized");
    public static readonly BillStatus Voided = new("Voided");

    private BillStatus(string value)
    {
        Value = value;
    }

    public string Value { get; }

    public static BillStatus Create(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);

        return value.Trim().ToUpperInvariant() switch
        {
            "DRAFT" => Draft,
            "GENERATED" => Generated,
            "FINALIZED" => Finalized,
            "VOIDED" => Voided,
            _ => new BillStatus(value.Trim())
        };
    }

    public override string ToString() => Value;

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value.ToUpperInvariant();
    }
}
