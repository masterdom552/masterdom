namespace Masterdom.Platform.Diagnostics;

/// <summary>
/// Writes diagnostic events.
/// </summary>
public interface IDiagnostics
{
    void Write(DiagnosticEntry entry);
}