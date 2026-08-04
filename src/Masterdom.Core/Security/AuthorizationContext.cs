namespace Masterdom.Core.Security;

/// <summary>
/// Describes the protected operation and its resolved business scope.
/// </summary>
public sealed record AuthorizationContext(
    string Operation,
    Guid? PropertyId = null,
    Guid? PersonId = null);
