using Masterdom.Platform.Diagnostics;
using Masterdom.Platform.Configuration;
using Masterdom.Platform.Core;
using Masterdom.Platform.Events;
using Masterdom.Platform.Metadata;
using Masterdom.Platform.Modules;
using Masterdom.Platform.Rules;
using Masterdom.Platform.Workflow;
using Masterdom.Platform.Services;

namespace Masterdom.TestKit.Platform;

/// <summary>
/// Minimal platform context for unit tests.
/// </summary>
public sealed class FakePlatformContext : IPlatformContext
{
    public required IModuleCatalog Modules { get; init; }

    public required IPlatformServiceRegistry Services { get; init; }

    public required IDiagnostics Diagnostics { get; init; }

    public IConfigurationResolver Configuration { get; init; } =
        new ConfigurationResolver(new InMemoryConfigurationRepository());

    public IMetadataResolver Metadata { get; init; } =
        new MetadataResolver(new InMemoryMetadataRepository());

    public IRuleResolver Rules { get; init; } =
        new RuleResolver(
            new InMemoryRuleRepository(),
            new ConfigurationResolver(new InMemoryConfigurationRepository()),
            new MetadataResolver(new InMemoryMetadataRepository()));

    public IWorkflowResolver Workflows { get; init; } =
        new WorkflowResolver(
            new InMemoryWorkflowRepository(),
            new ConfigurationResolver(new InMemoryConfigurationRepository()),
            new MetadataResolver(new InMemoryMetadataRepository()),
            new RuleResolver(
                new InMemoryRuleRepository(),
                new ConfigurationResolver(new InMemoryConfigurationRepository()),
                new MetadataResolver(new InMemoryMetadataRepository())),
            new InMemoryWorkflowStateStore());

    public IEventPublisher Events { get; init; } =
        new EventPublisher(
            new EventStore(new InMemoryEventRepository()),
            new EventDispatcher(
                new EventHandlerResolver(new EventRegistry())));

    public IDomainEventPublisher DomainEvents { get; init; } =
        new DomainEventPublisher(
            new DomainEventAdapter(),
            new EventPublisher(
                new EventStore(new InMemoryEventRepository()),
                new EventDispatcher(
                    new EventHandlerResolver(new EventRegistry()))));
}
