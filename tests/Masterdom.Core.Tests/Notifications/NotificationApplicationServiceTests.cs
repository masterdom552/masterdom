using Masterdom.Modules.Notifications.Application.Models;
using Masterdom.Modules.Notifications.Application.Services;
using Masterdom.Platform.Notifications;
using Masterdom.Platform.ReadModels;

namespace Masterdom.Core.Tests.Notifications;

public sealed class NotificationApplicationServiceTests
{
    [Fact]
    public void Generate_ShouldCreateNotificationForEveryV1EventCode()
    {
        var service = CreateService();
        var recipientId = Guid.NewGuid();

        foreach (var eventCode in EventCodeCatalog.All)
        {
            var result = service.Generate(
                eventCode,
                recipientId,
                DateTime.UtcNow,
                new Dictionary<string, string>(),
                null);

            Assert.Equal(eventCode, result.EventCode);
            Assert.NotEmpty(result.NotificationId);
            Assert.NotEqual("Failed", result.Status);
        }
    }

    [Fact]
    public void Generate_ShouldPersistHistoryAndSupportPaging()
    {
        var service = CreateService();
        var recipientId = Guid.NewGuid();

        service.Generate(EventCodeCatalog.BillGenerated, recipientId, DateTime.UtcNow, new Dictionary<string, string>(), null);
        service.Generate(EventCodeCatalog.PaymentReceived, recipientId, DateTime.UtcNow, new Dictionary<string, string>(), null);

        var firstPage = service.History(recipientId, 1, 1);
        var secondPage = service.History(recipientId, 2, 1);

        Assert.Single(firstPage);
        Assert.Single(secondPage);
        Assert.NotEqual(firstPage.First().NotificationId, secondPage.First().NotificationId);
    }

    [Fact]
    public void Generate_ShouldRetryUntilSuccessOrMaxAttempts()
    {
        var service = new NotificationApplicationService(
            new MetadataDrivenNotificationRegistry(),
            new InMemoryNotificationHistoryStore(),
            new AllowNotificationAuthorizationService(),
            new NotificationGenerationEngine(
                new NotificationTemplateRegistry(),
                new InMemoryNotificationPreferenceStore(),
                new FakeReadModelProjectionOrchestrator(),
                new DefaultNotificationTemplateRenderer(),
                new DirectRecipientResolver(),
                new PreferenceNotificationChannelResolver()),
            new InMemoryNotificationDeliveryQueue(),
            new NotificationDeliveryProcessor(
                [
                    new FlakyProvider(DeliveryChannel.Email),
                    new AlwaysSuccessProvider(DeliveryChannel.Push)
                ]));

        var result = service.Generate(
            EventCodeCatalog.BillGenerated,
            Guid.NewGuid(),
            DateTime.UtcNow,
            new Dictionary<string, string>(),
            null);

        Assert.True(result.AttemptCount >= 2);
        Assert.Equal("Delivered", result.Status);
    }

    private static NotificationApplicationService CreateService()
    {
        return new NotificationApplicationService(
            new MetadataDrivenNotificationRegistry(),
            new InMemoryNotificationHistoryStore(),
            new AllowNotificationAuthorizationService(),
            new NotificationGenerationEngine(
                new NotificationTemplateRegistry(),
                new InMemoryNotificationPreferenceStore(),
                new FakeReadModelProjectionOrchestrator(),
                new DefaultNotificationTemplateRenderer(),
                new DirectRecipientResolver(),
                new PreferenceNotificationChannelResolver()),
            new InMemoryNotificationDeliveryQueue(),
            new NotificationDeliveryProcessor(
                [
                    new AlwaysSuccessProvider(DeliveryChannel.Email),
                    new AlwaysSuccessProvider(DeliveryChannel.Sms),
                    new AlwaysSuccessProvider(DeliveryChannel.Push),
                    new AlwaysSuccessProvider(DeliveryChannel.WhatsApp)
                ]));
    }

    private sealed class AllowNotificationAuthorizationService : INotificationAuthorizationService
    {
        public void EnsureAuthorized(string eventCode, Guid recipientId)
        {
            _ = eventCode;
            _ = recipientId;
        }
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

    private sealed class FlakyProvider : IDeliveryProvider
    {
        private int _attempt;

        public FlakyProvider(DeliveryChannel channel)
        {
            Channel = channel;
        }

        public DeliveryChannel Channel { get; }

        public bool Deliver(GeneratedNotification notification)
        {
            _ = notification;
            _attempt++;
            return _attempt >= 2;
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
