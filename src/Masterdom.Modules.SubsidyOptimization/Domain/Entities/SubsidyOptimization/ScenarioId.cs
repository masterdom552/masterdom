using Masterdom.Core.Primitives;

namespace Masterdom.Modules.SubsidyOptimization.Domain.Entities.SubsidyOptimization;

public sealed class ScenarioId : ValueObject
{
    private ScenarioId(string value)
    {
        Value = value;
    }

    public string Value { get; }

    public static ScenarioId Create(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);

        var normalized = value.Trim().ToUpperInvariant();
        if (normalized.Length > 100)
        {
            throw new ArgumentException("ScenarioId cannot exceed 100 characters.", nameof(value));
        }

        return new ScenarioId(normalized);
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }
}
