namespace Masterdom.Modules.Authentication.Application.Services;

public sealed class AuthenticationCapabilityBehaviorService
{
    public AuthenticationCapabilityBehaviorResult Execute()
    {
        return new AuthenticationCapabilityBehaviorResult(
            Capability: "Authentication",
            ExecutionPath: "Runtime",
            IsSupported: true);
    }
}

public sealed record AuthenticationCapabilityBehaviorResult(
    string Capability,
    string ExecutionPath,
    bool IsSupported);
