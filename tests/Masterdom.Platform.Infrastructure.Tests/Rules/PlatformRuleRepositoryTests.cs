using System;
using Masterdom.Infrastructure.Persistence;
using Masterdom.Infrastructure.Persistence.Rules;
using Masterdom.Platform.Rules;
using Microsoft.EntityFrameworkCore;

namespace Masterdom.Platform.Tests.Rules;

public sealed class PlatformRuleRepositoryTests
{
    [Fact]
    public void GetAll_ShouldMapPersistedEntitiesToRuleDefinitions()
    {
        var options = new DbContextOptionsBuilder<MasterdomDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        using var dbContext = new MasterdomDbContext(options);

        var changedAt = DateTime.SpecifyKind(new DateTime(2026, 2, 1), DateTimeKind.Utc);
        var setId = Guid.NewGuid();
        var ruleId = Guid.NewGuid();

        dbContext.PlatformRuleSets.Add(new PlatformRuleSetEntity
        {
            Id = setId,
            Key = "rules.people.default",
            Name = "People Rules",
            Description = "Default people rules",
            Category = (int)RuleCategory.Validation,
            ScopeKind = (int)RuleScopeKind.Module,
            ScopeIdentifier = "people",
            Version = 1,
            EffectiveFromUtc = changedAt,
            EffectiveToUtc = null,
            IsDeprecated = false,
            ReplacedByKey = null,
            Compatibility = "v1",
            ChangedBy = "tester",
            ChangedAtUtc = changedAt
        });

        dbContext.PlatformRuleDefinitions.Add(new PlatformRuleDefinitionEntity
        {
            Id = ruleId,
            RuleSetId = setId,
            ParentRuleId = null,
            Key = "rule.people.is-enabled",
            Name = "Is Enabled",
            Description = "Checks enabled flag",
            Kind = (int)RuleKind.Boolean,
            Category = (int)RuleCategory.Validation,
            Severity = (int)RuleSeverity.Error,
            Priority = 5,
            ScopeKind = (int)RuleScopeKind.Module,
            ScopeIdentifier = "people",
            Version = 1,
            EffectiveFromUtc = changedAt,
            EffectiveToUtc = null,
            IsDeprecated = false,
            ReplacedByKey = null,
            Compatibility = "v1",
            ChangedBy = "tester",
            ChangedAtUtc = changedAt,
            InputKey = "isEnabled",
            ComparisonOperator = (int)RuleComparisonOperator.Equal,
            ExpectedValueKind = (int)RuleValueKind.Boolean,
            ExpectedBoolean = true
        });

        dbContext.SaveChanges();

        var repository = new PlatformRuleRepository(dbContext);

        var ruleSets = repository.GetAllRuleSets();
        var rules = repository.GetAllRules();

        var set = Assert.Single(ruleSets);
        Assert.Equal("rules.people.default", set.Key.Value);
        Assert.Equal(RuleScopeKind.Module, set.Scope.Kind);
        Assert.Equal("people", set.Scope.Identifier);

        var rule = Assert.Single(rules);
        Assert.Equal(ruleId, rule.Id.Value);
        Assert.Equal(setId, rule.RuleSetId.Value);
        Assert.Equal(RuleKind.Boolean, rule.Kind);
        Assert.Equal("rule.people.is-enabled", rule.Key.Value);
    }
}
