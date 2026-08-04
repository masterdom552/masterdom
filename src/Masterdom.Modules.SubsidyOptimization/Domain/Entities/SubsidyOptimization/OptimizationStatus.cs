using Masterdom.Core.Primitives;

namespace Masterdom.Modules.SubsidyOptimization.Domain.Entities.SubsidyOptimization;

public sealed class OptimizationStatus : ValueObject
{
    public static readonly OptimizationStatus Started = new("Started");
    public static readonly OptimizationStatus Completed = new("Completed");
    public static readonly OptimizationStatus Archived = new("Archived");

    private OptimizationStatus(string value)
    {
        Value = value;
    }

    public string Value { get; }

    public static OptimizationStatus Create(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);

        return value.Trim().ToUpperInvariant() switch
        {
            "STARTED" => Started,
            "COMPLETED" => Completed,
            "ARCHIVED" => Archived,
            _ => new OptimizationStatus(value.Trim())
        };
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value.ToUpperInvariant();
    }
}
