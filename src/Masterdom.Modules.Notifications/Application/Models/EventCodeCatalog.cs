namespace Masterdom.Modules.Notifications.Application.Models;

public static class EventCodeCatalog
{
    public const string BillGenerated = "bill-generated";
    public const string BillFinalized = "bill-finalized";
    public const string PaymentReceived = "payment-received";
    public const string PaymentReversed = "payment-reversed";
    public const string MeterReadingReminder = "meter-reading-reminder";
    public const string MissingMeterReadingReminder = "missing-meter-reading-reminder";
    public const string UpcomingMoveIn = "upcoming-move-in";
    public const string UpcomingMoveOut = "upcoming-move-out";
    public const string TenancyCreated = "tenancy-created";
    public const string TenancyClosed = "tenancy-closed";
    public const string MaintenanceTicketUpdated = "maintenance-ticket-updated";
    public const string SystemAnnouncement = "system-announcement";

    public static readonly IReadOnlyCollection<string> All =
    [
        BillGenerated,
        BillFinalized,
        PaymentReceived,
        PaymentReversed,
        MeterReadingReminder,
        MissingMeterReadingReminder,
        UpcomingMoveIn,
        UpcomingMoveOut,
        TenancyCreated,
        TenancyClosed,
        MaintenanceTicketUpdated,
        SystemAnnouncement
    ];

    public static string Normalize(string eventCode)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(eventCode);

        var normalized = eventCode.Trim().ToLowerInvariant();
        if (!All.Contains(normalized, StringComparer.OrdinalIgnoreCase))
        {
            throw new ArgumentException($"Unsupported event code '{eventCode}'.", nameof(eventCode));
        }

        return normalized;
    }
}
