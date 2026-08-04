using Masterdom.Modules.Notifications.Application.Models;

namespace Masterdom.Modules.Notifications.Application.Services;

public interface INotificationTemplateRegistry
{
    NotificationTemplate Resolve(string eventCode);

    IReadOnlyCollection<NotificationTemplate> GetAll();
}
