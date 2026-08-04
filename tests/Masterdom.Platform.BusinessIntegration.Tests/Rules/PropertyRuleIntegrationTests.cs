using Masterdom.Modules.Properties.Domain.Entities.Property;
using Masterdom.Platform.Configuration;
using Masterdom.Platform.Metadata;
using Masterdom.Platform.Rules;

namespace Masterdom.Platform.Tests.Rules;

public sealed class PropertyRuleIntegrationTests
{
    [Fact]
    public void Evaluate_ShouldAllowArchiveDecision_ForVacantUnitPolicy()
    {
        var asOfUtc = DateTime.SpecifyKind(new DateTime(2026, 7, 27), DateTimeKind.Utc);

        var property = Property.Create(
            new PropertyCode("RULE-01"),
            new PropertyName("Rule Driven Property"),
            PropertyType.Residential);

        property.CreateUnit(new UnitCode("A-1"), "A-1", UnitType.Room);

        var scope = RuleScope.Create(RuleScopeKind.Module, "properties");
        var ruleSet = new RuleSetDefinition(
            new RuleSetId(Guid.NewGuid()),
            new RuleSetKey("rules.properties.archive"),
            "Property Archive Rules",
            "Determines whether archive operation is allowed.",
            RuleCategory.Validation,
            scope,
            new RuleVersion(1),
            new RuleEffectivePeriod(asOfUtc.AddDays(-1), null),
            false,
            null,
            null,
            "tester",
            asOfUtc);

        var rule = new RuleDefinition(
            new RuleId(Guid.NewGuid()),
            ruleSet.Id,
            new RuleKey("rule.properties.units-empty"),
            "Units empty check",
            "Archive requires no units.",
            RuleKind.Boolean,
            RuleCondition.Boolean(new RuleInputKey("unitsEmpty"), false),
            RuleCategory.Validation,
            RuleSeverity.Error,
            new RulePriority(1),
            scope,
            new RuleVersion(1),
            new RuleEffectivePeriod(asOfUtc.AddDays(-1), null),
            null,
            false,
            null,
            null,
            "tester",
            asOfUtc);

        var resolver = new RuleResolver(
            new InMemoryRuleRepository(new[] { ruleSet }, new[] { rule }),
            new ConfigurationResolver(new InMemoryConfigurationRepository()),
            new MetadataResolver(new InMemoryMetadataRepository()));

        var output = resolver.Evaluate(
            ruleSet.Key,
            scope,
            new RuleContext
            {
                ModuleId = "properties",
                AsOfUtc = asOfUtc
            },
            new RuleInput(new[]
            {
                new RuleInputItem
                {
                    Key = new RuleInputKey("unitsEmpty"),
                    Value = RuleValue.FromBoolean(!property.Units.Any())
                }
            }));

        Assert.True(output.Passed);
        Assert.Single(output.Results);
    }
}
