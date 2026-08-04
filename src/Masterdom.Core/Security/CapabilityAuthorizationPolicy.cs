namespace Masterdom.Core.Security;

/// <summary>
/// Defines the minimal authorization requirements for a protected operation.
/// </summary>
public sealed record CapabilityAuthorizationPolicy(
    string Operation,
    string? RequiredPermission,
    bool IsPropertyScoped,
    bool AllowsPropertyOwner,
    bool AllowsTenantSelf);
