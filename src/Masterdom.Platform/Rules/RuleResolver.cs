using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Masterdom.Platform.Configuration;
using Masterdom.Platform.Metadata;

namespace Masterdom.Platform.Rules;

/// <summary>
/// Resolves and evaluates effective rule sets.
/// </summary>
public sealed class RuleResolver : IRuleResolver
{
    private readonly IRuleRepository _repository;
    private readonly IConfigurationResolver _configuration;
    private readonly IMetadataResolver _metadata;

    public RuleResolver(
        IRuleRepository repository,
        IConfigurationResolver configuration,
        IMetadataResolver metadata)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        _metadata = metadata ?? throw new ArgumentNullException(nameof(metadata));
    }

    public RuleOutput Evaluate(
        RuleSetKey ruleSetKey,
        RuleScope scope,
        RuleContext context,
        RuleInput input)
    {
        ArgumentNullException.ThrowIfNull(ruleSetKey);
        ArgumentNullException.ThrowIfNull(scope);
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(input);

        if (string.IsNullOrWhiteSpace(context.ModuleId))
        {
            throw new RuleValidationException("ModuleId is required for rule evaluation context.");
        }

        if (context.AsOfUtc.Kind != DateTimeKind.Utc)
        {
            throw new RuleValidationException("Rule evaluation context timestamp must be UTC.");
        }

        var ruleSets = _repository.GetAllRuleSets();
        var rules = _repository.GetAllRules();

        var activeSet = ResolveActiveRuleSet(ruleSets, ruleSetKey, scope, context.AsOfUtc);

        var activeRules = rules
            .Where(rule => rule.RuleSetId.Value == activeSet.Id.Value)
            .Where(rule => rule.Period.IsEffectiveAt(context.AsOfUtc))
            .OrderBy(rule => rule.Priority.Value)
            .ToList();

        var byParent = activeRules
            .GroupBy(rule => rule.ParentRuleId?.Value ?? Guid.Empty)
            .ToDictionary(group => group.Key, group => group.ToList());

        var results = new List<RuleResult>();

        if (byParent.TryGetValue(Guid.Empty, out var roots))
        {
            foreach (var root in roots.OrderBy(rule => rule.Priority.Value))
            {
                EvaluateRuleRecursive(root, byParent, context, input, results);
            }
        }

        return new RuleOutput
        {
            RuleSetId = activeSet.Id,
            RuleSetKey = activeSet.Key,
            Results = results
        };
    }

    private static RuleSetDefinition ResolveActiveRuleSet(
        IReadOnlyList<RuleSetDefinition> ruleSets,
        RuleSetKey key,
        RuleScope scope,
        DateTime asOfUtc)
    {
        var candidates = ruleSets
            .Where(ruleSet => ruleSet.Key.Equals(key))
            .Where(ruleSet => ruleSet.Scope.Equals(scope))
            .Where(ruleSet => ruleSet.Period.IsEffectiveAt(asOfUtc))
            .OrderByDescending(ruleSet => ruleSet.Period.EffectiveFromUtc)
            .ThenByDescending(ruleSet => ruleSet.Version.Value)
            .ToList();

        if (candidates.Count == 0)
        {
            throw new RuleValidationException(
                $"No active rule set was found for key '{key.Value}' in scope '{scope}'.");
        }

        return candidates[0];
    }

    private bool EvaluateRuleRecursive(
        RuleDefinition rule,
        IReadOnlyDictionary<Guid, List<RuleDefinition>> byParent,
        RuleContext context,
        RuleInput input,
        List<RuleResult> results)
    {
        bool passed;
        string message;

        if (rule.Kind == RuleKind.Composite)
        {
            var children = byParent.TryGetValue(rule.Id.Value, out var nested)
                ? nested.OrderBy(r => r.Priority.Value).ToList()
                : new List<RuleDefinition>();

            var childOutcomes = new List<bool>(children.Count);

            foreach (var child in children)
            {
                childOutcomes.Add(EvaluateRuleRecursive(child, byParent, context, input, results));
            }

            passed = ApplyComposite(rule.Condition.CompositeOperator!.Value, childOutcomes);
            message = $"Composite rule '{rule.Key.Value}' evaluated {childOutcomes.Count} child rule(s).";
        }
        else
        {
            passed = EvaluateAtomic(rule, context, input);
            message = passed
                ? $"Rule '{rule.Key.Value}' passed."
                : $"Rule '{rule.Key.Value}' failed.";
        }

        results.Add(new RuleResult
        {
            RuleId = rule.Id,
            RuleKey = rule.Key,
            Status = passed ? RuleResultStatus.Passed : RuleResultStatus.Failed,
            Severity = rule.Severity,
            Priority = rule.Priority,
            Message = message
        });

        return passed;
    }

    private bool EvaluateAtomic(RuleDefinition rule, RuleContext context, RuleInput input)
    {
        var condition = rule.Condition;

        return rule.Kind switch
        {
            RuleKind.Boolean => ResolveValue(condition.InputKey!, context, input).AsBoolean() == condition.ExpectedValue!.AsBoolean(),
            RuleKind.Comparison => EvaluateComparison(
                ResolveValue(condition.InputKey!, context, input),
                ResolveComparisonRightValue(condition, context, input),
                condition.ComparisonOperator!.Value),
            RuleKind.Range => EvaluateRange(
                ResolveValue(condition.InputKey!, context, input),
                condition.MinimumValue!,
                condition.MaximumValue!),
            RuleKind.Expression => EvaluateExpression(condition, context, input),
            _ => throw new RuleValidationException(
                $"Unsupported rule kind '{rule.Kind}' for atomic evaluation.")
        };
    }

    private RuleValue ResolveComparisonRightValue(
        RuleCondition condition,
        RuleContext context,
        RuleInput input)
    {
        if (condition.CompareInputKey is not null)
        {
            return ResolveValue(condition.CompareInputKey, context, input);
        }

        return condition.ExpectedValue!;
    }

    private static bool EvaluateComparison(
        RuleValue left,
        RuleValue right,
        RuleComparisonOperator op)
    {
        if (left.Kind != right.Kind)
        {
            throw new RuleValidationException("Comparison operands must have the same value kind.");
        }

        return left.Kind switch
        {
            RuleValueKind.Boolean => ApplyBooleanComparison(left.AsBoolean(), right.AsBoolean(), op),
            RuleValueKind.Number => ApplyNumberComparison(left.AsNumber(), right.AsNumber(), op),
            RuleValueKind.Text => ApplyTextComparison(left.AsText(), right.AsText(), op),
            _ => throw new RuleValidationException("Unsupported comparison value kind.")
        };
    }

    private static bool EvaluateRange(RuleValue input, RuleValue min, RuleValue max)
    {
        var number = input.AsNumber();
        var minimum = min.AsNumber();
        var maximum = max.AsNumber();

        return number >= minimum && number <= maximum;
    }

    private bool EvaluateExpression(
        RuleCondition condition,
        RuleContext context,
        RuleInput input)
    {
        var left = ResolveValue(condition.ExpressionLeftKey!, context, input).AsNumber();
        var right = ResolveValue(condition.ExpressionRightKey!, context, input).AsNumber();

        var result = condition.ArithmeticOperator!.Value switch
        {
            RuleArithmeticOperator.Add => left + right,
            RuleArithmeticOperator.Subtract => left - right,
            RuleArithmeticOperator.Multiply => left * right,
            RuleArithmeticOperator.Divide => right == 0
                ? throw new RuleValidationException("Expression rule division by zero is invalid.")
                : left / right,
            _ => throw new RuleValidationException("Unsupported arithmetic operator.")
        };

        var expected = condition.ExpressionExpectedValue!.AsNumber();
        return ApplyNumberComparison(result, expected, condition.ComparisonOperator!.Value);
    }

    private RuleValue ResolveValue(
        RuleInputKey key,
        RuleContext context,
        RuleInput input)
    {
        var value = key.Value;

        const string configPrefix = "config:";
        if (value.StartsWith(configPrefix, StringComparison.OrdinalIgnoreCase))
        {
            var configKey = new ConfigurationKey(value[configPrefix.Length..]);
            var resolved = _configuration.Resolve(
                configKey,
                new ConfigurationResolutionRequest
                {
                    ModuleId = context.ModuleId,
                    TenantId = context.TenantId,
                    PropertyId = context.PropertyId,
                    AsOfUtc = context.AsOfUtc
                });

            return ParseLiteralValue(resolved.Record.Value.Value);
        }

        const string metadataPrefix = "metadata:";
        if (value.StartsWith(metadataPrefix, StringComparison.OrdinalIgnoreCase))
        {
            var metadataKey = new MetadataKey(value[metadataPrefix.Length..]);
            var resolved = _metadata.Resolve(
                metadataKey,
                Masterdom.Platform.Metadata.MetadataScope.Module(context.ModuleId),
                context.AsOfUtc);

            return RuleValue.FromText(resolved.Name);
        }

        return input.GetRequiredValue(key);
    }

    private static RuleValue ParseLiteralValue(string value)
    {
        if (bool.TryParse(value, out var boolValue))
        {
            return RuleValue.FromBoolean(boolValue);
        }

        if (decimal.TryParse(
                value,
                NumberStyles.Number,
                CultureInfo.InvariantCulture,
                out var numberValue))
        {
            return RuleValue.FromNumber(numberValue);
        }

        return RuleValue.FromText(value);
    }

    private static bool ApplyComposite(
        RuleCompositeOperator op,
        IReadOnlyList<bool> outcomes)
    {
        return op switch
        {
            RuleCompositeOperator.All => outcomes.All(x => x),
            RuleCompositeOperator.Any => outcomes.Any(x => x),
            RuleCompositeOperator.None => outcomes.All(x => !x),
            _ => throw new RuleValidationException("Unsupported composite operator.")
        };
    }

    private static bool ApplyBooleanComparison(bool left, bool right, RuleComparisonOperator op)
    {
        return op switch
        {
            RuleComparisonOperator.Equal => left == right,
            RuleComparisonOperator.NotEqual => left != right,
            _ => throw new RuleValidationException(
                $"Boolean values do not support '{op}' comparison.")
        };
    }

    private static bool ApplyNumberComparison(decimal left, decimal right, RuleComparisonOperator op)
    {
        return op switch
        {
            RuleComparisonOperator.Equal => left == right,
            RuleComparisonOperator.NotEqual => left != right,
            RuleComparisonOperator.GreaterThan => left > right,
            RuleComparisonOperator.GreaterThanOrEqual => left >= right,
            RuleComparisonOperator.LessThan => left < right,
            RuleComparisonOperator.LessThanOrEqual => left <= right,
            _ => throw new RuleValidationException(
                $"Numeric values do not support '{op}' comparison.")
        };
    }

    private static bool ApplyTextComparison(string left, string right, RuleComparisonOperator op)
    {
        return op switch
        {
            RuleComparisonOperator.Equal => string.Equals(left, right, StringComparison.OrdinalIgnoreCase),
            RuleComparisonOperator.NotEqual => !string.Equals(left, right, StringComparison.OrdinalIgnoreCase),
            RuleComparisonOperator.Contains => left.Contains(right, StringComparison.OrdinalIgnoreCase),
            RuleComparisonOperator.StartsWith => left.StartsWith(right, StringComparison.OrdinalIgnoreCase),
            RuleComparisonOperator.EndsWith => left.EndsWith(right, StringComparison.OrdinalIgnoreCase),
            _ => throw new RuleValidationException(
                $"Text values do not support '{op}' comparison.")
        };
    }
}
