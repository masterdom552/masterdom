using Masterdom.Modules.Notifications.Application.Models;

namespace Masterdom.Modules.Notifications.Application.Services;

public sealed class NotificationTemplateRegistry : INotificationTemplateRegistry
{
    private static readonly IReadOnlyDictionary<string, NotificationTemplate> Templates =
        new Dictionary<string, NotificationTemplate>(StringComparer.OrdinalIgnoreCase)
        {
            [EventCodeCatalog.BillGenerated] = new(
                EventCodeCatalog.BillGenerated,
                "Bill generated",
                "A new bill was generated for your account.",
                NotificationCategory.Billing,
                NotificationPriority.Normal,
                [DeliveryChannel.Email, DeliveryChannel.Push]),
            [EventCodeCatalog.BillFinalized] = new(
                EventCodeCatalog.BillFinalized,
                "Bill finalized",
                "Your bill has been finalized.",
                NotificationCategory.Billing,
                NotificationPriority.High,
                [DeliveryChannel.Email, DeliveryChannel.Push]),
            [EventCodeCatalog.PaymentReceived] = new(
                EventCodeCatalog.PaymentReceived,
                "Payment received",
                "A payment has been received.",
                NotificationCategory.Payment,
                NotificationPriority.Normal,
                [DeliveryChannel.Email, DeliveryChannel.Push]),
            [EventCodeCatalog.PaymentReversed] = new(
                EventCodeCatalog.PaymentReversed,
                "Payment reversed",
                "A payment was reversed.",
                NotificationCategory.Payment,
                NotificationPriority.High,
                [DeliveryChannel.Email, DeliveryChannel.Sms, DeliveryChannel.Push]),
            [EventCodeCatalog.MeterReadingReminder] = new(
                EventCodeCatalog.MeterReadingReminder,
                "Meter reading reminder",
                "Please submit your latest meter reading.",
                NotificationCategory.Metering,
                NotificationPriority.Normal,
                [DeliveryChannel.Email, DeliveryChannel.Sms]),
            [EventCodeCatalog.MissingMeterReadingReminder] = new(
                EventCodeCatalog.MissingMeterReadingReminder,
                "Missing meter reading",
                "A meter reading is overdue.",
                NotificationCategory.Metering,
                NotificationPriority.High,
                [DeliveryChannel.Email, DeliveryChannel.Sms, DeliveryChannel.Push]),
            [EventCodeCatalog.UpcomingMoveIn] = new(
                EventCodeCatalog.UpcomingMoveIn,
                "Upcoming move-in",
                "A move-in date is approaching.",
                NotificationCategory.Tenancy,
                NotificationPriority.Normal,
                [DeliveryChannel.Email, DeliveryChannel.Push]),
            [EventCodeCatalog.UpcomingMoveOut] = new(
                EventCodeCatalog.UpcomingMoveOut,
                "Upcoming move-out",
                "A move-out date is approaching.",
                NotificationCategory.Tenancy,
                NotificationPriority.Normal,
                [DeliveryChannel.Email, DeliveryChannel.Push]),
            [EventCodeCatalog.TenancyCreated] = new(
                EventCodeCatalog.TenancyCreated,
                "Tenancy created",
                "A new tenancy was created.",
                NotificationCategory.Tenancy,
                NotificationPriority.Normal,
                [DeliveryChannel.Email, DeliveryChannel.Push]),
            [EventCodeCatalog.TenancyClosed] = new(
                EventCodeCatalog.TenancyClosed,
                "Tenancy closed",
                "A tenancy was closed.",
                NotificationCategory.Tenancy,
                NotificationPriority.High,
                [DeliveryChannel.Email, DeliveryChannel.Push]),
            [EventCodeCatalog.MaintenanceTicketUpdated] = new(
                EventCodeCatalog.MaintenanceTicketUpdated,
                "Maintenance update",
                "A maintenance ticket has been updated.",
                NotificationCategory.Maintenance,
                NotificationPriority.Normal,
                [DeliveryChannel.Email, DeliveryChannel.Push]),
            [EventCodeCatalog.SystemAnnouncement] = new(
                EventCodeCatalog.SystemAnnouncement,
                "System announcement",
                "There is an important system announcement.",
                NotificationCategory.Announcement,
                NotificationPriority.Critical,
                [DeliveryChannel.Email, DeliveryChannel.Push, DeliveryChannel.WhatsApp])
        };

    public NotificationTemplate Resolve(string eventCode)
    {
        var normalized = EventCodeCatalog.Normalize(eventCode);
        return Templates.TryGetValue(normalized, out var template)
            ? template
            : throw new InvalidOperationException($"No notification template exists for event '{eventCode}'.");
    }

    public IReadOnlyCollection<NotificationTemplate> GetAll() => Templates.Values.ToList();
}
