using System;
using System.Collections.Generic;
using Masterdom.Platform.Configuration;
using Masterdom.Platform.Metadata;
using Masterdom.Platform.Rules;

namespace Masterdom.Platform.Tests.Rules;

public sealed class RuleResolverTests
{
    [Fact]
    public void Evaluate_WhenBooleanRuleMatchesInput_ShouldPass()
    {
        var asOfUtc = DateTime.SpecifyKind(new DateTime(2026, 1, 5), DateTimeKind.Utc);
        var scope = RuleScope.Create(RuleScopeKind.Module, "billing");

        var (ruleSet, rule) = CreateBooleanRuleSet(scope, asOfUtc);

        var repository = new InMemoryRuleRepository(
            new[] { ruleSet },
            new[] { rule });

        var resolver = new RuleResolver(
            repository,
            new ConfigurationResolver(new InMemoryConfigurationRepository()),
            new MetadataResolver(new InMemoryMetadataRepository()));

        var output = resolver.Evaluate(
            ruleSet.Key,
            scope,
            new RuleContext
            {
                ModuleId = "billing",
                AsOfUtc = asOfUtc
            },
            new RuleInput(new[]
            {
                new RuleInputItem
                {
                    Key = new RuleInputKey("requiresApproval"),
                    Value = RuleValue.FromBoolean(true)
                }
            }));

        var result = Assert.Single(output.Results);

        Assert.Equal(RuleResultStatus.Passed, result.Status);
        Assert.True(output.Passed);
    }

    [Fact]
    public void Evaluate_WhenRuleUsesConfigurationPrefix_ShouldResolveFromConfiguration()
    {
        var asOfUtc = DateTime.SpecifyKind(new DateTime(2026, 1, 5), DateTimeKind.Utc);
        var scope = RuleScope.Create(RuleScopeKind.Module, "billing");

        var ruleSet = CreateRuleSet(scope, asOfUtc);
        var rule = CreateRule(
            ruleSet.Id,
            new RuleKey("rule.billing.min-score"),
            RuleKind.Comparison,
            RuleCondition.Comparison(
                new RuleInputKey("config:billing.min-score"),
                RuleComparisonOperator.GreaterThanOrEqual,
                RuleValue.FromNumber(10m)),
            RuleSeverity.Warning,
            10,
            scope,
            asOfUtc,
            null);

        var configKey = new ConfigurationKey("billing.min-score");
        var configRepository = new InMemoryConfigurationRepository(new List<ConfigurationRecord>
        {
            new(
                new ConfigurationId(Guid.NewGuid()),
                configKey,
                ConfigurationScope.Module("billing"),
                new ConfigurationVersion(1),
                new ConfigurationValue("15"),
                new EffectivePeriod(asOfUtc.AddDays(-10), null),
                "tester",
                "seed",
                asOfUtc.AddDays(-10))
        });

        var resolver = new RuleResolver(
            new InMemoryRuleRepository(new[] { ruleSet }, new[] { rule }),
            new ConfigurationResolver(configRepository),
            new MetadataResolver(new InMemoryMetadataRepository()));

        var output = resolver.Evaluate(
            ruleSet.Key,
            scope,
            new RuleContext
            {
                ModuleId = "billing",
                AsOfUtc = asOfUtc
            },
            new RuleInput(Array.Empty<RuleInputItem>()));

        var result = Assert.Single(output.Results);
        Assert.Equal(RuleResultStatus.Passed, result.Status);
    }

    [Fact]
    public void Evaluate_WhenRuleUsesMetadataPrefix_ShouldResolveFromMetadata()
    {
        var asOfUtc = DateTime.SpecifyKind(new DateTime(2026, 1, 5), DateTimeKind.Utc);
        var scope = RuleScope.Create(RuleScopeKind.Module, "people");

        var ruleSet = CreateRuleSet(scope, asOfUtc);
        var rule = CreateRule(
            ruleSet.Id,
            new RuleKey("rule.people.metadata-name"),
            RuleKind.Comparison,
            RuleCondition.Comparison(
                new RuleInputKey("metadata:module.people"),
                RuleComparisonOperator.Equal,
                RuleValue.FromText("people")),
            RuleSeverity.Info,
            10,
            scope,
            asOfUtc,
            null);

        var metadataRepository = new InMemoryMetadataRepository(new List<MetadataDefinition>
        {
            new(
                new MetadataId(Guid.NewGuid()),
                new MetadataKey("module.people"),
                MetadataCategory.Module,
                MetadataScope.Module("people"),
                new MetadataVersion(1),
                new MetadataEffectivePeriod(asOfUtc.AddDays(-10), null),
                "people",
                "People module",
                null,
                false,
                null,
                null,
                "tester",
                asOfUtc.AddDays(-10))
        });

        var resolver = new RuleResolver(
            new InMemoryRuleRepository(new[] { ruleSet }, new[] { rule }),
            new ConfigurationResolver(new InMemoryConfigurationRepository()),
            new MetadataResolver(metadataRepository));

        var output = resolver.Evaluate(
            ruleSet.Key,
            scope,
            new RuleContext
            {
                ModuleId = "people",
                AsOfUtc = asOfUtc
            },
            new RuleInput(Array.Empty<RuleInputItem>()));

        var result = Assert.Single(output.Results);
        Assert.Equal(RuleResultStatus.Passed, result.Status);
    }

    [Fact]
    public void Evaluate_WhenCompositeRuleUsesAllAndOneChildFails_ShouldFailParent()
    {
        var asOfUtc = DateTime.SpecifyKind(new DateTime(2026, 1, 5), DateTimeKind.Utc);
        var scope = RuleScope.Create(RuleScopeKind.Module, "crm");

        var ruleSet = CreateRuleSet(scope, asOfUtc);

        var root = CreateRule(
            ruleSet.Id,
            new RuleKey("rule.crm.composite"),
            RuleKind.Composite,
            RuleCondition.Composite(RuleCompositeOperator.All),
            RuleSeverity.Error,
            1,
            scope,
            asOfUtc,
            null);

        var childPass = CreateRule(
            ruleSet.Id,
            new RuleKey("rule.crm.child-pass"),
            RuleKind.Boolean,
            RuleCondition.Boolean(new RuleInputKey("isActive"), true),
            RuleSeverity.Error,
            2,
            scope,
            asOfUtc,
            root.Id);

        var childFail = CreateRule(
            ruleSet.Id,
            new RuleKey("rule.crm.child-fail"),
            RuleKind.Boolean,
            RuleCondition.Boolean(new RuleInputKey("isVerified"), true),
            RuleSeverity.Error,
            3,
            scope,
            asOfUtc,
            root.Id);

        var resolver = new RuleResolver(
            new InMemoryRuleRepository(
                new[] { ruleSet },
                new[] { root, childPass, childFail }),
            new ConfigurationResolver(new InMemoryConfigurationRepository()),
            new MetadataResolver(new InMemoryMetadataRepository()));

        var output = resolver.Evaluate(
            ruleSet.Key,
            scope,
            new RuleContext
            {
                ModuleId = "crm",
                AsOfUtc = asOfUtc
            },
            new RuleInput(new[]
            {
                new RuleInputItem
                {
                    Key = new RuleInputKey("isActive"),
                    Value = RuleValue.FromBoolean(true)
                },
                new RuleInputItem
                {
                    Key = new RuleInputKey("isVerified"),
                    Value = RuleValue.FromBoolean(false)
                }
            }));

        Assert.Equal(3, output.Results.Count);
        Assert.Contains(output.Results, x => x.RuleKey.Value == "rule.crm.child-pass" && x.Status == RuleResultStatus.Passed);
        Assert.Contains(output.Results, x => x.RuleKey.Value == "rule.crm.child-fail" && x.Status == RuleResultStatus.Failed);
        Assert.Contains(output.Results, x => x.RuleKey.Value == "rule.crm.composite" && x.Status == RuleResultStatus.Failed);
    }

    private static (RuleSetDefinition RuleSet, RuleDefinition Rule) CreateBooleanRuleSet(
        RuleScope scope,
        DateTime asOfUtc)
    {
        var ruleSet = CreateRuleSet(scope, asOfUtc);
        var rule = CreateRule(
            ruleSet.Id,
            new RuleKey("rule.billing.requires-approval"),
            RuleKind.Boolean,
            RuleCondition.Boolean(new RuleInputKey("requiresApproval"), true),
            RuleSeverity.Error,
            10,
            scope,
            asOfUtc,
            null);

        return (ruleSet, rule);
    }

    private static RuleSetDefinition CreateRuleSet(RuleScope scope, DateTime asOfUtc)
    {
        return new RuleSetDefinition(
            new RuleSetId(Guid.NewGuid()),
            new RuleSetKey("rules.default"),
            "Default Rules",
            "Test rule set",
            RuleCategory.Validation,
            scope,
            new RuleVersion(1),
            new RuleEffectivePeriod(asOfUtc.AddDays(-10), null),
            false,
            null,
            null,
            "tester",
            asOfUtc);
    }

    private static RuleDefinition CreateRule(
        RuleSetId ruleSetId,
        RuleKey key,
        RuleKind kind,
        RuleCondition condition,
        RuleSeverity severity,
        int priority,
        RuleScope scope,
        DateTime asOfUtc,
        RuleId? parentRuleId)
    {
        return new RuleDefinition(
            new RuleId(Guid.NewGuid()),
            ruleSetId,
            key,
            key.Value,
            "Test rule",
            kind,
            condition,
            RuleCategory.Validation,
            severity,
            new RulePriority(priority),
            scope,
            new RuleVersion(1),
            new RuleEffectivePeriod(asOfUtc.AddDays(-10), null),
            parentRuleId,
            false,
            null,
            null,
            "tester",
            asOfUtc);
    }
}
