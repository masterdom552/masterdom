using System;
using System.Collections.Generic;
using System.Linq;

namespace Masterdom.Platform.Rules;

/// <summary>
/// Performs rule-model validation.
/// </summary>
public static class RuleValidation
{
    public static void ValidateAll(
        IReadOnlyList<RuleSetDefinition> ruleSets,
        IReadOnlyList<RuleDefinition> rules)
    {
        ArgumentNullException.ThrowIfNull(ruleSets);
        ArgumentNullException.ThrowIfNull(rules);

        ValidateDuplicateRuleSetIds(ruleSets);
        ValidateDuplicateRuleIds(rules);
        ValidateDuplicateRuleSets(ruleSets);
        ValidateDuplicateRules(rules);
        ValidateMissingRuleSetDependencies(ruleSets, rules);
        ValidateMissingParentRules(rules);
        ValidateCircularReferences(rules);
        ValidateInvalidScopes(ruleSets, rules);
        ValidateInvalidConditions(rules);
    }

    private static void ValidateDuplicateRuleSetIds(IReadOnlyList<RuleSetDefinition> ruleSets)
    {
        var duplicates = ruleSets
            .GroupBy(ruleSet => ruleSet.Id.Value)
            .Where(group => group.Count() > 1)
            .Select(group => group.Key)
            .ToList();

        if (duplicates.Count > 0)
        {
            throw new RuleValidationException(
                $"Duplicate rule-set identifiers were found: {string.Join(", ", duplicates)}.");
        }
    }

    private static void ValidateDuplicateRuleIds(IReadOnlyList<RuleDefinition> rules)
    {
        var duplicates = rules
            .GroupBy(rule => rule.Id.Value)
            .Where(group => group.Count() > 1)
            .Select(group => group.Key)
            .ToList();

        if (duplicates.Count > 0)
        {
            throw new RuleValidationException(
                $"Duplicate rule identifiers were found: {string.Join(", ", duplicates)}.");
        }
    }

    private static void ValidateDuplicateRuleSets(IReadOnlyList<RuleSetDefinition> ruleSets)
    {
        var duplicates = ruleSets
            .GroupBy(ruleSet => new
            {
                Key = ruleSet.Key.Value.ToUpperInvariant(),
                Scope = ruleSet.Scope.ToString().ToUpperInvariant(),
                Version = ruleSet.Version.Value,
                EffectiveFrom = ruleSet.Period.EffectiveFromUtc
            })
            .Where(group => group.Count() > 1)
            .ToList();

        if (duplicates.Count > 0)
        {
            throw new RuleValidationException(
                "Duplicate rule sets were found for the same key, scope, version, and effective date.");
        }
    }

    private static void ValidateDuplicateRules(IReadOnlyList<RuleDefinition> rules)
    {
        var duplicates = rules
            .GroupBy(rule => new
            {
                RuleSetId = rule.RuleSetId.Value,
                Key = rule.Key.Value.ToUpperInvariant(),
                Scope = rule.Scope.ToString().ToUpperInvariant(),
                Version = rule.Version.Value,
                EffectiveFrom = rule.Period.EffectiveFromUtc
            })
            .Where(group => group.Count() > 1)
            .ToList();

        if (duplicates.Count > 0)
        {
            throw new RuleValidationException(
                "Duplicate rules were found for the same set, key, scope, version, and effective date.");
        }
    }

    private static void ValidateMissingRuleSetDependencies(
        IReadOnlyList<RuleSetDefinition> ruleSets,
        IReadOnlyList<RuleDefinition> rules)
    {
        var setIds = ruleSets.Select(ruleSet => ruleSet.Id.Value).ToHashSet();

        foreach (var rule in rules)
        {
            if (setIds.Contains(rule.RuleSetId.Value))
            {
                continue;
            }

            throw new RuleValidationException(
                $"Rule '{rule.Key.Value}' references missing rule set '{rule.RuleSetId.Value}'.");
        }
    }

    private static void ValidateMissingParentRules(IReadOnlyList<RuleDefinition> rules)
    {
        var ruleIds = rules.Select(rule => rule.Id.Value).ToHashSet();

        foreach (var rule in rules)
        {
            if (!rule.ParentRuleId.HasValue)
            {
                continue;
            }

            if (ruleIds.Contains(rule.ParentRuleId.Value.Value))
            {
                continue;
            }

            throw new RuleValidationException(
                $"Rule '{rule.Key.Value}' references missing parent rule '{rule.ParentRuleId.Value.Value}'.");
        }
    }

    private static void ValidateCircularReferences(IReadOnlyList<RuleDefinition> rules)
    {
        var byId = rules.ToDictionary(rule => rule.Id.Value);

        foreach (var rule in rules)
        {
            var visited = new HashSet<Guid>();
            var cursor = rule;

            while (cursor.ParentRuleId.HasValue)
            {
                if (!visited.Add(cursor.Id.Value))
                {
                    throw new RuleValidationException(
                        $"Circular rule references detected at '{cursor.Id.Value}'.");
                }

                if (!byId.TryGetValue(cursor.ParentRuleId.Value.Value, out var parent))
                {
                    break;
                }

                cursor = parent;
            }
        }
    }

    private static void ValidateInvalidScopes(
        IReadOnlyList<RuleSetDefinition> ruleSets,
        IReadOnlyList<RuleDefinition> rules)
    {
        foreach (var set in ruleSets)
        {
            if (IsScopeAllowed(set.Category, set.Scope.Kind))
            {
                continue;
            }

            throw new RuleValidationException(
                $"Invalid rule-set scope '{set.Scope.Kind}' for category '{set.Category}'.");
        }

        foreach (var rule in rules)
        {
            if (IsScopeAllowed(rule.Category, rule.Scope.Kind))
            {
                continue;
            }

            throw new RuleValidationException(
                $"Invalid rule scope '{rule.Scope.Kind}' for category '{rule.Category}' and key '{rule.Key.Value}'.");
        }
    }

    private static void ValidateInvalidConditions(IReadOnlyList<RuleDefinition> rules)
    {
        foreach (var rule in rules)
        {
            var condition = rule.Condition;

            switch (rule.Kind)
            {
                case RuleKind.Boolean:
                    if (condition.InputKey is null ||
                        condition.ExpectedValue is null ||
                        condition.ExpectedValue.Kind != RuleValueKind.Boolean)
                    {
                        throw new RuleValidationException(
                            $"Boolean rule '{rule.Key.Value}' has an invalid condition.");
                    }

                    break;

                case RuleKind.Comparison:
                    if (condition.InputKey is null ||
                        !condition.ComparisonOperator.HasValue ||
                        (condition.ExpectedValue is null && condition.CompareInputKey is null))
                    {
                        throw new RuleValidationException(
                            $"Comparison rule '{rule.Key.Value}' has an invalid condition.");
                    }

                    break;

                case RuleKind.Range:
                    if (condition.InputKey is null ||
                        condition.MinimumValue is null ||
                        condition.MaximumValue is null ||
                        condition.MinimumValue.Kind != RuleValueKind.Number ||
                        condition.MaximumValue.Kind != RuleValueKind.Number)
                    {
                        throw new RuleValidationException(
                            $"Range rule '{rule.Key.Value}' has an invalid condition.");
                    }

                    if (condition.MinimumValue.AsNumber() > condition.MaximumValue.AsNumber())
                    {
                        throw new RuleValidationException(
                            $"Range rule '{rule.Key.Value}' has minimum greater than maximum.");
                    }

                    break;

                case RuleKind.Expression:
                    if (condition.ExpressionLeftKey is null ||
                        condition.ExpressionRightKey is null ||
                        !condition.ArithmeticOperator.HasValue ||
                        !condition.ComparisonOperator.HasValue ||
                        condition.ExpressionExpectedValue is null ||
                        condition.ExpressionExpectedValue.Kind != RuleValueKind.Number)
                    {
                        throw new RuleValidationException(
                            $"Expression rule '{rule.Key.Value}' has an invalid condition.");
                    }

                    break;

                case RuleKind.Composite:
                    if (!condition.CompositeOperator.HasValue)
                    {
                        throw new RuleValidationException(
                            $"Composite rule '{rule.Key.Value}' has an invalid condition.");
                    }

                    break;

                default:
                    throw new RuleValidationException(
                        $"Rule '{rule.Key.Value}' uses unsupported kind '{rule.Kind}'.");
            }
        }
    }

    private static bool IsScopeAllowed(RuleCategory category, RuleScopeKind scopeKind)
    {
        return category switch
        {
            RuleCategory.Validation => scopeKind is RuleScopeKind.Global or RuleScopeKind.Module or RuleScopeKind.Tenant or RuleScopeKind.Aggregate or RuleScopeKind.Entity or RuleScopeKind.Property or RuleScopeKind.Field,
            RuleCategory.Eligibility => scopeKind is RuleScopeKind.Module or RuleScopeKind.Tenant or RuleScopeKind.Entity or RuleScopeKind.Property,
            RuleCategory.Pricing => scopeKind is RuleScopeKind.Module or RuleScopeKind.Tenant or RuleScopeKind.Property,
            RuleCategory.Compliance => scopeKind is RuleScopeKind.Global or RuleScopeKind.Module or RuleScopeKind.Tenant or RuleScopeKind.Entity,
            RuleCategory.Security => scopeKind is RuleScopeKind.Global or RuleScopeKind.Module or RuleScopeKind.Tenant,
            RuleCategory.Custom => scopeKind is RuleScopeKind.Global or RuleScopeKind.Module or RuleScopeKind.Tenant or RuleScopeKind.Aggregate or RuleScopeKind.Entity or RuleScopeKind.Property or RuleScopeKind.Field,
            _ => false
        };
    }
}
