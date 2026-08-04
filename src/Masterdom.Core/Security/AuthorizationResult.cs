namespace Masterdom.Core.Security;

/// <summary>
/// Represents the outcome of an authorization check.
/// </summary>
public sealed class AuthorizationResult
{
    private AuthorizationResult(bool isAllowed, string errorCode, string? errorMessage)
    {
        IsAllowed = isAllowed;
        ErrorCode = errorCode;
        ErrorMessage = errorMessage;
    }

    public bool IsAllowed { get; }

    public string ErrorCode { get; }

    public string? ErrorMessage { get; }

    public static AuthorizationResult Allowed() => new(true, string.Empty, null);

    public static AuthorizationResult Challenge(string? errorMessage = null) =>
        new(false, "unauthorized", errorMessage ?? "Authentication is required.");

    public static AuthorizationResult Forbid(string? errorMessage = null) =>
        new(false, "forbidden", errorMessage ?? "The current user is not authorized for this operation.");
}
