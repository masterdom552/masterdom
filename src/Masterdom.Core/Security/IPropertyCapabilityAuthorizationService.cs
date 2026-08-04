namespace Masterdom.Core.Security;

/// <summary>
/// Evaluates request authorization for the Property capability runtime.
/// </summary>
public interface IPropertyCapabilityAuthorizationService
{
    AuthorizationResult Authorize(AuthorizationContext context);
}
