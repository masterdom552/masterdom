namespace Masterdom.Platform.BusinessContext;

/// <summary>
/// Represents validation failures for Business Context assembly.
/// </summary>
public sealed class BusinessContextValidationException : Exception
{
    public BusinessContextValidationException(string message)
        : base(message)
    {
    }
}
