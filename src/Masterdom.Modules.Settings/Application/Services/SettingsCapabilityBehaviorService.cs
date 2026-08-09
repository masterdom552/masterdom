namespace Masterdom.Modules.Settings.Application.Services;

public sealed class SettingsCapabilityBehaviorService
{
    public SettingsCapabilityBehaviorResult Execute()
    {
        return new SettingsCapabilityBehaviorResult(
            Capability: "Settings",
            ExecutionPath: "Runtime",
            IsSupported: true);
    }
}

public sealed record SettingsCapabilityBehaviorResult(
    string Capability,
    string ExecutionPath,
    bool IsSupported);
