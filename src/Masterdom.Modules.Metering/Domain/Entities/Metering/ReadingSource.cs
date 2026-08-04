using Masterdom.Core.Primitives;

namespace Masterdom.Modules.Metering.Domain.Entities.Metering;

public sealed class ReadingSource : ValueObject
{
    public static readonly ReadingSource Manual = new("Manual");
    public static readonly ReadingSource Device = new("Device");
    public static readonly ReadingSource Imported = new("Imported");

    private ReadingSource(string value)
    {
        Value = value;
    }

    public string Value { get; }

    public static ReadingSource Create(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);

        return value.Trim().ToUpperInvariant() switch
        {
            "MANUAL" => Manual,
            "DEVICE" => Device,
            "IMPORTED" => Imported,
            _ => new ReadingSource(value.Trim())
        };
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value.ToUpperInvariant();
    }
}
