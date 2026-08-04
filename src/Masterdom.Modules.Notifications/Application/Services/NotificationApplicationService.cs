using Masterdom.Modules.Notifications.Application.Models;
using Masterdom.Platform.Notifications;
using Masterdom.Platform.ReadModels;

namespace Masterdom.Modules.Notifications.Application.Services;

public sealed class NotificationApplicationService : INotificationApplicationService
{
    private readonly INotificationRegistry _notificationRegistry;
    private readonly INotificationHistoryStore _historyStore;
    private readonly INotificationAuthorizationService _authorizationService;
    private readonly INotificationGenerationEngine _generationEngine;
    private readonly INotificationDeliveryQueue _deliveryQueue;
    private readonly INotificationDeliveryProcessor _deliveryProcessor;

    public NotificationApplicationService(
        INotificationRegistry notificationRegistry,
        INotificationHistoryStore historyStore,
        INotificationAuthorizationService authorizationService,
        INotificationGenerationEngine generationEngine,
        INotificationDeliveryQueue deliveryQueue,
        INotificationDeliveryProcessor deliveryProcessor)
    {
        _notificationRegistry = notificationRegistry ?? throw new ArgumentNullException(nameof(notificationRegistry));
        _historyStore = historyStore ?? throw new ArgumentNullException(nameof(historyStore));
        _authorizationService = authorizationService ?? throw new ArgumentNullException(nameof(authorizationService));
        _generationEngine = generationEngine ?? throw new ArgumentNullException(nameof(generationEngine));
        _deliveryQueue = deliveryQueue ?? throw new ArgumentNullException(nameof(deliveryQueue));
        _deliveryProcessor = deliveryProcessor ?? throw new ArgumentNullException(nameof(deliveryProcessor));
    }

    public GeneratedNotification Generate(
        string eventCode,
        Guid recipientId,
        DateTime requestedAtUtc,
        IReadOnlyDictionary<string, string> parameters,
        DateTime? requestedDeliveryAtUtc)
    {
        var normalizedEvent = EventCodeCatalog.Normalize(eventCode);
        _authorizationService.EnsureAuthorized(normalizedEvent, recipientId);

        var registration = _notificationRegistry.Resolve(normalizedEvent);
        var envelope = _generationEngine.Generate(
            registration,
            recipientId,
            requestedAtUtc,
            parameters,
            requestedDeliveryAtUtc);

        _deliveryQueue.Enqueue(envelope);
        var queued = _deliveryQueue.Dequeue() ?? envelope;
        var delivered = _deliveryProcessor.Deliver(queued);

        var audit = $"event={normalizedEvent};channels={string.Join(',', delivered.Channels)};attempts={delivered.AttemptCount};";

        _historyStore.Save(new NotificationHistoryEntry(
            delivered.NotificationId,
            normalizedEvent,
            delivered.RecipientId,
            requestedAtUtc,
            DateTime.UtcNow,
            delivered.AttemptCount,
            delivered.IsDelivered,
            delivered.Status,
            audit));

        return new GeneratedNotification(
            delivered.NotificationId,
            normalizedEvent,
            delivered.RecipientId,
            ParseCategory(delivered.Category),
            ParsePriority(delivered.Priority),
            delivered.Subject,
            delivered.Body,
            ParseChannels(delivered.Channels),
            delivered.ScheduledAtUtc,
            delivered.AttemptCount,
            delivered.IsDelivered,
            delivered.Status);
    }

    public IReadOnlyCollection<NotificationHistoryEntry> History(Guid recipientId, int page, int pageSize)
    {
        return _historyStore.GetByRecipient(recipientId, page, pageSize);
    }

    private static IReadOnlyCollection<DeliveryChannel> ParseChannels(IReadOnlyCollection<string> channels)
    {
        return channels
            .Select(channel => Enum.TryParse<DeliveryChannel>(channel, true, out var parsed) ? parsed : (DeliveryChannel?)null)
            .Where(x => x != null)
            .Select(x => x!.Value)
            .ToList();
    }

    private static NotificationCategory ParseCategory(string category)
    {
        return Enum.TryParse<NotificationCategory>(category, true, out var parsed)
            ? parsed
            : NotificationCategory.Announcement;
    }

    private static NotificationPriority ParsePriority(string priority)
    {
        return Enum.TryParse<NotificationPriority>(priority, true, out var parsed)
            ? parsed
            : NotificationPriority.Normal;
    }
}
