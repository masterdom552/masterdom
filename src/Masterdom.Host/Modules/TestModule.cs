using System;
using Masterdom.Platform.Core;
using Masterdom.Platform.Modules;

namespace Masterdom.Host.Modules;

/// <summary>
/// Temporary module used to verify the platform startup pipeline.
/// </summary>
public sealed class TestModule : PlatformModule
{
    public TestModule()
        : base(new ModuleDescriptor
        {
            Id = "test",
            Name = "Test",
            DisplayName = "Test Module",
            Version = PlatformVersion.Current.ToString(),
            Description = "Temporary module used for platform verification."
        })
    {
    }

    public override void Initialize(IPlatformContext context)
    {
        base.Initialize(context);

        Console.WriteLine("TestModule initialized.");
    }

    public override void Shutdown(IPlatformContext context)
    {
        Console.WriteLine("TestModule shutting down.");

        base.Shutdown(context);
    }
}