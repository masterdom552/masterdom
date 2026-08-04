using System;
using Masterdom.Platform.Core;
using Masterdom.Platform.Modules;

namespace Masterdom.TestKit.Platform;

/// <summary>
/// Module that throws during initialization.
/// </summary>
public sealed class ThrowingModule : ModuleBase
{
    public ThrowingModule(string id)
        : base(new ModuleDescriptor
        {
            Id = id,
            Name = id,
            DisplayName = id,
            Version = "1.0.0",
            Description = "Throwing test module"
        })
    {
    }

    public override void Initialize(IPlatformContext context)
    {
        throw new InvalidOperationException("Initialization failed.");
    }
}
