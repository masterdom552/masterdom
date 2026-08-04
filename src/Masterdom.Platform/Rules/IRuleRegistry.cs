using System.Collections.Generic;

namespace Masterdom.Platform.Rules;

/// <summary>
/// Registers rule sets and rules into the runtime repository.
/// </summary>
public interface IRuleRegistry
{
    void ReplaceAll(
        IReadOnlyList<RuleSetDefinition> ruleSets,
        IReadOnlyList<RuleDefinition> rules);

    void Register(
        IReadOnlyList<RuleSetDefinition> ruleSets,
        IReadOnlyList<RuleDefinition> rules);

    IRuleCatalog GetCatalog();
}
