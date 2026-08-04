using System;
using System.Collections.Generic;
using System.Linq;
using Masterdom.Platform.Configuration;
using Masterdom.Platform.Core;
using Masterdom.Platform.Diagnostics;
using Masterdom.Platform.Metadata;
using Masterdom.Platform.Modules;
using Masterdom.Platform.Rules;
using Masterdom.Platform.Workflow;
using Masterdom.Platform.Services;
using Masterdom.TestKit.Platform;
using PlatformKernel = Masterdom.Platform.Core.Kernel;

namespace Masterdom.Platform.Tests.Kernel;

public sealed class KernelTests
{
    [Fact]
    public void Constructor_ShouldInitializeCreatedState()
    {
        var kernel = new PlatformKernel();

        Assert.Equal(KernelState.Created, kernel.State);
        Assert.Equal(KernelHealthStatus.Degraded, kernel.CheckHealth().Status);
    }

    [Fact]
    public void Start_WithCatalogModules_ShouldTransitionToRunning()
    {
        var kernel = new PlatformKernel();
        var module = new TestModule("people");

        kernel.LoadCatalog(CreateCatalog(CreateEntry(module, startupOrder: 0)));
        kernel.Start();

        Assert.Equal(KernelState.Running, kernel.State);
        Assert.Equal(KernelHealthStatus.Healthy, kernel.CheckHealth().Status);
    }

    [Fact]
    public void Start_WithCatalog_ShouldHonorStartupOrderAndDependencies()
    {
        var order = new List<string>();

        var core = new OrderedModule("core", order);
        var audit = new OrderedModule("audit", order);
        var billing = new OrderedModule("billing", order);
        var notifications = new OrderedModule("notifications", order);

        var kernel = new PlatformKernel();

        kernel.LoadCatalog(CreateCatalog(
            CreateEntry(core, startupOrder: 0),
            CreateEntry(audit, startupOrder: 10),
            CreateEntry(
                billing,
                startupOrder: 20,
                dependencies: new[]
                {
                    new ModuleCatalogDependency
                    {
                        ModuleId = "core",
                        RequiredVersion = "1.0.0"
                    }
                }),
            CreateEntry(
                notifications,
                startupOrder: 30,
                dependencies: new[]
                {
                    new ModuleCatalogDependency
                    {
                        ModuleId = "billing",
                        RequiredVersion = "1.0.0"
                    }
                })));

        kernel.Start();

        Assert.Equal(new[] { "core", "audit", "billing", "notifications" }, order);
    }

    [Fact]
    public void Stop_AfterStart_ShouldTransitionToStopped()
    {
        var kernel = new PlatformKernel();
        var module = new TestModule("people");

        kernel.LoadCatalog(CreateCatalog(CreateEntry(module, startupOrder: 0)));
        kernel.Start();
        kernel.Stop();

        Assert.Equal(KernelState.Stopped, kernel.State);
        Assert.Equal(KernelHealthStatus.Degraded, kernel.CheckHealth().Status);
    }

    [Fact]
    public void Start_WhenCatalogModuleThrows_ShouldFaultAndRollbackInitializedModules()
    {
        var kernel = new PlatformKernel();
        var recording = new RecordingModule("recording");
        var throwing = new ThrowingModule("throwing");

        kernel.LoadCatalog(CreateCatalog(
            CreateEntry(recording, startupOrder: 0),
            CreateEntry(
                throwing,
                startupOrder: 10,
                dependencies: new[]
                {
                    new ModuleCatalogDependency
                    {
                        ModuleId = "recording",
                        RequiredVersion = "1.0.0"
                    }
                })));

        Assert.Throws<InvalidOperationException>(new Action(kernel.Start));

        Assert.Equal(KernelState.Faulted, kernel.State);
        Assert.True(recording.Initialized);
        Assert.True(recording.ShutdownCalled);
        Assert.Equal(KernelHealthStatus.Unhealthy, kernel.CheckHealth().Status);
    }

    [Fact]
    public void LoadCatalog_WhenDuplicateIdentifierExists_ShouldThrow()
    {
        var kernel = new PlatformKernel();
        var first = new TestModule("dup");
        var second = new TestModule("dup");

        var exception = Assert.Throws<ModuleCatalogValidationException>(() =>
            kernel.LoadCatalog(CreateCatalog(
                CreateEntry(first, startupOrder: 0),
                CreateEntry(second, startupOrder: 1))));

        Assert.Contains("Duplicate module identifiers", exception.Message);
    }

    [Fact]
    public void LoadCatalog_WhenDependencyMissing_ShouldThrow()
    {
        var kernel = new PlatformKernel();
        var module = new TestModule("people");

        var exception = Assert.Throws<ModuleCatalogValidationException>(() =>
            kernel.LoadCatalog(CreateCatalog(
                CreateEntry(
                    module,
                    startupOrder: 0,
                    dependencies: new[]
                    {
                        new ModuleCatalogDependency
                        {
                            ModuleId = "missing",
                            RequiredVersion = "1.0.0"
                        }
                    }))));

        Assert.Contains("depends on missing module", exception.Message);
    }

    [Fact]
    public void LoadCatalog_WhenCircularDependenciesExist_ShouldThrow()
    {
        var kernel = new PlatformKernel();
        var first = new TestModule("first");
        var second = new TestModule("second");

        var exception = Assert.Throws<ModuleCatalogValidationException>(() =>
            kernel.LoadCatalog(CreateCatalog(
                CreateEntry(
                    first,
                    startupOrder: 0,
                    dependencies: new[]
                    {
                        new ModuleCatalogDependency
                        {
                            ModuleId = "second",
                            RequiredVersion = "1.0.0"
                        }
                    }),
                CreateEntry(
                    second,
                    startupOrder: 0,
                    dependencies: new[]
                    {
                        new ModuleCatalogDependency
                        {
                            ModuleId = "first",
                            RequiredVersion = "1.0.0"
                        }
                    }))));

        Assert.Contains("Circular dependencies", exception.Message);
    }

    [Fact]
    public void LoadCatalog_WhenDependencyVersionConflictExists_ShouldThrow()
    {
        var kernel = new PlatformKernel();
        var baseModule = new TestModule("base");
        var dependent = new TestModule("dependent");

        var exception = Assert.Throws<ModuleCatalogValidationException>(() =>
            kernel.LoadCatalog(CreateCatalog(
                CreateEntry(baseModule, startupOrder: 0),
                CreateEntry(
                    dependent,
                    startupOrder: 10,
                    dependencies: new[]
                    {
                        new ModuleCatalogDependency
                        {
                            ModuleId = "base",
                            RequiredVersion = "2.0.0"
                        }
                    }))));

        Assert.Contains("requires 'base' version '2.0.0'", exception.Message);
    }

    [Fact]
    public void Start_WhenRequiredServiceMissing_ShouldFault()
    {
        var kernel = new PlatformKernel();
        var module = new TestModule("people");

        kernel.LoadCatalog(CreateCatalog(
            CreateEntry(
                module,
                startupOrder: 0,
                requiredServices: new[] { typeof(TestRuntimeService) })));

        Assert.Throws<ModuleCatalogValidationException>(new Action(kernel.Start));

        Assert.Equal(KernelState.Faulted, kernel.State);
    }

    [Fact]
    public void Start_ConfigurableModule_ShouldRegisterRequiredService()
    {
        var kernel = new PlatformKernel();
        var module = new ConfigurableModule();

        kernel.LoadCatalog(CreateCatalog(
            CreateEntry(
                module,
                startupOrder: 0,
                requiredServices: new[] { typeof(TestRuntimeService) })));

        kernel.Start();

        var service = kernel.Context.Services.GetRequired<TestRuntimeService>();

        Assert.NotNull(service);
    }

    [Fact]
    public void Start_ShouldWriteDiagnosticsEntriesWithModuleIds()
    {
        var diagnostics = new RecordingDiagnostics();
        var kernel = new PlatformKernel(diagnostics: diagnostics);
        var module = new TestModule("people");

        kernel.LoadCatalog(CreateCatalog(CreateEntry(module, startupOrder: 0)));
        kernel.Start();

        Assert.Contains(
            diagnostics.Entries,
            entry => entry.Message == "Kernel startup completed.");

        Assert.Contains(
            diagnostics.Entries,
            entry => entry.Source == "people" &&
                     entry.Message == "Module initialized.");
    }

    [Fact]
    public void LoadCatalog_ShouldExposeModuleConfigurationThroughResolver()
    {
        var kernel = new PlatformKernel();
        var module = new TestModule("people");

        kernel.LoadCatalog(CreateCatalog(CreateEntry(
            module,
            startupOrder: 0,
            configuration: new Dictionary<string, string>
            {
                ["time-zone"] = "UTC"
            })));

        var resolver = kernel.Context.Configuration;

        var result = resolver.Resolve(
            new ConfigurationKey("people.time-zone"),
            new ConfigurationResolutionRequest
            {
                ModuleId = "people",
                AsOfUtc = DateTime.SpecifyKind(new DateTime(2026, 1, 1), DateTimeKind.Utc)
            });

        Assert.Equal("UTC", result.Record.Value.Value);
        Assert.False(result.IsDefault);
    }

    [Fact]
    public void LoadCatalog_ShouldExposeModuleMetadataThroughResolver()
    {
        var kernel = new PlatformKernel();
        var module = new TestModule("people");

        kernel.LoadCatalog(CreateCatalog(CreateEntry(module, startupOrder: 0)));

        var resolved = kernel.Context.Metadata.Resolve(
            new MetadataKey("module.people"),
            MetadataScope.Module("people"),
            DateTime.SpecifyKind(new DateTime(2026, 1, 1), DateTimeKind.Utc));

        Assert.Equal(MetadataCategory.Module, resolved.Category);
        Assert.Equal("people", resolved.Scope.Identifier);
        Assert.Equal("people", resolved.Name);
    }

    [Fact]
    public void LoadCatalog_ShouldExposeSeededRuleSetThroughResolver()
    {
        var kernel = new PlatformKernel();
        var module = new TestModule("people");
        var asOfUtc = DateTime.SpecifyKind(new DateTime(2026, 1, 1), DateTimeKind.Utc);

        kernel.LoadCatalog(CreateCatalog(CreateEntry(module, startupOrder: 0)));

        var output = kernel.Context.Rules.Evaluate(
            new RuleSetKey("rules.people.default"),
            RuleScope.Create(RuleScopeKind.Module, "people"),
            new RuleContext
            {
                ModuleId = "people",
                AsOfUtc = asOfUtc
            },
            new RuleInput(Array.Empty<RuleInputItem>()));

        Assert.Equal("rules.people.default", output.RuleSetKey.Value);
        Assert.Empty(output.Results);
        Assert.True(output.Passed);
    }

    [Fact]
    public void LoadCatalog_ShouldExposeSeededWorkflowThroughResolver()
    {
        var kernel = new PlatformKernel();
        var module = new TestModule("people");
        var asOfUtc = DateTime.SpecifyKind(new DateTime(2026, 1, 1), DateTimeKind.Utc);

        kernel.LoadCatalog(CreateCatalog(CreateEntry(module, startupOrder: 0)));

        var output = kernel.Context.Workflows.Execute(
            new WorkflowKey("workflow.people.default"),
            WorkflowScope.Create(WorkflowScopeKind.Module, "people"),
            new WorkflowContext
            {
                ModuleId = "people",
                AsOfUtc = asOfUtc
            });

        Assert.Equal(WorkflowExecutionStatus.Completed, output.State.Status);
        Assert.True(output.IsTerminal);
    }

    private static PlatformModuleCatalog CreateCatalog(
        params ModuleCatalogEntry[] entries)
    {
        return new PlatformModuleCatalog(entries);
    }

    private static ModuleCatalogEntry CreateEntry(
        IModule module,
        int startupOrder,
        IReadOnlyList<ModuleCatalogDependency>? dependencies = null,
        IReadOnlyList<Type>? requiredServices = null,
        IReadOnlyList<Type>? optionalServices = null,
        IReadOnlyDictionary<string, string>? configuration = null)
    {
        return new ModuleCatalogEntry
        {
            ModuleId = module.Metadata.Id,
            Module = module,
            Version = module.Metadata.Version,
            StartupOrder = startupOrder,
            Dependencies = dependencies ?? Array.Empty<ModuleCatalogDependency>(),
            RequiredServices = requiredServices ?? Array.Empty<Type>(),
            OptionalServices = optionalServices ?? Array.Empty<Type>(),
            HealthChecks = new[] { $"health.{module.Metadata.Id}" },
            Capabilities = new[] { $"capability.{module.Metadata.Id}" },
            Configuration = configuration ?? new Dictionary<string, string>()
        };
    }

    private sealed class RecordingDiagnostics : IDiagnostics
    {
        public List<DiagnosticEntry> Entries { get; } = new();

        public void Write(DiagnosticEntry entry)
        {
            Entries.Add(entry);
        }
    }

    private sealed class OrderedModule : ModuleBase
    {
        private readonly List<string> _order;

        public OrderedModule(string id, List<string> order)
            : base(new ModuleDescriptor
            {
                Id = id,
                Name = id,
                DisplayName = id,
                Version = "1.0.0",
                Description = "Ordered module"
            })
        {
            _order = order;
        }

        public override void Initialize(IPlatformContext context)
        {
            base.Initialize(context);

            _order.Add(Metadata.Id);
        }
    }

    private sealed class ConfigurableModule : ModuleBase, IConfigurableModule
    {
        public ConfigurableModule()
            : base(new ModuleDescriptor
            {
                Id = "configurable",
                Name = "configurable",
                DisplayName = "Configurable",
                Version = "1.0.0",
                Description = "Configurable module"
            })
        {
        }

        public void ConfigureServices(IPlatformServiceRegistry services)
        {
            services.AddSingleton(new TestRuntimeService());
        }
    }

    private sealed class TestRuntimeService
    {
    }
}
