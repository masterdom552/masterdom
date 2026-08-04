using Masterdom.Core.Primitives;

namespace Masterdom.Modules.Metering.Domain.Entities.Metering;

public sealed class ReadingNotes : ValueObject
{
    private ReadingNotes(string value)
    {
        Value = value;
    }

    public string Value { get; }

    public static ReadingNotes Create(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);

        var normalized = value.Trim();
        if (normalized.Length > 500)
        {
            throw new ArgumentException("Reading notes cannot exceed 500 characters.", nameof(value));
        }

        return new ReadingNotes(normalized);
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }
}
