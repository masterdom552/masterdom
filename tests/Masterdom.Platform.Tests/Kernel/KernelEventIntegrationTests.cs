using Masterdom.Platform.Core;
using Masterdom.Platform.Events;
using Masterdom.Platform.Modules;
using Masterdom.TestKit.Platform;
using PlatformKernel = Masterdom.Platform.Core.Kernel;

namespace Masterdom.Platform.Tests.Kernel;

public sealed class KernelEventIntegrationTests
{
    [Fact]
    public void Constructor_ShouldExposeEventInfrastructureThroughContext()
    {
        var kernel = new PlatformKernel();

        Assert.NotNull(kernel.Context.Events);
        Assert.NotNull(kernel.Context.DomainEvents);
    }

    [Fact]
    public void Start_ShouldCaptureLifecycleEventsInStore()
    {
        var kernel = new PlatformKernel();
        var module = new TestModule("events");

        kernel.LoadCatalog(new PlatformModuleCatalog(new[]
        {
            new ModuleCatalogEntry
            {
                ModuleId = module.Metadata.Id,
                Module = module,
                Version = module.Metadata.Version,
                StartupOrder = 0
            }
        }));

        kernel.Start();

        var store = kernel.Context.Services.GetRequired<IEventStore>();
        var stored = store.Read(new EventReadRequest { ModuleId = "events" });

        Assert.NotEmpty(stored);
        Assert.Contains(stored, e => e.EventType.Value.Contains("initialized", System.StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void Start_WhenRequiredEventHasNoHandler_ShouldFault()
    {
        var kernel = new PlatformKernel();
        var module = new TestModule("events");

        kernel.LoadCatalog(new PlatformModuleCatalog(new[]
        {
            new ModuleCatalogEntry
            {
                ModuleId = module.Metadata.Id,
                Module = module,
                Version = module.Metadata.Version,
                StartupOrder = 0
            }
        }));

        var registry = kernel.Context.Services.GetRequired<IEventRegistry>();
        registry.RegisterEvent(new EventDescriptor
        {
            EventType = new EventType("platform.events.required-handler"),
            Category = EventCategory.Platform,
            Version = new EventVersion(1),
            RequiresHandler = true
        });

        Assert.Throws<EventValidationException>(kernel.Start);
        Assert.Equal(KernelState.Faulted, kernel.State);
    }
}
