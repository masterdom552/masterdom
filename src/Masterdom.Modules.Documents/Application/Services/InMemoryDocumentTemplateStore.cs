using Masterdom.Modules.Documents.Application.Models;

namespace Masterdom.Modules.Documents.Application.Services;

public sealed class InMemoryDocumentTemplateStore : IDocumentTemplateStore
{
    private static readonly IReadOnlyCollection<DocumentTemplate> Templates =
    [
        new("tpl-tenancy-agreement", DocumentTypeCatalog.TenancyAgreement, 1, true, "Tenancy Agreement\nTenancy: {{tenancyId}}\nStatus: {{status}}", ["tenancyId"], new Dictionary<string, string> { ["layout"] = "text" }),
        new("tpl-move-in-form", DocumentTypeCatalog.MoveInForm, 1, true, "Move-In Form\nTenancy: {{tenancyId}}\nMove-In Date: {{moveInDate}}", ["tenancyId"], new Dictionary<string, string> { ["layout"] = "text" }),
        new("tpl-move-out-form", DocumentTypeCatalog.MoveOutForm, 1, true, "Move-Out Form\nTenancy: {{tenancyId}}\nMove-Out Date: {{moveOutDate}}", ["tenancyId"], new Dictionary<string, string> { ["layout"] = "text" }),
        new("tpl-occupancy-certificate", DocumentTypeCatalog.OccupancyCertificate, 1, true, "Occupancy Certificate\nProperty: {{propertyId}}\nOccupancy Rate: {{occupancyRate}}", ["propertyId"], new Dictionary<string, string> { ["layout"] = "text" }),
        new("tpl-noc", DocumentTypeCatalog.Noc, 1, true, "No Objection Certificate\nTenancy: {{tenancyId}}", ["tenancyId"], new Dictionary<string, string> { ["layout"] = "text" }),

        new("tpl-bill", DocumentTypeCatalog.Bill, 1, true, "Bill\nBill Number: {{billNumber}}\nStatus: {{status}}", ["billId"], new Dictionary<string, string> { ["layout"] = "text" }),
        new("tpl-tax-invoice", DocumentTypeCatalog.TaxInvoice, 1, true, "Tax Invoice\nBill Number: {{billNumber}}\nStatus: {{status}}", ["billId"], new Dictionary<string, string> { ["layout"] = "text" }),
        new("tpl-debit-note", DocumentTypeCatalog.DebitNote, 1, true, "Debit Note\nBill: {{billId}}\nCharges: {{chargeTotal}}", ["billId"], new Dictionary<string, string> { ["layout"] = "text" }),
        new("tpl-credit-note", DocumentTypeCatalog.CreditNote, 1, true, "Credit Note\nBill: {{billId}}\nStatus: {{status}}", ["billId"], new Dictionary<string, string> { ["layout"] = "text" }),

        new("tpl-payment-receipt", DocumentTypeCatalog.PaymentReceipt, 1, true, "Payment Receipt\nReference: {{paymentReference}}\nAmount: {{amount}}", ["paymentId"], new Dictionary<string, string> { ["layout"] = "text" }),
        new("tpl-refund-receipt", DocumentTypeCatalog.RefundReceipt, 1, true, "Refund Receipt\nReference: {{paymentReference}}\nReversed At: {{reversedAt}}", ["paymentId"], new Dictionary<string, string> { ["layout"] = "text" }),

        new("tpl-account-statement", DocumentTypeCatalog.AccountStatement, 1, true, "Account Statement\nAccount: {{accountCode}} - {{accountName}}\nBalance: {{balance}}", ["accountCode"], new Dictionary<string, string> { ["layout"] = "text" }),
        new("tpl-ledger-statement", DocumentTypeCatalog.LedgerStatement, 1, true, "Ledger Statement\nJournal: {{journalNumber}}\nDebits: {{debits}}\nCredits: {{credits}}", ["journalNumber"], new Dictionary<string, string> { ["layout"] = "text" }),

        new("tpl-property-summary", DocumentTypeCatalog.PropertySummary, 1, true, "Property Summary\nProperty: {{propertyId}}\nTotal Units: {{totalUnits}}\nOccupied: {{occupiedUnits}}", ["propertyId"], new Dictionary<string, string> { ["layout"] = "text" }),

        new("tpl-arrears-notice", DocumentTypeCatalog.ArrearsNotice, 1, true, "Arrears Notice\nBill: {{billNumber}}\nOutstanding: {{outstandingAmount}}", ["billId"], new Dictionary<string, string> { ["layout"] = "text" }),
        new("tpl-payment-reminder", DocumentTypeCatalog.PaymentReminder, 1, true, "Payment Reminder\nBill: {{billNumber}}\nOutstanding: {{outstandingAmount}}", ["billId"], new Dictionary<string, string> { ["layout"] = "text" }),
        new("tpl-demand-notice", DocumentTypeCatalog.DemandNotice, 1, true, "Demand Notice\nBill: {{billNumber}}\nOutstanding: {{outstandingAmount}}", ["billId"], new Dictionary<string, string> { ["layout"] = "text" })
    ];

    public DocumentTemplate Resolve(string documentType, string? templateCode, int? version)
    {
        var normalizedType = DocumentTypeCatalog.Normalize(documentType);

        var candidates = Templates
            .Where(x => x.DocumentType.Equals(normalizedType, StringComparison.OrdinalIgnoreCase))
            .ToList();

        if (!string.IsNullOrWhiteSpace(templateCode))
        {
            candidates = candidates
                .Where(x => x.TemplateCode.Equals(templateCode.Trim(), StringComparison.OrdinalIgnoreCase))
                .ToList();
        }

        if (version.HasValue)
        {
            candidates = candidates.Where(x => x.Version == version.Value).ToList();
        }
        else
        {
            candidates = candidates.Where(x => x.IsActive).ToList();
        }

        var selected = candidates.OrderByDescending(x => x.Version).FirstOrDefault();
        return selected ?? throw new InvalidOperationException($"No template resolved for document type '{documentType}'.");
    }

    public IReadOnlyCollection<DocumentTemplate> GetByDocumentType(string documentType)
    {
        var normalizedType = DocumentTypeCatalog.Normalize(documentType);
        return Templates.Where(x => x.DocumentType.Equals(normalizedType, StringComparison.OrdinalIgnoreCase)).ToList();
    }
}
