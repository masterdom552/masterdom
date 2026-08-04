namespace Masterdom.Core.Security;

/// <summary>
/// Resolves authorization requirements for protected operations.
/// </summary>
public interface ICapabilityAuthorizationPolicyProvider
{
    CapabilityAuthorizationPolicy GetPolicy(string operation);
}
