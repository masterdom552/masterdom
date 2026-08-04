using System;
using System.Collections.Generic;
using Masterdom.Host.Modules;
using Masterdom.Platform.Core;
using Masterdom.Platform.Diagnostics;
using Masterdom.Platform.Modules;

namespace Masterdom.Host;

/// <summary>
/// Bootstraps the Masterdom platform.
/// </summary>
public sealed class Bootstrapper
{
    private readonly Kernel _kernel;
    private readonly IPlatformModuleCatalog _moduleCatalog;

    public Bootstrapper()
    {
        _kernel = new Kernel(diagnostics: new ConsoleDiagnostics());

        var module = new TestModule();

        _moduleCatalog = new PlatformModuleCatalog(new[]
        {
            new ModuleCatalogEntry
            {
                ModuleId = module.Metadata.Id,
                Module = module,
                Version = module.Metadata.Version,
                StartupOrder = 0,
                Dependencies = Array.Empty<ModuleCatalogDependency>(),
                RequiredServices = Array.Empty<Type>(),
                OptionalServices = Array.Empty<Type>(),
                HealthChecks = new[] { "kernel.lifecycle" },
                Capabilities = new[] { "platform.bootstrap" },
                Configuration = new Dictionary<string, string>()
            }
        });

        _kernel.LoadCatalog(_moduleCatalog);
    }

    public KernelHealthCheckResult Health => _kernel.CheckHealth();

    public IPlatformModuleCatalog ModuleCatalog => _moduleCatalog;

    public void Start()
    {
        _kernel.Start();
    }

    public void Stop()
    {
        if (_kernel.State != KernelState.Running)
        {
            return;
        }

        _kernel.Stop();
    }
}
