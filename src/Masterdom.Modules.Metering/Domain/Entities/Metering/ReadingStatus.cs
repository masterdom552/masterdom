using Masterdom.Core.Primitives;

namespace Masterdom.Modules.Metering.Domain.Entities.Metering;

public sealed class ReadingStatus : ValueObject
{
    public static readonly ReadingStatus Submitted = new("Submitted");
    public static readonly ReadingStatus Approved = new("Approved");
    public static readonly ReadingStatus Corrected = new("Corrected");

    private ReadingStatus(string value)
    {
        Value = value;
    }

    public string Value { get; }

    public static ReadingStatus Create(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);

        return value.Trim().ToUpperInvariant() switch
        {
            "SUBMITTED" => Submitted,
            "APPROVED" => Approved,
            "CORRECTED" => Corrected,
            _ => new ReadingStatus(value.Trim())
        };
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value.ToUpperInvariant();
    }
}
