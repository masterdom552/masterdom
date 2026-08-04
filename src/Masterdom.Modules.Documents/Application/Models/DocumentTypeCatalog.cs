namespace Masterdom.Modules.Documents.Application.Models;

public static class DocumentTypeCatalog
{
    public const string TenancyAgreement = "tenancy-agreement";
    public const string MoveInForm = "move-in-form";
    public const string MoveOutForm = "move-out-form";
    public const string OccupancyCertificate = "occupancy-certificate";
    public const string Noc = "noc";

    public const string Bill = "bill";
    public const string TaxInvoice = "tax-invoice";
    public const string DebitNote = "debit-note";
    public const string CreditNote = "credit-note";

    public const string PaymentReceipt = "payment-receipt";
    public const string RefundReceipt = "refund-receipt";

    public const string AccountStatement = "account-statement";
    public const string LedgerStatement = "ledger-statement";

    public const string PropertySummary = "property-summary";

    public const string ArrearsNotice = "arrears-notice";
    public const string PaymentReminder = "payment-reminder";
    public const string DemandNotice = "demand-notice";

    public static readonly IReadOnlyCollection<string> All =
    [
        TenancyAgreement,
        MoveInForm,
        MoveOutForm,
        OccupancyCertificate,
        Noc,
        Bill,
        TaxInvoice,
        DebitNote,
        CreditNote,
        PaymentReceipt,
        RefundReceipt,
        AccountStatement,
        LedgerStatement,
        PropertySummary,
        ArrearsNotice,
        PaymentReminder,
        DemandNotice
    ];

    public static string Normalize(string documentType)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(documentType);

        var normalized = documentType.Trim().ToLowerInvariant();
        if (!All.Contains(normalized, StringComparer.OrdinalIgnoreCase))
        {
            throw new ArgumentException($"Unsupported document type '{documentType}'.", nameof(documentType));
        }

        return normalized;
    }
}
