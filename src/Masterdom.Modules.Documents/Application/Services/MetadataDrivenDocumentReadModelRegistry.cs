using Masterdom.Modules.Documents.Application.Models;
using Masterdom.Platform.ReadModels;

namespace Masterdom.Modules.Documents.Application.Services;

public sealed class MetadataDrivenDocumentReadModelRegistry : IDocumentReadModelRegistry
{
    private static readonly IReadOnlyCollection<DocumentReadModelRegistration> Registrations =
    [
        new(DocumentTypeCatalog.TenancyAgreement, BaselineReadModelKeys.ActiveTenancies, DocumentCategory.Tenancy, "tpl-tenancy-agreement", ["tenancyId"], "Tenancy agreement document."),
        new(DocumentTypeCatalog.MoveInForm, BaselineReadModelKeys.UpcomingMoveIns, DocumentCategory.Tenancy, "tpl-move-in-form", ["tenancyId"], "Move-in form document."),
        new(DocumentTypeCatalog.MoveOutForm, BaselineReadModelKeys.UpcomingMoveOuts, DocumentCategory.Tenancy, "tpl-move-out-form", ["tenancyId"], "Move-out form document."),
        new(DocumentTypeCatalog.OccupancyCertificate, BaselineReadModelKeys.OccupancySummary, DocumentCategory.Tenancy, "tpl-occupancy-certificate", ["propertyId"], "Occupancy certificate document."),
        new(DocumentTypeCatalog.Noc, BaselineReadModelKeys.ActiveTenancies, DocumentCategory.Tenancy, "tpl-noc", ["tenancyId"], "No-objection certificate document."),

        new(DocumentTypeCatalog.Bill, BaselineReadModelKeys.BillsGenerated, DocumentCategory.Billing, "tpl-bill", ["billId"], "Bill document."),
        new(DocumentTypeCatalog.TaxInvoice, BaselineReadModelKeys.BillsFinalized, DocumentCategory.Billing, "tpl-tax-invoice", ["billId"], "Tax invoice document."),
        new(DocumentTypeCatalog.DebitNote, BaselineReadModelKeys.ChargeBreakdown, DocumentCategory.Billing, "tpl-debit-note", ["billId"], "Debit note document."),
        new(DocumentTypeCatalog.CreditNote, BaselineReadModelKeys.BillsVoided, DocumentCategory.Billing, "tpl-credit-note", ["billId"], "Credit note document."),

        new(DocumentTypeCatalog.PaymentReceipt, BaselineReadModelKeys.PaymentRegister, DocumentCategory.Payment, "tpl-payment-receipt", ["paymentId"], "Payment receipt document."),
        new(DocumentTypeCatalog.RefundReceipt, BaselineReadModelKeys.PaymentReversals, DocumentCategory.Payment, "tpl-refund-receipt", ["paymentId"], "Refund receipt document."),

        new(DocumentTypeCatalog.AccountStatement, BaselineReadModelKeys.AccountBalances, DocumentCategory.FinancialLedger, "tpl-account-statement", ["accountCode"], "Account statement document."),
        new(DocumentTypeCatalog.LedgerStatement, BaselineReadModelKeys.GeneralLedger, DocumentCategory.FinancialLedger, "tpl-ledger-statement", ["journalNumber"], "Ledger statement document."),

        new(DocumentTypeCatalog.PropertySummary, BaselineReadModelKeys.OccupancySummary, DocumentCategory.Property, "tpl-property-summary", ["propertyId"], "Property summary document."),

        new(DocumentTypeCatalog.ArrearsNotice, BaselineReadModelKeys.OutstandingBills, DocumentCategory.Management, "tpl-arrears-notice", ["billId"], "Arrears notice document."),
        new(DocumentTypeCatalog.PaymentReminder, BaselineReadModelKeys.OutstandingBills, DocumentCategory.Management, "tpl-payment-reminder", ["billId"], "Payment reminder document."),
        new(DocumentTypeCatalog.DemandNotice, BaselineReadModelKeys.OutstandingBills, DocumentCategory.Management, "tpl-demand-notice", ["billId"], "Demand notice document.")
    ];

    public DocumentReadModelRegistration Resolve(string documentType)
    {
        var normalized = DocumentTypeCatalog.Normalize(documentType);

        return Registrations.FirstOrDefault(x => x.DocumentType.Equals(normalized, StringComparison.OrdinalIgnoreCase))
            ?? throw new InvalidOperationException($"No read model registration exists for document type '{documentType}'.");
    }

    public IReadOnlyCollection<DocumentReadModelRegistration> GetAll() => Registrations;
}
