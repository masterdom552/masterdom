using Masterdom.Core.Primitives;

namespace Masterdom.Modules.Metering.Domain.Entities.Metering;

public sealed class ReviewedBy : ValueObject
{
    private ReviewedBy(string value)
    {
        Value = value;
    }

    public string Value { get; }

    public static ReviewedBy Create(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);

        var normalized = value.Trim();
        if (normalized.Length > 100)
        {
            throw new ArgumentException("ReviewedBy cannot exceed 100 characters.", nameof(value));
        }

        return new ReviewedBy(normalized);
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }
}
