using Masterdom.Modules.Notifications.Application.Models;
using Masterdom.Platform.Notifications;

namespace Masterdom.Modules.Notifications.Application.Services;

public sealed class PreferenceNotificationChannelResolver : INotificationChannelResolver
{
    public IReadOnlyCollection<string> ResolveChannels(
        NotificationRegistration registration,
        IReadOnlyCollection<string> preferredChannels,
        bool notificationsEnabled)
    {
        if (!notificationsEnabled)
        {
            return [];
        }

        return registration.DeliveryChannels
            .Where(channel => preferredChannels.Contains(channel, StringComparer.OrdinalIgnoreCase))
            .Distinct()
            .ToList();
    }
}
