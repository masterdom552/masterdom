using Masterdom.Platform.Core;
using Masterdom.Platform.Modules;

namespace Masterdom.TestKit.Platform;

/// <summary>
/// Simple module used by unit tests.
/// </summary>
public sealed class TestModule : ModuleBase
{
    public TestModule(string id)
        : base(new ModuleDescriptor
        {
            Id = id,
            Name = id,
            DisplayName = id,
            Version = "1.0.0",
            Description = "Test module"
        })
    {
    }
}
