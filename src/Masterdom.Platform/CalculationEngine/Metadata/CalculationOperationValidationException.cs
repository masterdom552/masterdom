namespace Masterdom.Platform.CalculationEngine.Metadata;

/// <summary>
/// Raised when calculation operation metadata is invalid.
/// </summary>
public sealed class CalculationOperationValidationException : InvalidOperationException
{
    public CalculationOperationValidationException(string message)
        : base(message)
    {
    }
}
