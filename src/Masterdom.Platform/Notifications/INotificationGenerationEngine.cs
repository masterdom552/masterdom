namespace Masterdom.Platform.Notifications;

public interface INotificationGenerationEngine
{
    NotificationDeliveryEnvelope Generate(
        NotificationRegistration registration,
        Guid recipientId,
        DateTime requestedAtUtc,
        IReadOnlyDictionary<string, string> parameters,
        DateTime? requestedDeliveryAtUtc);
}
