namespace Masterdom.Core.Errors;

/// <summary>
/// Represents a domain or application error.
/// </summary>
public sealed record Error(
    string Code,
    string Message,
    ErrorType Type = ErrorType.Failure)
{
    /// <summary>
    /// Represents the absence of an error.
    /// </summary>
    public static readonly Error None = new(
        string.Empty,
        string.Empty,
        ErrorType.None);
}
