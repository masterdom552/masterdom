using System;
using System.Collections.Generic;
using System.Linq;
using Masterdom.Platform.Rules;
using Microsoft.EntityFrameworkCore;

namespace Masterdom.Infrastructure.Persistence.Rules;

/// <summary>
/// EF Core-backed rules repository implementation.
/// </summary>
public sealed class PlatformRuleRepository : IRuleRepository
{
    private readonly MasterdomDbContext _dbContext;

    public PlatformRuleRepository(MasterdomDbContext dbContext)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
    }

    public IReadOnlyList<RuleSetDefinition> GetAllRuleSets()
    {
        var entities = _dbContext
            .Set<PlatformRuleSetEntity>()
            .AsNoTracking()
            .OrderBy(x => x.Key)
            .ThenBy(x => x.ScopeKind)
            .ThenBy(x => x.ScopeIdentifier)
            .ThenByDescending(x => x.EffectiveFromUtc)
            .ThenByDescending(x => x.Version)
            .ToList();

        return entities
            .Select(entity => new RuleSetDefinition(
                new RuleSetId(entity.Id),
                new RuleSetKey(entity.Key),
                entity.Name,
                entity.Description,
                (RuleCategory)entity.Category,
                RuleScope.Create(
                    (RuleScopeKind)entity.ScopeKind,
                    entity.ScopeIdentifier),
                new RuleVersion(entity.Version),
                new RuleEffectivePeriod(entity.EffectiveFromUtc, entity.EffectiveToUtc),
                entity.IsDeprecated,
                string.IsNullOrWhiteSpace(entity.ReplacedByKey)
                    ? null
                    : new RuleSetKey(entity.ReplacedByKey),
                entity.Compatibility,
                entity.ChangedBy,
                entity.ChangedAtUtc))
            .ToList();
    }

    public IReadOnlyList<RuleDefinition> GetAllRules()
    {
        var entities = _dbContext
            .Set<PlatformRuleDefinitionEntity>()
            .AsNoTracking()
            .OrderBy(x => x.RuleSetId)
            .ThenBy(x => x.ParentRuleId)
            .ThenBy(x => x.Priority)
            .ThenByDescending(x => x.EffectiveFromUtc)
            .ThenByDescending(x => x.Version)
            .ToList();

        return entities
            .Select(MapRule)
            .ToList();
    }

    private static RuleDefinition MapRule(PlatformRuleDefinitionEntity entity)
    {
        return new RuleDefinition(
            new RuleId(entity.Id),
            new RuleSetId(entity.RuleSetId),
            new RuleKey(entity.Key),
            entity.Name,
            entity.Description,
            (RuleKind)entity.Kind,
            MapCondition(entity),
            (RuleCategory)entity.Category,
            (RuleSeverity)entity.Severity,
            new RulePriority(entity.Priority),
            RuleScope.Create(
                (RuleScopeKind)entity.ScopeKind,
                entity.ScopeIdentifier),
            new RuleVersion(entity.Version),
            new RuleEffectivePeriod(entity.EffectiveFromUtc, entity.EffectiveToUtc),
            entity.ParentRuleId.HasValue ? new RuleId(entity.ParentRuleId.Value) : null,
            entity.IsDeprecated,
            string.IsNullOrWhiteSpace(entity.ReplacedByKey)
                ? null
                : new RuleKey(entity.ReplacedByKey),
            entity.Compatibility,
            entity.ChangedBy,
            entity.ChangedAtUtc);
    }

    private static RuleCondition MapCondition(PlatformRuleDefinitionEntity entity)
    {
        var kind = (RuleKind)entity.Kind;

        return kind switch
        {
            RuleKind.Boolean => RuleCondition.Boolean(
                new RuleInputKey(entity.InputKey!),
                entity.ExpectedBoolean ?? false),

            RuleKind.Comparison => RuleCondition.Comparison(
                new RuleInputKey(entity.InputKey!),
                (RuleComparisonOperator)entity.ComparisonOperator!.Value,
                MapExpectedValue(entity),
                string.IsNullOrWhiteSpace(entity.CompareInputKey)
                    ? null
                    : new RuleInputKey(entity.CompareInputKey)),

            RuleKind.Range => RuleCondition.Range(
                new RuleInputKey(entity.InputKey!),
                RuleValue.FromNumber(entity.MinNumber ?? 0m),
                RuleValue.FromNumber(entity.MaxNumber ?? 0m)),

            RuleKind.Expression => RuleCondition.Expression(
                new RuleInputKey(entity.ExpressionLeftKey!),
                new RuleInputKey(entity.ExpressionRightKey!),
                (RuleArithmeticOperator)entity.ArithmeticOperator!.Value,
                (RuleComparisonOperator)entity.ComparisonOperator!.Value,
                RuleValue.FromNumber(entity.ExpressionExpectedNumber ?? 0m)),

            RuleKind.Composite => RuleCondition.Composite(
                (RuleCompositeOperator)entity.CompositeOperator!.Value),

            _ => throw new RuleValidationException(
                $"Unsupported rule kind '{kind}' during persistence mapping.")
        };
    }

    private static RuleValue MapExpectedValue(PlatformRuleDefinitionEntity entity)
    {
        if (!entity.ExpectedValueKind.HasValue)
        {
            throw new RuleValidationException("Expected value kind is missing.");
        }

        var valueKind = (RuleValueKind)entity.ExpectedValueKind.Value;

        return valueKind switch
        {
            RuleValueKind.Boolean => RuleValue.FromBoolean(entity.ExpectedBoolean ?? false),
            RuleValueKind.Number => RuleValue.FromNumber(entity.ExpectedNumber ?? 0m),
            RuleValueKind.Text => RuleValue.FromText(entity.ExpectedText ?? string.Empty),
            _ => throw new RuleValidationException("Unsupported expected value kind.")
        };
    }
}
