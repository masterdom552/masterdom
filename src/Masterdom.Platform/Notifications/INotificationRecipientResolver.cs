namespace Masterdom.Platform.Notifications;

public interface INotificationRecipientResolver
{
    Guid Resolve(string resolverCode, Guid requestedRecipientId, IReadOnlyDictionary<string, string> parameters);
}
