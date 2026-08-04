using System;

namespace Masterdom.Platform.Events;

/// <summary>
/// Represents an immutable event payload.
/// </summary>
public sealed class EventPayload
{
    public EventPayload(string body, string contentType = "application/json")
    {
        if (string.IsNullOrWhiteSpace(body))
        {
            throw new EventValidationException("Event payload body is required.");
        }

        if (string.IsNullOrWhiteSpace(contentType))
        {
            throw new EventValidationException("Event payload content type is required.");
        }

        Body = body;
        ContentType = contentType.Trim();
    }

    public string Body { get; }

    public string ContentType { get; }
}
