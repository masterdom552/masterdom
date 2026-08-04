using System;
using Masterdom.Platform.Rules;

namespace Masterdom.Platform.Tests.Rules;

public sealed class RuleValidationTests
{
    [Fact]
    public void ValidateAll_WhenDuplicateRuleIdentifiersExist_ShouldThrow()
    {
        var scope = RuleScope.Create(RuleScopeKind.Module, "billing");
        var now = DateTime.SpecifyKind(new DateTime(2026, 1, 1), DateTimeKind.Utc);
        var set = CreateRuleSet(scope, now);
        var id = new RuleId(Guid.NewGuid());

        var first = CreateRule(id, set.Id, "rule.one", scope, now, null);
        var second = CreateRule(id, set.Id, "rule.two", scope, now, null);

        Assert.Throws<RuleValidationException>(() =>
            RuleValidation.ValidateAll(new[] { set }, new[] { first, second }));
    }

    [Fact]
    public void ValidateAll_WhenRuleSetReferenceIsMissing_ShouldThrow()
    {
        var scope = RuleScope.Create(RuleScopeKind.Module, "billing");
        var now = DateTime.SpecifyKind(new DateTime(2026, 1, 1), DateTimeKind.Utc);

        var missingSetId = new RuleSetId(Guid.NewGuid());
        var rule = CreateRule(new RuleId(Guid.NewGuid()), missingSetId, "rule.missing-set", scope, now, null);

        Assert.Throws<RuleValidationException>(() =>
            RuleValidation.ValidateAll(Array.Empty<RuleSetDefinition>(), new[] { rule }));
    }

    [Fact]
    public void ValidateAll_WhenParentReferenceIsCircular_ShouldThrow()
    {
        var scope = RuleScope.Create(RuleScopeKind.Module, "billing");
        var now = DateTime.SpecifyKind(new DateTime(2026, 1, 1), DateTimeKind.Utc);
        var set = CreateRuleSet(scope, now);

        var firstId = new RuleId(Guid.NewGuid());
        var secondId = new RuleId(Guid.NewGuid());

        var first = CreateRule(firstId, set.Id, "rule.first", scope, now, secondId);
        var second = CreateRule(secondId, set.Id, "rule.second", scope, now, firstId);

        Assert.Throws<RuleValidationException>(() =>
            RuleValidation.ValidateAll(new[] { set }, new[] { first, second }));
    }

    [Fact]
    public void ValidateAll_WhenScopeIsInvalidForCategory_ShouldThrow()
    {
        var now = DateTime.SpecifyKind(new DateTime(2026, 1, 1), DateTimeKind.Utc);
        var setScope = RuleScope.Create(RuleScopeKind.Module, "billing");
        var invalidRuleScope = RuleScope.Create(RuleScopeKind.Entity, "people.person");

        var set = CreateRuleSet(setScope, now);

        var rule = new RuleDefinition(
            new RuleId(Guid.NewGuid()),
            set.Id,
            new RuleKey("rule.pricing.invalid-scope"),
            "Invalid pricing scope",
            null,
            RuleKind.Boolean,
            RuleCondition.Boolean(new RuleInputKey("enabled"), true),
            RuleCategory.Pricing,
            RuleSeverity.Error,
            new RulePriority(10),
            invalidRuleScope,
            new RuleVersion(1),
            new RuleEffectivePeriod(now, null),
            null,
            false,
            null,
            null,
            "tester",
            now);

        Assert.Throws<RuleValidationException>(() =>
            RuleValidation.ValidateAll(new[] { set }, new[] { rule }));
    }

    private static RuleSetDefinition CreateRuleSet(RuleScope scope, DateTime now)
    {
        return new RuleSetDefinition(
            new RuleSetId(Guid.NewGuid()),
            new RuleSetKey("rules.validation"),
            "Validation Rules",
            null,
            RuleCategory.Validation,
            scope,
            new RuleVersion(1),
            new RuleEffectivePeriod(now, null),
            false,
            null,
            null,
            "tester",
            now);
    }

    private static RuleDefinition CreateRule(
        RuleId id,
        RuleSetId setId,
        string key,
        RuleScope scope,
        DateTime now,
        RuleId? parentRuleId)
    {
        return new RuleDefinition(
            id,
            setId,
            new RuleKey(key),
            key,
            null,
            RuleKind.Boolean,
            RuleCondition.Boolean(new RuleInputKey("enabled"), true),
            RuleCategory.Validation,
            RuleSeverity.Error,
            new RulePriority(10),
            scope,
            new RuleVersion(1),
            new RuleEffectivePeriod(now, null),
            parentRuleId,
            false,
            null,
            null,
            "tester",
            now);
    }
}
