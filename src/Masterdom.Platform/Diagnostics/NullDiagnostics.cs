namespace Masterdom.Platform.Diagnostics;

/// <summary>
/// Diagnostic sink that discards all entries.
/// </summary>
public sealed class NullDiagnostics : IDiagnostics
{
    /// <inheritdoc />
    public void Write(DiagnosticEntry entry)
    {
    }
}
