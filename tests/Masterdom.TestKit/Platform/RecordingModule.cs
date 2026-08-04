using Masterdom.Platform.Core;
using Masterdom.Platform.Modules;

namespace Masterdom.TestKit.Platform;

/// <summary>
/// Records lifecycle events for verification.
/// </summary>
public sealed class RecordingModule : ModuleBase
{
    public bool Initialized { get; private set; }

    public bool ShutdownCalled { get; private set; }

    public RecordingModule(string id)
        : base(new ModuleDescriptor
        {
            Id = id,
            Name = id,
            DisplayName = id,
            Version = "1.0.0",
            Description = "Recording module"
        })
    {
    }

    public override void Initialize(IPlatformContext context)
    {
        Initialized = true;
    }

    public override void Shutdown(IPlatformContext context)
    {
        ShutdownCalled = true;
    }
}
