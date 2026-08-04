using Masterdom.Core.Primitives;

namespace Masterdom.Modules.SubsidyOptimization.Domain.Entities.SubsidyOptimization;

public sealed class SubsidyScenario : ValueObject
{
    private SubsidyScenario(ScenarioId scenarioId, string name, string description)
    {
        ScenarioId = scenarioId;
        Name = name;
        Description = description;
    }

    public ScenarioId ScenarioId { get; }

    public string Name { get; }

    public string Description { get; }

    public static SubsidyScenario Create(ScenarioId scenarioId, string name, string description)
    {
        ArgumentNullException.ThrowIfNull(scenarioId);
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentException.ThrowIfNullOrWhiteSpace(description);

        return new SubsidyScenario(
            scenarioId,
            name.Trim(),
            description.Trim());
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return ScenarioId;
        yield return Name;
        yield return Description;
    }
}
