using Masterdom.Modules.Notifications.Application.Models;

namespace Masterdom.Modules.Notifications.Application.Services;

public sealed class InMemoryNotificationPreferenceStore : INotificationPreferenceStore
{
    private readonly Dictionary<Guid, NotificationPreference> _preferences = new();

    public NotificationPreference Get(Guid recipientId)
    {
        if (_preferences.TryGetValue(recipientId, out var preference))
        {
            return preference;
        }

        return new NotificationPreference(
            recipientId,
            [DeliveryChannel.Email, DeliveryChannel.Push],
            true,
            22,
            6);
    }

    public void Save(NotificationPreference preference)
    {
        ArgumentNullException.ThrowIfNull(preference);
        _preferences[preference.RecipientId] = preference;
    }
}
