using System;
using Masterdom.Platform.Configuration;
using Masterdom.Platform.Diagnostics;
using Masterdom.Platform.Events;
using Masterdom.Platform.Metadata;
using Masterdom.Platform.Modules;
using Masterdom.Platform.Rules;
using Masterdom.Platform.Workflow;
using Masterdom.Platform.Services;

namespace Masterdom.Platform.Core;

/// <summary>
/// Represents the platform context that is shared with modules.
/// </summary>
public sealed class PlatformContext : IPlatformContext
{
    /// <summary>
    /// Initializes a new instance of the <see cref="PlatformContext"/> class.
    /// </summary>
    /// <param name="modules">The platform module catalog.</param>
    /// <param name="services">The platform service registry.</param>
    /// <param name="diagnostics">The diagnostics sink.</param>
    /// <param name="configuration">The configuration resolver.</param>
    /// <param name="metadata">The metadata resolver.</param>
    /// <param name="rules">The rules resolver.</param>
    /// <param name="workflows">The workflows resolver.</param>
    /// <param name="events">The platform event publisher.</param>
    /// <param name="domainEvents">The domain event publisher adapter.</param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="modules"/> is <see langword="null"/>.
    /// </exception>
    public PlatformContext(
        IModuleCatalog modules,
        IPlatformServiceRegistry services,
        IDiagnostics diagnostics,
        IConfigurationResolver configuration,
        IMetadataResolver metadata,
        IRuleResolver rules,
        IWorkflowResolver workflows,
        IEventPublisher events,
        IDomainEventPublisher domainEvents)
    {
        Modules = modules ?? throw new ArgumentNullException(nameof(modules));
        Services = services ?? throw new ArgumentNullException(nameof(services));
        Diagnostics = diagnostics ?? throw new ArgumentNullException(nameof(diagnostics));
        Configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        Metadata = metadata ?? throw new ArgumentNullException(nameof(metadata));
        Rules = rules ?? throw new ArgumentNullException(nameof(rules));
        Workflows = workflows ?? throw new ArgumentNullException(nameof(workflows));
        Events = events ?? throw new ArgumentNullException(nameof(events));
        DomainEvents = domainEvents ?? throw new ArgumentNullException(nameof(domainEvents));
    }

    /// <inheritdoc/>
    public IModuleCatalog Modules { get; }

    /// <inheritdoc/>
    public IPlatformServiceRegistry Services { get; }

    /// <inheritdoc/>
    public IDiagnostics Diagnostics { get; }

    /// <inheritdoc/>
    public IConfigurationResolver Configuration { get; }

    /// <inheritdoc/>
    public IMetadataResolver Metadata { get; }

    /// <inheritdoc/>
    public IRuleResolver Rules { get; }

    /// <inheritdoc/>
    public IWorkflowResolver Workflows { get; }

    /// <inheritdoc/>
    public IEventPublisher Events { get; }

    /// <inheritdoc/>
    public IDomainEventPublisher DomainEvents { get; }
}
