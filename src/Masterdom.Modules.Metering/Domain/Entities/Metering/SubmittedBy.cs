using Masterdom.Core.Primitives;

namespace Masterdom.Modules.Metering.Domain.Entities.Metering;

public sealed class SubmittedBy : ValueObject
{
    private SubmittedBy(string value)
    {
        Value = value;
    }

    public string Value { get; }

    public static SubmittedBy Create(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);

        var normalized = value.Trim();
        if (normalized.Length > 100)
        {
            throw new ArgumentException("SubmittedBy cannot exceed 100 characters.", nameof(value));
        }

        return new SubmittedBy(normalized);
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }
}
