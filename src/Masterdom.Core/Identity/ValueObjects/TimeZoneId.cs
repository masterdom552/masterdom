using Masterdom.Core.Common.Primitives;

namespace Masterdom.Core.Identity.ValueObjects;

public sealed class TimeZoneId : ValueObject
{
    public string Value { get; }

    public TimeZoneId(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);

        try
        {
            TimeZoneInfo.FindSystemTimeZoneById(value);
        }
        catch
        {
            throw new ArgumentException(
                "Invalid IANA/OS timezone.");
        }

        Value = value;
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString()
        => Value;
}
