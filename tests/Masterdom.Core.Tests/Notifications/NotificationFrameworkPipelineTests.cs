using Masterdom.Modules.Notifications.Application.Models;
using Masterdom.Modules.Notifications.Application.Services;
using Masterdom.Platform.Notifications;
using Masterdom.Platform.ReadModels;

namespace Masterdom.Core.Tests.Notifications;

public sealed class NotificationFrameworkPipelineTests
{
    [Fact]
    public void GenerationEngine_ShouldGenerateWithoutDelivery()
    {
        var engine = new NotificationGenerationEngine(
            new NotificationTemplateRegistry(),
            new InMemoryNotificationPreferenceStore(),
            new FakeReadModelProjectionOrchestrator(),
            new DefaultNotificationTemplateRenderer(),
            new DirectRecipientResolver(),
            new PreferenceNotificationChannelResolver());

        var registration = new MetadataDrivenNotificationRegistry().Resolve(EventCodeCatalog.BillGenerated);

        var envelope = engine.Generate(
            registration,
            Guid.NewGuid(),
            DateTime.UtcNow,
            new Dictionary<string, string>(),
            null);

        Assert.Equal("Queued", envelope.Instance.Status);
        Assert.False(envelope.Instance.IsDelivered);
        Assert.Equal(0, envelope.Instance.AttemptCount);
    }

    [Fact]
    public void DeliveryProcessor_ShouldDeliverQueuedInstance()
    {
        var processor = new NotificationDeliveryProcessor(
            [
                new AlwaysSuccessProvider(DeliveryChannel.Email),
                new AlwaysSuccessProvider(DeliveryChannel.Push)
            ]);

        var instance = new NotificationInstance(
            Guid.CreateVersion7().ToString("N"),
            EventCodeCatalog.BillGenerated,
            Guid.NewGuid(),
            "subject",
            "body",
            ["Email", "Push"],
            "Billing",
            "Normal",
            DateTime.UtcNow,
            0,
            false,
            "Queued");

        var delivered = processor.Deliver(new NotificationDeliveryEnvelope(instance, 3, 5));

        Assert.True(delivered.IsDelivered);
        Assert.Equal("Delivered", delivered.Status);
        Assert.True(delivered.AttemptCount >= 1);
    }

    private sealed class AlwaysSuccessProvider : IDeliveryProvider
    {
        public AlwaysSuccessProvider(DeliveryChannel channel)
        {
            Channel = channel;
        }

        public DeliveryChannel Channel { get; }

        public bool Deliver(GeneratedNotification notification)
        {
            _ = notification;
            return true;
        }
    }

    private sealed class FakeReadModelProjectionOrchestrator : IReadModelProjectionOrchestrator
    {
        public IReadOnlyCollection<ReadModelProjectionResult> Project(string readModelKey, ReadModelProjectionRequest request)
        {
            _ = readModelKey;
            _ = request;

            return
            [
                new ReadModelProjectionResult(
                    new ReadModelMetadata(
                        "test",
                        "test",
                        1,
                        "test",
                        "Fake",
                        ["Notifications"],
                        [],
                        new Dictionary<string, string>()),
                    [new ReadModelRecord(new Dictionary<string, string> { ["name"] = "Resident" })],
                    DateTime.UtcNow)
            ];
        }
    }
}
