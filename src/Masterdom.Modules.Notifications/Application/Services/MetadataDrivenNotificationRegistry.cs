using Masterdom.Modules.Notifications.Application.Models;
using Masterdom.Platform.Notifications;
using Masterdom.Platform.ReadModels;

namespace Masterdom.Modules.Notifications.Application.Services;

public sealed class MetadataDrivenNotificationRegistry : INotificationRegistry
{
    private static readonly IReadOnlyCollection<NotificationRegistration> Registrations =
    [
        new(EventCodeCatalog.BillGenerated, 1, BaselineReadModelKeys.BillsGenerated, EventCodeCatalog.BillGenerated, "recipient.direct", ["Email", "Push"], "Normal", 3, 5, "immediate", true),
        new(EventCodeCatalog.BillFinalized, 1, BaselineReadModelKeys.BillsFinalized, EventCodeCatalog.BillFinalized, "recipient.direct", ["Email", "Push"], "High", 3, 5, "immediate", true),
        new(EventCodeCatalog.PaymentReceived, 1, BaselineReadModelKeys.PaymentRegister, EventCodeCatalog.PaymentReceived, "recipient.direct", ["Email", "Push"], "Normal", 3, 5, "immediate", true),
        new(EventCodeCatalog.PaymentReversed, 1, BaselineReadModelKeys.PaymentReversals, EventCodeCatalog.PaymentReversed, "recipient.direct", ["Email", "Sms", "Push"], "High", 3, 5, "immediate", true),
        new(EventCodeCatalog.MeterReadingReminder, 1, BaselineReadModelKeys.MeterReadingHistory, EventCodeCatalog.MeterReadingReminder, "recipient.direct", ["Email", "Sms"], "Normal", 3, 5, "scheduled", true),
        new(EventCodeCatalog.MissingMeterReadingReminder, 1, BaselineReadModelKeys.MissingReadings, EventCodeCatalog.MissingMeterReadingReminder, "recipient.direct", ["Email", "Sms", "Push"], "High", 3, 5, "scheduled", true),
        new(EventCodeCatalog.UpcomingMoveIn, 1, BaselineReadModelKeys.UpcomingMoveIns, EventCodeCatalog.UpcomingMoveIn, "recipient.direct", ["Email", "Push"], "Normal", 3, 5, "scheduled", true),
        new(EventCodeCatalog.UpcomingMoveOut, 1, BaselineReadModelKeys.UpcomingMoveOuts, EventCodeCatalog.UpcomingMoveOut, "recipient.direct", ["Email", "Push"], "Normal", 3, 5, "scheduled", true),
        new(EventCodeCatalog.TenancyCreated, 1, BaselineReadModelKeys.ActiveTenancies, EventCodeCatalog.TenancyCreated, "recipient.direct", ["Email", "Push"], "Normal", 3, 5, "immediate", true),
        new(EventCodeCatalog.TenancyClosed, 1, BaselineReadModelKeys.ActiveTenancies, EventCodeCatalog.TenancyClosed, "recipient.direct", ["Email", "Push"], "High", 3, 5, "immediate", true),
        new(EventCodeCatalog.MaintenanceTicketUpdated, 1, BaselineReadModelKeys.OccupancySummary, EventCodeCatalog.MaintenanceTicketUpdated, "recipient.direct", ["Email", "Push"], "Normal", 3, 5, "immediate", true),
        new(EventCodeCatalog.SystemAnnouncement, 1, BaselineReadModelKeys.CollectionSummary, EventCodeCatalog.SystemAnnouncement, "recipient.direct", ["Email", "Push", "WhatsApp"], "Critical", 3, 5, "scheduled", true)
    ];

    public NotificationRegistration Resolve(string eventCode)
    {
        var normalized = EventCodeCatalog.Normalize(eventCode);
        return Registrations.FirstOrDefault(x => x.EventCode.Equals(normalized, StringComparison.OrdinalIgnoreCase))
            ?? throw new InvalidOperationException($"No notification registration exists for event '{eventCode}'.");
    }

    public IReadOnlyCollection<NotificationRegistration> GetAll() => Registrations;
}
