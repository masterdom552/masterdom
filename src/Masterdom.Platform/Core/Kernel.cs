using System;
using System.Collections.Generic;
using System.Linq;
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
/// Represents the Masterdom platform kernel.
/// </summary>
public sealed class Kernel
{
    private readonly IModuleRegistry _moduleRegistry;
    private readonly IModuleLoader _moduleLoader;
    private readonly IPlatformServiceRegistry _serviceRegistry;
    private readonly InMemoryConfigurationRepository _configurationRepository;
    private readonly InMemoryMetadataRepository _metadataRepository;
    private readonly InMemoryRuleRepository _ruleRepository;
    private readonly InMemoryWorkflowRepository _workflowRepository;
    private readonly IConfigurationDefaults _configurationDefaults;
    private readonly ConfigurationRegistry _configurationRegistry;
    private readonly ConfigurationResolver _configurationResolver;
    private readonly MetadataRegistry _metadataRegistry;
    private readonly MetadataResolver _metadataResolver;
    private readonly RuleRegistry _ruleRegistry;
    private readonly RuleResolver _ruleResolver;
    private readonly InMemoryEventRepository _eventRepository;
    private readonly EventStore _eventStore;
    private readonly EventRegistry _eventRegistry;
    private readonly EventHandlerResolver _eventHandlerResolver;
    private readonly EventDispatcher _eventDispatcher;
    private readonly EventPublisher _eventPublisher;
    private readonly DomainEventAdapter _domainEventAdapter;
    private readonly DomainEventPublisher _domainEventPublisher;
    private readonly IWorkflowStateStore _workflowStateStore;
    private readonly WorkflowRegistry _workflowRegistry;
    private readonly WorkflowResolver _workflowResolver;
    private readonly IDiagnostics _diagnostics;
    private readonly KernelHealthCheck _healthCheck;
    private readonly PlatformContext _platformContext;
    private readonly List<ModuleCatalogEntry> _catalogEntries = new();
    private readonly List<IModule> _initializedModules = new();
    private ModuleStartupGraph? _startupGraph;

    /// <summary>
    /// Initializes a new instance of the <see cref="Kernel"/> class.
    /// </summary>
    public Kernel(
        IModuleRegistry? moduleRegistry = null,
        IModuleLoader? moduleLoader = null,
        IPlatformServiceRegistry? serviceRegistry = null,
        IConfigurationRepository? configurationRepository = null,
        IMetadataRepository? metadataRepository = null,
        IRuleRepository? ruleRepository = null,
        IWorkflowRepository? workflowRepository = null,
        IWorkflowStateStore? workflowStateStore = null,
        IConfigurationDefaults? configurationDefaults = null,
        IDiagnostics? diagnostics = null,
        KernelHealthCheck? healthCheck = null)
    {
        _moduleRegistry = moduleRegistry ?? new ModuleRegistry();
        _moduleLoader = moduleLoader ?? new ModuleLoader();
        _serviceRegistry = serviceRegistry ?? new PlatformServiceRegistry();
        _configurationRepository = configurationRepository as InMemoryConfigurationRepository
            ?? new InMemoryConfigurationRepository(configurationRepository?.GetAll());
        _metadataRepository = metadataRepository as InMemoryMetadataRepository
            ?? new InMemoryMetadataRepository(metadataRepository?.GetAll());
        _ruleRepository = ruleRepository as InMemoryRuleRepository
            ?? new InMemoryRuleRepository(
                ruleRepository?.GetAllRuleSets(),
                ruleRepository?.GetAllRules());
        _workflowRepository = workflowRepository as InMemoryWorkflowRepository
            ?? new InMemoryWorkflowRepository(
                workflowRepository?.GetAllWorkflows(),
                workflowRepository?.GetAllVersions(),
                workflowRepository?.GetAllSteps(),
                workflowRepository?.GetAllTransitions());
        _configurationDefaults = configurationDefaults ?? new DefaultConfigurationDefaults();
        _configurationRegistry = new ConfigurationRegistry(_configurationRepository);
        _configurationResolver = new ConfigurationResolver(
            _configurationRepository,
            _configurationDefaults);
        _metadataRegistry = new MetadataRegistry(_metadataRepository);
        _metadataResolver = new MetadataResolver(_metadataRepository);
        _ruleRegistry = new RuleRegistry(_ruleRepository);
        _ruleResolver = new RuleResolver(
            _ruleRepository,
            _configurationResolver,
            _metadataResolver);
        _eventRepository = new InMemoryEventRepository();
        _eventStore = new EventStore(_eventRepository);
        _eventRegistry = new EventRegistry();
        _eventHandlerResolver = new EventHandlerResolver(_eventRegistry);
        _eventDispatcher = new EventDispatcher(_eventHandlerResolver);
        _eventPublisher = new EventPublisher(_eventStore, _eventDispatcher);
        _domainEventAdapter = new DomainEventAdapter();
        _domainEventPublisher = new DomainEventPublisher(_domainEventAdapter, _eventPublisher);
        _workflowStateStore = workflowStateStore ?? new InMemoryWorkflowStateStore();
        _workflowRegistry = new WorkflowRegistry(_workflowRepository);
        _workflowResolver = new WorkflowResolver(
            _workflowRepository,
            _configurationResolver,
            _metadataResolver,
            _ruleResolver,
            _workflowStateStore);
        _diagnostics = diagnostics ?? new NullDiagnostics();
        _healthCheck = healthCheck ?? new KernelHealthCheck();

        _platformContext = new PlatformContext(
            _moduleRegistry,
            _serviceRegistry,
            _diagnostics,
            _configurationResolver,
            _metadataResolver,
            _ruleResolver,
            _workflowResolver,
            _eventPublisher,
            _domainEventPublisher);

        RegisterCoreServices();

        State = KernelState.Created;

        WriteDiagnostic(
            DiagnosticSeverity.Information,
            nameof(Kernel),
            "Kernel created.");
    }

    /// <summary>
    /// Gets the current kernel state.
    /// </summary>
    public KernelState State { get; private set; }

    /// <summary>
    /// Gets the platform context.
    /// </summary>
    public IPlatformContext Context => _platformContext;

    /// <summary>
    /// Gets the startup graph that was generated from the active module catalog.
    /// </summary>
    public ModuleStartupGraph? StartupGraph => _startupGraph;

    /// <summary>
    /// Gets a health summary for the current kernel state.
    /// </summary>
    public KernelHealthCheckResult CheckHealth()
    {
        return _healthCheck.Evaluate(State);
    }

    /// <summary>
    /// Loads a module into the platform.
    /// </summary>
    /// <param name="module">The module to load.</param>
    public void LoadModule(IModule module)
    {
        EnsureState(KernelState.Created);

        _moduleLoader.Load(module, _moduleRegistry);

        WriteDiagnostic(
            DiagnosticSeverity.Information,
            nameof(Kernel),
            $"Module '{module.Metadata.Id}' loaded.");
    }

    /// <summary>
    /// Loads multiple modules into the platform.
    /// </summary>
    public void LoadModules(IEnumerable<IModule> modules)
    {
        ArgumentNullException.ThrowIfNull(modules);

        foreach (var module in modules)
        {
            LoadModule(module);
        }
    }

    /// <summary>
    /// Loads modules from the specified authoritative catalog.
    /// </summary>
    /// <param name="catalog">The catalog to load.</param>
    public void LoadCatalog(IPlatformModuleCatalog catalog)
    {
        EnsureState(KernelState.Created);

        ArgumentNullException.ThrowIfNull(catalog);

        if (_moduleRegistry.Count > 0)
        {
            throw new InvalidOperationException(
                "Catalog must be loaded before individual modules are registered.");
        }

        var graph = ModuleStartupGraphBuilder.Build(catalog);

        _startupGraph = graph;
        _catalogEntries.Clear();
        _catalogEntries.AddRange(graph.OrderedModules);

        foreach (var entry in _catalogEntries)
        {
            _moduleLoader.Load(entry.Module, _moduleRegistry);

            WriteDiagnostic(
                DiagnosticSeverity.Information,
                entry.ModuleId,
                "Module loaded from catalog.");
        }

        if (!_serviceRegistry.Contains(typeof(IPlatformModuleCatalog)))
        {
            _serviceRegistry.AddSingleton<IPlatformModuleCatalog>(catalog);
        }

        RegisterCatalogConfiguration();
        RegisterCatalogMetadata();
        RegisterCatalogRules();
        RegisterCatalogWorkflows();
        RegisterCatalogEvents();

        WriteDiagnostic(
            DiagnosticSeverity.Information,
            nameof(Kernel),
            $"Catalog loaded with {_catalogEntries.Count} module(s).");
    }

    /// <summary>
    /// Starts the platform kernel.
    /// </summary>
    public void Start()
    {
        EnsureState(KernelState.Created);

        WriteDiagnostic(
            DiagnosticSeverity.Information,
            nameof(Kernel),
            "Kernel startup started.");

        State = KernelState.Starting;

        _initializedModules.Clear();

        try
        {
            var startupModules = GetStartupModules();

            ConfigureModuleServices(startupModules);
            RunStartupValidation(startupModules);

            foreach (var module in startupModules)
            {
                module.Initialize(_platformContext);
                _initializedModules.Add(module);

                PublishLifecycleEvent(
                    PlatformEventTypes.ModuleInitialized(module.Metadata.Id),
                    module.Metadata.Id,
                    "Module initialized.");

                WriteDiagnostic(
                    DiagnosticSeverity.Information,
                    module.Metadata.Id,
                    "Module initialized.");
            }

            State = KernelState.Running;

            PublishLifecycleEvent(
                PlatformEventTypes.KernelStarted,
                nameof(Kernel),
                "Kernel started.");

            WriteDiagnostic(
                DiagnosticSeverity.Information,
                nameof(Kernel),
                "Kernel startup completed.");
        }
        catch (Exception ex)
        {
            WriteDiagnostic(
                DiagnosticSeverity.Error,
                nameof(Kernel),
                "Kernel startup failed.",
                ex);

            for (int i = _initializedModules.Count - 1; i >= 0; i--)
            {
                try
                {
                    _initializedModules[i].Shutdown(_platformContext);

                    WriteDiagnostic(
                        DiagnosticSeverity.Warning,
                        _initializedModules[i].Metadata.Id,
                        "Module rolled back.");
                }
                catch
                {
                    // Ignore rollback failures.
                }
            }

            _initializedModules.Clear();

            State = KernelState.Faulted;

            throw;
        }
    }

    /// <summary>
    /// Stops the platform kernel.
    /// </summary>
    public void Stop()
    {
        EnsureState(KernelState.Running);

        WriteDiagnostic(
            DiagnosticSeverity.Information,
            nameof(Kernel),
            "Kernel shutdown started.");

        State = KernelState.Stopping;

        for (int i = _initializedModules.Count - 1; i >= 0; i--)
        {
            try
            {
                _initializedModules[i].Shutdown(_platformContext);

                PublishLifecycleEvent(
                    PlatformEventTypes.ModuleShutdown(_initializedModules[i].Metadata.Id),
                    _initializedModules[i].Metadata.Id,
                    "Module shutdown completed.");

                WriteDiagnostic(
                    DiagnosticSeverity.Information,
                    _initializedModules[i].Metadata.Id,
                    "Module shutdown completed.");
            }
            catch
            {
                // Continue shutting down remaining modules.
            }
        }

        _initializedModules.Clear();

        State = KernelState.Stopped;

        PublishLifecycleEvent(
            PlatformEventTypes.KernelStopped,
            nameof(Kernel),
            "Kernel stopped.");

        WriteDiagnostic(
            DiagnosticSeverity.Information,
            nameof(Kernel),
            "Kernel shutdown completed.");
    }

    private void ConfigureModuleServices(IEnumerable<IModule> startupModules)
    {
        foreach (var module in startupModules)
        {
            if (module is not IConfigurableModule configurableModule)
            {
                continue;
            }

            configurableModule.ConfigureServices(_serviceRegistry);

            WriteDiagnostic(
                DiagnosticSeverity.Debug,
                module.Metadata.Id,
                "Module services configured.");
        }
    }

    private void ValidateRequiredServices(IEnumerable<IModule> startupModules)
    {
        if (_catalogEntries.Count == 0)
        {
            return;
        }

        var byId = _catalogEntries.ToDictionary(
            entry => entry.ModuleId,
            StringComparer.OrdinalIgnoreCase);

        foreach (var module in startupModules)
        {
            if (!byId.TryGetValue(module.Metadata.Id, out var entry))
            {
                continue;
            }

            foreach (var requiredService in entry.RequiredServices)
            {
                if (_serviceRegistry.Contains(requiredService))
                {
                    continue;
                }

                throw new ModuleCatalogValidationException(
                    $"Required service '{requiredService.FullName}' was not registered for module '{entry.ModuleId}'.");
            }

            foreach (var optionalService in entry.OptionalServices)
            {
                if (_serviceRegistry.Contains(optionalService))
                {
                    continue;
                }

                WriteDiagnostic(
                    DiagnosticSeverity.Debug,
                    entry.ModuleId,
                    $"Optional service '{optionalService.FullName}' is not registered.");
            }
        }
    }

    private void RunStartupValidation(IReadOnlyList<IModule> startupModules)
    {
        new ValidationPipeline()
            .Add("required-services", () => ValidateRequiredServices(startupModules))
            .Add("event-registry", () => _eventRegistry.Validate())
            .Execute();
    }

    private IReadOnlyList<IModule> GetStartupModules()
    {
        if (_catalogEntries.Count > 0)
        {
            return _catalogEntries
                .Select(entry => entry.Module)
                .ToList();
        }

        return _moduleRegistry.ToList();
    }

    private void RegisterCoreServices()
    {
        _serviceRegistry.AddSingleton<IModuleCatalog>(_moduleRegistry);
        _serviceRegistry.AddSingleton<IModuleRegistry>(_moduleRegistry);
        _serviceRegistry.AddSingleton<IPlatformServiceRegistry>(_serviceRegistry);
        _serviceRegistry.AddSingleton<IDiagnostics>(_diagnostics);
        _serviceRegistry.AddSingleton<IConfigurationRepository>(_configurationRepository);
        _serviceRegistry.AddSingleton<IConfigurationRegistry>(_configurationRegistry);
        _serviceRegistry.AddSingleton<IConfigurationDefaults>(_configurationDefaults);
        _serviceRegistry.AddSingleton<IConfigurationResolver>(_configurationResolver);
        _serviceRegistry.AddSingleton<IMetadataRepository>(_metadataRepository);
        _serviceRegistry.AddSingleton<IMetadataRegistry>(_metadataRegistry);
        _serviceRegistry.AddSingleton<IMetadataResolver>(_metadataResolver);
        _serviceRegistry.AddSingleton<IRuleRepository>(_ruleRepository);
        _serviceRegistry.AddSingleton<IRuleRegistry>(_ruleRegistry);
        _serviceRegistry.AddSingleton<IRuleResolver>(_ruleResolver);
        _serviceRegistry.AddSingleton<IEventRepository>(_eventRepository);
        _serviceRegistry.AddSingleton<IEventStore>(_eventStore);
        _serviceRegistry.AddSingleton<IEventRegistry>(_eventRegistry);
        _serviceRegistry.AddSingleton<IEventHandlerResolver>(_eventHandlerResolver);
        _serviceRegistry.AddSingleton<IEventDispatcher>(_eventDispatcher);
        _serviceRegistry.AddSingleton<IEventPublisher>(_eventPublisher);
        _serviceRegistry.AddSingleton<IDomainEventAdapter>(_domainEventAdapter);
        _serviceRegistry.AddSingleton<IDomainEventPublisher>(_domainEventPublisher);
        _serviceRegistry.AddSingleton<IWorkflowRepository>(_workflowRepository);
        _serviceRegistry.AddSingleton<IWorkflowStateStore>(_workflowStateStore);
        _serviceRegistry.AddSingleton<IWorkflowRegistry>(_workflowRegistry);
        _serviceRegistry.AddSingleton<IWorkflowResolver>(_workflowResolver);
        _serviceRegistry.AddSingleton<IPlatformContext>(_platformContext);
        _serviceRegistry.AddSingleton(_healthCheck);
    }

    private void RegisterCatalogConfiguration()
    {
        var records = ConfigurationCatalogBuilder.BuildFromCatalog(_catalogEntries);

        _configurationRegistry.ReplaceAll(records);
    }

    private void RegisterCatalogMetadata()
    {
        var fromCatalog = MetadataCatalogBuilder.BuildFromCatalog(_catalogEntries);

        var merged = _metadataRepository.GetAll()
            .Concat(fromCatalog)
            .ToList();

        _metadataRegistry.ReplaceAll(merged);
    }

    private void RegisterCatalogRules()
    {
        var seeded = RuleCatalogBuilder.BuildFromCatalog(_catalogEntries);

        var mergedRuleSets = _ruleRepository.GetAllRuleSets()
            .Concat(seeded.RuleSets)
            .ToList();

        var mergedRules = _ruleRepository.GetAllRules()
            .Concat(seeded.Rules)
            .ToList();

        _ruleRegistry.ReplaceAll(mergedRuleSets, mergedRules);
    }

    private void RegisterCatalogWorkflows()
    {
        var seeded = WorkflowCatalogBuilder.BuildFromCatalog(_catalogEntries);

        var mergedWorkflows = _workflowRepository.GetAllWorkflows()
            .Concat(seeded.Workflows)
            .ToList();

        var mergedVersions = _workflowRepository.GetAllVersions()
            .Concat(seeded.Versions)
            .ToList();

        var mergedSteps = _workflowRepository.GetAllSteps()
            .Concat(seeded.Steps)
            .ToList();

        var mergedTransitions = _workflowRepository.GetAllTransitions()
            .Concat(seeded.Transitions)
            .ToList();

        _workflowRegistry.ReplaceAll(
            mergedWorkflows,
            mergedVersions,
            mergedSteps,
            mergedTransitions);
    }

    private void RegisterCatalogEvents()
    {
        var descriptors = EventCatalogBuilder.BuildFromCatalog(_catalogEntries);

        _eventRegistry.RegisterEvents(descriptors);

        foreach (var entry in _catalogEntries)
        {
            PublishLifecycleEvent(
                PlatformEventTypes.ModuleLoaded(entry.ModuleId),
                entry.ModuleId,
                "Module loaded from catalog.");
        }
    }

    private void PublishLifecycleEvent(EventType eventType, string moduleId, string message)
    {
        var nowUtc = DateTime.UtcNow;

        var platformEvent = new PlatformEvent(
            new EventId(Guid.NewGuid()),
            new EventVersion(1),
            eventType,
            nowUtc,
            new EventPayload($"{{\"message\":\"{message}\"}}"),
            EventCategory.Lifecycle);

        var context = new EventContext
        {
            ModuleId = moduleId,
            OccurredAtUtc = nowUtc
        };

        _eventPublisher.Publish(platformEvent, context);
    }

    private void WriteDiagnostic(
        DiagnosticSeverity severity,
        string source,
        string message,
        Exception? exception = null)
    {
        _diagnostics.Write(new DiagnosticEntry
        {
            TimestampUtc = DateTime.UtcNow,
            Severity = severity,
            Source = source,
            Message = message,
            Exception = exception
        });
    }

    private void EnsureState(KernelState expectedState)
    {
        if (State != expectedState)
        {
            throw new InvalidOperationException(
                $"Kernel is in state '{State}' but expected '{expectedState}'.");
        }
    }
}
