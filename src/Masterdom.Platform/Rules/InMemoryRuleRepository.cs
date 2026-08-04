using System;
using System.Collections.Generic;
using System.Linq;

namespace Masterdom.Platform.Rules;

/// <summary>
/// In-memory rules repository implementation.
/// </summary>
public sealed class InMemoryRuleRepository : IRuleRepository
{
    private List<RuleSetDefinition> _ruleSets;
    private List<RuleDefinition> _rules;

    public InMemoryRuleRepository(
        IReadOnlyList<RuleSetDefinition>? ruleSets = null,
        IReadOnlyList<RuleDefinition>? rules = null)
    {
        _ruleSets = ruleSets?.ToList() ?? new List<RuleSetDefinition>();
        _rules = rules?.ToList() ?? new List<RuleDefinition>();
    }

    public IReadOnlyList<RuleSetDefinition> GetAllRuleSets()
    {
        return _ruleSets;
    }

    public IReadOnlyList<RuleDefinition> GetAllRules()
    {
        return _rules;
    }

    public void ReplaceAll(
        IReadOnlyList<RuleSetDefinition> ruleSets,
        IReadOnlyList<RuleDefinition> rules)
    {
        _ruleSets = ruleSets?.ToList()
            ?? throw new ArgumentNullException(nameof(ruleSets));

        _rules = rules?.ToList()
            ?? throw new ArgumentNullException(nameof(rules));
    }
}
