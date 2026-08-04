using Masterdom.Core.Primitives;

namespace Masterdom.Modules.UtilityRating.Domain.Entities.UtilityRating;

public sealed class RatingStatus : ValueObject
{
    public static readonly RatingStatus Calculated = new("Calculated");
    public static readonly RatingStatus Approved = new("Approved");
    public static readonly RatingStatus Archived = new("Archived");

    private RatingStatus(string value)
    {
        Value = value;
    }

    public string Value { get; }

    public static RatingStatus Create(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);

        return value.Trim().ToUpperInvariant() switch
        {
            "CALCULATED" => Calculated,
            "APPROVED" => Approved,
            "ARCHIVED" => Archived,
            _ => new RatingStatus(value.Trim())
        };
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value.ToUpperInvariant();
    }
}
