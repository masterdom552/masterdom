using System.Collections.Generic;

namespace Masterdom.Platform.Rules;

/// <summary>
/// Provides read access to rule sets and rules.
/// </summary>
public interface IRuleRepository
{
    IReadOnlyList<RuleSetDefinition> GetAllRuleSets();

    IReadOnlyList<RuleDefinition> GetAllRules();
}
