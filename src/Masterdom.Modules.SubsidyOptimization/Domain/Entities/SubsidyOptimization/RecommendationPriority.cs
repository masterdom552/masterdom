using Masterdom.Core.Primitives;

namespace Masterdom.Modules.SubsidyOptimization.Domain.Entities.SubsidyOptimization;

public sealed class RecommendationPriority : ValueObject
{
    public static readonly RecommendationPriority High = new("High", 1);
    public static readonly RecommendationPriority Medium = new("Medium", 2);
    public static readonly RecommendationPriority Low = new("Low", 3);

    private RecommendationPriority(string value, int rank)
    {
        Value = value;
        Rank = rank;
    }

    public string Value { get; }

    public int Rank { get; }

    public static RecommendationPriority Create(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);

        return value.Trim().ToUpperInvariant() switch
        {
            "HIGH" => High,
            "MEDIUM" => Medium,
            "LOW" => Low,
            _ => throw new InvalidOperationException("Recommendation priority must be High, Medium, or Low.")
        };
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value.ToUpperInvariant();
    }
}
