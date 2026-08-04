namespace Masterdom.Platform.Notifications;

public interface INotificationTemplateRenderer
{
    string Render(string templateText, IReadOnlyDictionary<string, string> parameters);
}
