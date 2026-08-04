using Masterdom.Modules.Notifications.Application.Models;

namespace Masterdom.Modules.Notifications.Application.Services;

public interface INotificationPreferenceStore
{
    NotificationPreference Get(Guid recipientId);

    void Save(NotificationPreference preference);
}
