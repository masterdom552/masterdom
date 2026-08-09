using System.Text.Json;
using Masterdom.Modules.UtilityRating.Application.Support;
using Masterdom.Modules.UtilityRating.Domain.Entities.UtilityRating;
using Masterdom.Platform.Configuration;
using Masterdom.Platform.Events;
using Masterdom.Platform.Metadata;
using Masterdom.Platform.Rules;
using Masterdom.Platform.Workflow;
using UtilityRatingAggregate = Masterdom.Modules.UtilityRating.Domain.Entities.UtilityRating.UtilityRating;

namespace Masterdom.Infrastructure.Persistence.UtilityRating;

public sealed class UtilityRatingPlatformOrchestrator : IUtilityRatingPlatformOrchestrator
{
    private const string ModuleId = "utilityrating";
    private static readonly ConfigurationKey TariffConfigurationKey = new("utilityrating.tariff.default");

    private readonly IConfigurationResolver _configurationResolver;
    private readonly IMetadataResolver _metadataResolver;
    private readonly IRuleResolver _ruleResolver;
    private readonly IWorkflowResolver _workflowResolver;
    private readonly IDomainEventPublisher _domainEventPublisher;

    public UtilityRatingPlatformOrchestrator(
        IConfigurationResolver configurationResolver,
        IMetadataResolver metadataResolver,
        IRuleResolver ruleResolver,
        IWorkflowResolver workflowResolver,
        IDomainEventPublisher domainEventPublisher)
    {
        _configurationResolver = configurationResolver ?? throw new ArgumentNullException(nameof(configurationResolver));
        _metadataResolver = metadataResolver ?? throw new ArgumentNullException(nameof(metadataResolver));
        _ruleResolver = ruleResolver ?? throw new ArgumentNullException(nameof(ruleResolver));
        _workflowResolver = workflowResolver ?? throw new ArgumentNullException(nameof(workflowResolver));
        _domainEventPublisher = domainEventPublisher ?? throw new ArgumentNullException(nameof(domainEventPublisher));
    }

    /// <summary>
    /// Resolves and adapts the effective governed tariff configuration.
    /// </summary>
    public TariffSchedule? ResolveTariffSchedule(string tariffCode, Guid meterId, DateTime asOfUtc)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(tariffCode);

        try
        {
            var resolved = _configurationResolver.Resolve(
                TariffConfigurationKey,
                new ConfigurationResolutionRequest
                {
                    ModuleId = ModuleId,
                    PropertyId = meterId.ToString("N"),
                    AsOfUtc = asOfUtc
                });
            var payload = JsonSerializer.Deserialize<TariffConfigurationPayload>(
                resolved.Record.Value.Value,
                JsonSerializerOptions.Web);
            if (payload is null
                || !string.Equals(payload.TariffCode, tariffCode, StringComparison.OrdinalIgnoreCase))
            {
                return null;
            }

            var tariffReference = TariffReference.Create(
                payload.TariffCode,
                resolved.Record.Version.Value);
            var utilityRate = UtilityRate.Create(
                tariffReference,
                FixedCharge.Create(payload.FixedCharge),
                VariableCharge.Create(payload.VariableCharge),
                MinimumCharge.Create(payload.MinimumCharge),
                AdjustmentComponent.Create(payload.Adjustment));
            var effectiveTo = resolved.Record.Period.EffectiveToUtc.HasValue
                ? DateOnly.FromDateTime(resolved.Record.Period.EffectiveToUtc.Value.AddTicks(-1))
                : (DateOnly?)null;

            return TariffSchedule.Create(
                tariffReference,
                DateOnly.FromDateTime(resolved.Record.Period.EffectiveFromUtc),
                effectiveTo,
                utilityRate);
        }
        catch (Exception ex) when (ex is PlatformConfigurationValidationException
            or JsonException
            or ArgumentException
            or InvalidOperationException)
        {
            return null;
        }
    }

    public void OnRatingMutated(UtilityRatingAggregate rating, string operationName)
    {
        ArgumentNullException.ThrowIfNull(rating);
        ArgumentException.ThrowIfNullOrWhiteSpace(operationName);

        var nowUtc = DateTime.UtcNow;
        var meterId = rating.MeterReference.MeterId.ToString("N");

        TryResolveMetadata(nowUtc);
        TryEvaluateRules(nowUtc, meterId);
        TryExecuteWorkflow(nowUtc, meterId);

        _domainEventPublisher.Publish(
            rating,
            new EventContext
            {
                ModuleId = ModuleId,
                AggregateId = rating.Id.ToString(),
                AggregateType = nameof(UtilityRatingAggregate),
                CorrelationId = operationName,
                OccurredAtUtc = nowUtc
            });
    }

    private void TryResolveMetadata(DateTime asOfUtc)
    {
        try
        {
            _metadataResolver.Resolve(
                new MetadataKey("utilityrating.aggregate.rating"),
                MetadataScope.Module(ModuleId),
                asOfUtc);
        }
        catch
        {
            // Best-effort platform integration must not alter aggregate behavior.
        }
    }

    private void TryEvaluateRules(DateTime asOfUtc, string meterId)
    {
        try
        {
            _ruleResolver.Evaluate(
                new RuleSetKey("rules.utilityrating.ratings"),
                RuleScope.Create(RuleScopeKind.Module, ModuleId),
                new RuleContext
                {
                    ModuleId = ModuleId,
                    PropertyId = meterId,
                    AsOfUtc = asOfUtc
                },
                new RuleInput(Array.Empty<RuleInputItem>()));
        }
        catch
        {
            // Best-effort platform integration must not alter aggregate behavior.
        }
    }

    private void TryExecuteWorkflow(DateTime asOfUtc, string meterId)
    {
        try
        {
            _workflowResolver.Execute(
                new WorkflowKey("workflow.utilityrating.ratings"),
                WorkflowScope.Create(WorkflowScopeKind.Module, ModuleId),
                new WorkflowContext
                {
                    ModuleId = ModuleId,
                    PropertyId = meterId,
                    AsOfUtc = asOfUtc
                });
        }
        catch
        {
            // Best-effort platform integration must not alter aggregate behavior.
        }
    }

    private sealed record TariffConfigurationPayload(
        string TariffCode,
        decimal FixedCharge,
        decimal VariableCharge,
        decimal MinimumCharge,
        decimal Adjustment);
}
