using Masterdom.Platform.Notifications;

namespace Masterdom.Modules.Notifications.Application.Services;

public sealed class DefaultNotificationTemplateRenderer : INotificationTemplateRenderer
{
    public string Render(string templateText, IReadOnlyDictionary<string, string> parameters)
    {
        var output = templateText;

        foreach (var kv in parameters)
        {
            output = output.Replace($"{{{{{kv.Key}}}}}", kv.Value, StringComparison.OrdinalIgnoreCase);
        }

        return output;
    }
}
