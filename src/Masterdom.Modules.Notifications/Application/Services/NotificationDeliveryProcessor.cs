using Masterdom.Modules.Notifications.Application.Models;
using Masterdom.Platform.Notifications;

namespace Masterdom.Modules.Notifications.Application.Services;

public sealed class NotificationDeliveryProcessor : INotificationDeliveryProcessor
{
    private readonly IReadOnlyDictionary<string, IDeliveryProvider> _providers;

    public NotificationDeliveryProcessor(IEnumerable<IDeliveryProvider> providers)
    {
        _providers = providers?.ToDictionary(x => x.Channel.ToString(), StringComparer.OrdinalIgnoreCase)
            ?? throw new ArgumentNullException(nameof(providers));
    }

    public NotificationInstance Deliver(NotificationDeliveryEnvelope envelope)
    {
        ArgumentNullException.ThrowIfNull(envelope);

        var attempts = 0;
        while (attempts < envelope.MaxAttempts)
        {
            attempts++;
            var allSucceeded = true;
            var parsedChannels = envelope.Instance.Channels
                .Select(ParseChannel)
                .Where(x => x != null)
                .Select(x => x!.Value)
                .ToList();

            var category = ParseCategory(envelope.Instance.Category);
            var priority = ParsePriority(envelope.Instance.Priority);

            foreach (var channel in envelope.Instance.Channels)
            {
                if (!_providers.TryGetValue(channel, out var provider))
                {
                    allSucceeded = false;
                    continue;
                }

                var success = provider.Deliver(new GeneratedNotification(
                    envelope.Instance.NotificationId,
                    envelope.Instance.EventCode,
                    envelope.Instance.RecipientId,
                    category,
                    priority,
                    envelope.Instance.Subject,
                    envelope.Instance.Body,
                    parsedChannels,
                    envelope.Instance.ScheduledAtUtc,
                    attempts,
                    false,
                    "Pending"));

                if (!success)
                {
                    allSucceeded = false;
                }
            }

            if (allSucceeded)
            {
                return envelope.Instance with
                {
                    AttemptCount = attempts,
                    IsDelivered = true,
                    Status = "Delivered"
                };
            }
        }

        return envelope.Instance with
        {
            AttemptCount = attempts,
            IsDelivered = false,
            Status = "Failed"
        };
    }

    private static DeliveryChannel? ParseChannel(string channel)
    {
        return Enum.TryParse<DeliveryChannel>(channel, true, out var parsed)
            ? parsed
            : null;
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
