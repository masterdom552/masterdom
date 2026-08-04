using System;
using System.Collections.Generic;
using System.Linq;

namespace Masterdom.Platform.Rules;

/// <summary>
/// Default rule registry implementation.
/// </summary>
public sealed class RuleRegistry : IRuleRegistry
{
    private readonly InMemoryRuleRepository _repository;

    public RuleRegistry(IRuleRepository? repository = null)
    {
        _repository = repository as InMemoryRuleRepository
            ?? new InMemoryRuleRepository(
                repository?.GetAllRuleSets(),
                repository?.GetAllRules());
    }

    public void ReplaceAll(
        IReadOnlyList<RuleSetDefinition> ruleSets,
        IReadOnlyList<RuleDefinition> rules)
    {
        ArgumentNullException.ThrowIfNull(ruleSets);
        ArgumentNullException.ThrowIfNull(rules);

        RuleValidation.ValidateAll(ruleSets, rules);

        _repository.ReplaceAll(ruleSets, rules);
    }

    public void Register(
        IReadOnlyList<RuleSetDefinition> ruleSets,
        IReadOnlyList<RuleDefinition> rules)
    {
        ArgumentNullException.ThrowIfNull(ruleSets);
        ArgumentNullException.ThrowIfNull(rules);

        var mergedSets = _repository.GetAllRuleSets()
            .Concat(ruleSets)
            .ToList();

        var mergedRules = _repository.GetAllRules()
            .Concat(rules)
            .ToList();

        RuleValidation.ValidateAll(mergedSets, mergedRules);

        _repository.ReplaceAll(mergedSets, mergedRules);
    }

    public IRuleCatalog GetCatalog()
    {
        return new RuleCatalog(
            _repository.GetAllRuleSets(),
            _repository.GetAllRules());
    }
}
