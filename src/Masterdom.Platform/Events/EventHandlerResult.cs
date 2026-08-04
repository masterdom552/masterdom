using System.Collections.Generic;

namespace Masterdom.Platform.Events;

/// <summary>
/// Represents the result returned by one event handler.
/// </summary>
public sealed class EventHandlerResult
{
    public bool IsSuccessful { get; init; } = true;

    public string? Warning { get; init; }

    public IReadOnlyDictionary<string, string> Diagnostics { get; init; } =
        new Dictionary<string, string>();
}
