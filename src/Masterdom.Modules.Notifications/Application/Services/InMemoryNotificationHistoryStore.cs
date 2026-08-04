using Masterdom.Modules.Notifications.Application.Models;

namespace Masterdom.Modules.Notifications.Application.Services;

public sealed class InMemoryNotificationHistoryStore : INotificationHistoryStore
{
    private readonly List<NotificationHistoryEntry> _entries = [];

    public void Save(NotificationHistoryEntry entry)
    {
        ArgumentNullException.ThrowIfNull(entry);
        _entries.Add(entry);
    }

    public IReadOnlyCollection<NotificationHistoryEntry> GetByRecipient(Guid recipientId, int page, int pageSize)
    {
        var normalizedPage = page <= 0 ? 1 : page;
        var normalizedPageSize = pageSize <= 0 ? 20 : pageSize;

        return _entries
            .Where(x => x.RecipientId == recipientId)
            .OrderByDescending(x => x.LastAttemptAtUtc)
            .Skip((normalizedPage - 1) * normalizedPageSize)
            .Take(normalizedPageSize)
            .ToList();
    }
}
