using Masterdom.Modules.Notifications.Application.Models;
using Masterdom.Platform.Notifications;
using Masterdom.Platform.ReadModels;

namespace Masterdom.Modules.Notifications.Application.Services;

public sealed class NotificationGenerationEngine : INotificationGenerationEngine
{
    private readonly INotificationTemplateRegistry _templateRegistry;
    private readonly INotificationPreferenceStore _preferenceStore;
    private readonly IReadModelProjectionOrchestrator _projectionOrchestrator;
    private readonly INotificationTemplateRenderer _templateRenderer;
    private readonly INotificationRecipientResolver _recipientResolver;
    private readonly INotificationChannelResolver _channelResolver;

    public NotificationGenerationEngine(
        INotificationTemplateRegistry templateRegistry,
        INotificationPreferenceStore preferenceStore,
        IReadModelProjectionOrchestrator projectionOrchestrator,
        INotificationTemplateRenderer templateRenderer,
        INotificationRecipientResolver recipientResolver,
        INotificationChannelResolver channelResolver)
    {
        _templateRegistry = templateRegistry ?? throw new ArgumentNullException(nameof(templateRegistry));
        _preferenceStore = preferenceStore ?? throw new ArgumentNullException(nameof(preferenceStore));
        _projectionOrchestrator = projectionOrchestrator ?? throw new ArgumentNullException(nameof(projectionOrchestrator));
        _templateRenderer = templateRenderer ?? throw new ArgumentNullException(nameof(templateRenderer));
        _recipientResolver = recipientResolver ?? throw new ArgumentNullException(nameof(recipientResolver));
        _channelResolver = channelResolver ?? throw new ArgumentNullException(nameof(channelResolver));
    }

    public NotificationDeliveryEnvelope Generate(
        NotificationRegistration registration,
        Guid recipientId,
        DateTime requestedAtUtc,
        IReadOnlyDictionary<string, string> parameters,
        DateTime? requestedDeliveryAtUtc)
    {
        ArgumentNullException.ThrowIfNull(registration);

        var resolvedRecipientId = _recipientResolver.Resolve(registration.RecipientResolver, recipientId, parameters);
        var template = _templateRegistry.Resolve(registration.TemplateCode);
        var preference = _preferenceStore.Get(resolvedRecipientId);
        var preferredChannels = preference.Channels.Select(x => x.ToString()).ToList();

        var hydrated = HydrateParameters(registration.ReadModelKey, requestedAtUtc, parameters);
        var subject = _templateRenderer.Render(template.SubjectTemplate, hydrated);
        var body = _templateRenderer.Render(template.BodyTemplate, hydrated);
        var channels = _channelResolver.ResolveChannels(registration, preferredChannels, preference.IsEnabled);

        var instance = new NotificationInstance(
            Guid.CreateVersion7().ToString("N"),
            registration.EventCode,
            resolvedRecipientId,
            subject,
            body,
            channels,
            template.Category.ToString(),
            registration.Priority,
            requestedDeliveryAtUtc ?? requestedAtUtc,
            0,
            false,
            "Queued");

        return new NotificationDeliveryEnvelope(instance, registration.RetryMaxAttempts, registration.RetryDelaySeconds);
    }

    private IReadOnlyDictionary<string, string> HydrateParameters(
        string readModelKey,
        DateTime asOfUtc,
        IReadOnlyDictionary<string, string> parameters)
    {
        var hydrated = new Dictionary<string, string>(parameters, StringComparer.OrdinalIgnoreCase);

        var projections = _projectionOrchestrator.Project(
            readModelKey,
            new ReadModelProjectionRequest(parameters, asOfUtc));

        var first = projections.SelectMany(x => x.Records).FirstOrDefault();
        if (first is null)
        {
            return hydrated;
        }

        foreach (var kv in first.Fields)
        {
            if (!hydrated.ContainsKey(kv.Key))
            {
                hydrated[kv.Key] = kv.Value;
            }
        }

        return hydrated;
    }
}
