namespace Masterdom.Platform.Notifications;

public interface INotificationChannelResolver
{
    IReadOnlyCollection<string> ResolveChannels(
        NotificationRegistration registration,
        IReadOnlyCollection<string> preferredChannels,
        bool notificationsEnabled);
}
