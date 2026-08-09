namespace Masterdom.Modules.SubsidyOptimization.Application.Maximizer;

internal static class SubsidyOptimizerConfigurationValidator
{
    public static void Validate(ResolvedSubsidyOptimizerConfiguration configuration, DateTime asOfUtc)
    {
        ArgumentNullException.ThrowIfNull(configuration);

        ValidateIdentity(configuration.PolicyIdentity, asOfUtc);
        ValidateIdentity(configuration.ModelIdentity, asOfUtc);
        ValidateIdentity(configuration.StrategyIdentity, asOfUtc);
        ValidatePolicy(configuration.Policy);
        ValidateModel(configuration.Model, configuration.Policy);
        ValidateStrategy(configuration.Strategy);
    }

    private static void ValidateIdentity(ResolvedConfigurationIdentity identity, DateTime asOfUtc)
    {
        if (string.IsNullOrWhiteSpace(identity.ConfigurationKey)
            || string.IsNullOrWhiteSpace(identity.DefinitionId)
            || identity.Version <= 0
            || identity.EffectiveFromUtc.Kind != DateTimeKind.Utc
            || identity.EffectiveToUtc?.Kind is not (null or DateTimeKind.Utc)
            || identity.EffectiveToUtc <= identity.EffectiveFromUtc
            || asOfUtc < identity.EffectiveFromUtc
            || asOfUtc >= identity.EffectiveToUtc)
        {
            throw new InvalidOperationException("Governed configuration identity, version, or effective period is invalid.");
        }
    }

    private static void ValidatePolicy(SubsidyPolicyConfiguration policy)
    {
        if (string.IsNullOrWhiteSpace(policy.PolicyCode)
            || policy.Slabs.Count == 0
            || policy.SanctionedLoadLimit <= 0m
            || policy.SanctionedLoadPenaltyPerUnit < 0m
            || policy.EligibleMeterTypes.Count == 0
            || policy.EligibleMeterTypes.Any(string.IsNullOrWhiteSpace)
            || policy.EligibleMeterTypes.Distinct(StringComparer.OrdinalIgnoreCase).Count() != policy.EligibleMeterTypes.Count)
        {
            throw new InvalidOperationException("Governed subsidy policy contains invalid identifiers, load limits, penalties, or meter eligibility.");
        }

        decimal previousBoundary = 0m;
        foreach (var slab in policy.Slabs)
        {
            if (slab.MaximumUnits <= previousBoundary || slab.SubsidyAmount < 0m)
            {
                throw new InvalidOperationException("Governed subsidy slabs must have strictly increasing boundaries and nonnegative amounts.");
            }

            previousBoundary = slab.MaximumUnits;
        }
    }

    private static void ValidateModel(OptimizationModelConfiguration model, SubsidyPolicyConfiguration policy)
    {
        var weights = new[] { model.SubsidyWeight, model.CostWeight, model.LoadImpactWeight, model.RiskWeight };
        if (string.IsNullOrWhiteSpace(model.ModelCode)
            || weights.Any(x => x < 0m)
            || weights.All(x => x == 0m)
            || model.BoundaryTolerance <= 0m
            || model.MaximumScenarioCount <= 0)
        {
            throw new InvalidOperationException("Governed optimization model contains invalid weights, tolerance, or scenario limits.");
        }

        var mandatoryCandidateCount = policy.Slabs
            .Where(x => x.IsCliff)
            .SelectMany(x => new[]
            {
                decimal.Max(0m, x.MaximumUnits - model.BoundaryTolerance),
                x.MaximumUnits,
                x.MaximumUnits + model.BoundaryTolerance
            })
            .Distinct()
            .Count();

        if (model.MaximumScenarioCount < mandatoryCandidateCount)
        {
            throw new InvalidOperationException("MaximumScenarioCount cannot represent every mandatory subsidy cliff candidate.");
        }
    }

    private static void ValidateStrategy(OptimizationStrategyConfiguration strategy)
    {
        if (string.IsNullOrWhiteSpace(strategy.StrategyCode)
            || strategy.ConsumptionFactors.Count == 0
            || strategy.ConsumptionFactors.Any(x => x <= 0m)
            || strategy.ConsumptionFactors.Distinct().Count() != strategy.ConsumptionFactors.Count
            || strategy.MaximumCrossMeterMovementFraction < 0m
            || strategy.MaximumCrossMeterMovementFraction > 1m
            || (!strategy.PermitCrossMeterMovement && strategy.MaximumCrossMeterMovementFraction != 0m)
            || (strategy.PermitCrossMeterMovement && strategy.MaximumCrossMeterMovementFraction == 0m))
        {
            throw new InvalidOperationException("Governed optimization strategy contains invalid factors or movement bounds.");
        }
    }
}
