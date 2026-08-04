using Masterdom.Platform.Notifications;

namespace Masterdom.Modules.Notifications.Application.Services;

public sealed class DirectRecipientResolver : INotificationRecipientResolver
{
    public Guid Resolve(string resolverCode, Guid requestedRecipientId, IReadOnlyDictionary<string, string> parameters)
    {
        _ = resolverCode;
        _ = parameters;
        return requestedRecipientId;
    }
}
