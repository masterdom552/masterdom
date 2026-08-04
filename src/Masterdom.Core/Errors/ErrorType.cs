namespace Masterdom.Core.Errors;

/// <summary>
/// Categorizes an error.
/// </summary>
public enum ErrorType
{
    None = 0,
    Failure = 1,
    Validation = 2,
    Conflict = 3,
    NotFound = 4,
    Unauthorized = 5,
    Forbidden = 6
}
