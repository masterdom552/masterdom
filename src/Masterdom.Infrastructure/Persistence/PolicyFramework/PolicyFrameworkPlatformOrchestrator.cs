using Masterdom.Modules.PolicyFramework.Application.Support;
using Masterdom.Platform.Configuration;
using Masterdom.Platform.Events;
using Masterdom.Platform.Metadata;
using PolicyAggregate = Masterdom.Modules.PolicyFramework.Domain.Entities.PolicyFramework.Policy;

namespace Masterdom.Infrastructure.Persistence.PolicyFramework;

public sealed class PolicyFrameworkPlatformOrchestrator : IPolicyFrameworkPlatformOrchestrator
{
    private const string ModuleId = "policyframework";

    private readonly IConfigurationResolver _configurationResolver;
    private readonly IMetadataResolver _metadataResolver;
    private readonly IDomainEventPublisher _domainEventPublisher;

    public PolicyFrameworkPlatformOrchestrator(
        IConfigurationResolver configurationResolver,
        IMetadataResolver metadataResolver,
        IDomainEventPublisher domainEventPublisher)
    {
        _configurationResolver = configurationResolver ?? throw new ArgumentNullException(nameof(configurationResolver));
        _metadataResolver = metadataResolver ?? throw new ArgumentNullException(nameof(metadataResolver));
        _domainEventPublisher = domainEventPublisher ?? throw new ArgumentNullException(nameof(domainEventPublisher));
    }

    public void OnPolicyMutated(PolicyAggregate policy, string operationName)
    {
        ArgumentNullException.ThrowIfNull(policy);
        ArgumentException.ThrowIfNullOrWhiteSpace(operationName);

        var nowUtc = DateTime.UtcNow;

        TryResolveConfiguration(nowUtc);
        TryResolveMetadata(nowUtc);

        _domainEventPublisher.Publish(
            policy,
            new EventContext
            {
                ModuleId = ModuleId,
                AggregateId = policy.Id.ToString(),
                AggregateType = nameof(PolicyAggregate),
                CorrelationId = operationName,
                OccurredAtUtc = nowUtc
            });
    }

    private void TryResolveConfiguration(DateTime asOfUtc)
    {
        try
        {
            _configurationResolver.Resolve(
                new ConfigurationKey("policyframework.selection.default"),
                new ConfigurationResolutionRequest
                {
                    ModuleId = ModuleId,
                    AsOfUtc = asOfUtc
                });
        }
        catch
        {
            // Best-effort platform integration must not alter aggregate behavior.
        }
    }

    private void TryResolveMetadata(DateTime asOfUtc)
    {
        try
        {
            _metadataResolver.Resolve(
                new MetadataKey("policyframework.aggregate.policy"),
                MetadataScope.Module(ModuleId),
                asOfUtc);
        }
        catch
        {
            // Best-effort platform integration must not alter aggregate behavior.
        }
    }
}
