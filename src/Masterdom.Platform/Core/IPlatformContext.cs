using Masterdom.Platform.Diagnostics;
using Masterdom.Platform.Configuration;
using Masterdom.Platform.Events;
using Masterdom.Platform.Metadata;
using Masterdom.Platform.Rules;
using Masterdom.Platform.Workflow;
using Masterdom.Platform.Modules;
using Masterdom.Platform.Services;

namespace Masterdom.Platform.Core;

/// <summary>
/// Provides access to platform services available to modules.
/// </summary>
public interface IPlatformContext
{
    /// <summary>
    /// Gets the read-only catalog of loaded modules.
    /// </summary>
    IModuleCatalog Modules { get; }

    /// <summary>
    /// Gets the runtime service registry.
    /// </summary>
    IPlatformServiceRegistry Services { get; }

    /// <summary>
    /// Gets the diagnostics sink.
    /// </summary>
    IDiagnostics Diagnostics { get; }

    /// <summary>
    /// Gets the versioned configuration resolver.
    /// </summary>
    IConfigurationResolver Configuration { get; }

    /// <summary>
    /// Gets the versioned metadata resolver.
    /// </summary>
    IMetadataResolver Metadata { get; }

    /// <summary>
    /// Gets the deterministic rules resolver.
    /// </summary>
    IRuleResolver Rules { get; }

    /// <summary>
    /// Gets the deterministic workflow resolver.
    /// </summary>
    IWorkflowResolver Workflows { get; }

    /// <summary>
    /// Gets the platform event publisher.
    /// </summary>
    IEventPublisher Events { get; }

    /// <summary>
    /// Gets the domain-event publisher adapter.
    /// </summary>
    IDomainEventPublisher DomainEvents { get; }
}
