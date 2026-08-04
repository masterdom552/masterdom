namespace Masterdom.Modules.Notifications.Application.Models;

public sealed class NotificationRequest
{
    public NotificationRequest(
        string EventCode,
        Guid RecipientId,
        DateTime RequestedAtUtc,
        IReadOnlyDictionary<string, string> Parameters)
    {
        EventCode = EventCodeCatalog.Normalize(EventCode);
        RecipientId = RecipientId;
        RequestedAtUtc = RequestedAtUtc;
        Parameters = Parameters;
    }

    public string EventCode { get; }

    public Guid RecipientId { get; }

    public DateTime RequestedAtUtc { get; }

    public IReadOnlyDictionary<string, string> Parameters { get; }
}
