using System;

namespace Masterdom.Platform.Diagnostics;

/// <summary>
/// Writes diagnostics to the console.
/// </summary>
public sealed class ConsoleDiagnostics : IDiagnostics
{
    public void Write(DiagnosticEntry entry)
    {
        ArgumentNullException.ThrowIfNull(entry);

        Console.WriteLine(
            $"[{entry.TimestampUtc:O}] " +
            $"[{entry.Severity}] " +
            $"[{entry.Source}] " +
            $"{entry.Message}");

        if (entry.Exception is not null)
        {
            Console.WriteLine(entry.Exception);
        }
    }
}