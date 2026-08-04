using Masterdom.Core.Security;

namespace Masterdom.Infrastructure.Security;

internal sealed class DefaultCapabilityAuthorizationPolicyProvider : ICapabilityAuthorizationPolicyProvider
{
    private static readonly IReadOnlyDictionary<string, CapabilityAuthorizationPolicy> Policies =
        new Dictionary<string, CapabilityAuthorizationPolicy>(StringComparer.OrdinalIgnoreCase)
        {
            [PropertyCapabilityOperationNames.CreateProperty] = new(PropertyCapabilityOperationNames.CreateProperty, "properties.create", false, true, false),
            [PropertyCapabilityOperationNames.RenameProperty] = new(PropertyCapabilityOperationNames.RenameProperty, "properties.manage", true, true, false),
            [PropertyCapabilityOperationNames.ChangePropertyStatus] = new(PropertyCapabilityOperationNames.ChangePropertyStatus, "properties.manage", true, true, false),
            [PropertyCapabilityOperationNames.CreateUnit] = new(PropertyCapabilityOperationNames.CreateUnit, "properties.manage", true, true, false),
            [PropertyCapabilityOperationNames.RemoveUnit] = new(PropertyCapabilityOperationNames.RemoveUnit, "properties.manage", true, true, false),
            [PropertyCapabilityOperationNames.GetPropertyById] = new(PropertyCapabilityOperationNames.GetPropertyById, "properties.read", true, true, false),
            [PropertyCapabilityOperationNames.GetPropertyByCode] = new(PropertyCapabilityOperationNames.GetPropertyByCode, "properties.read", true, true, false),
            [PropertyCapabilityOperationNames.ListUnits] = new(PropertyCapabilityOperationNames.ListUnits, "properties.read", true, true, false),
            [PropertyCapabilityOperationNames.SearchProperties] = new(PropertyCapabilityOperationNames.SearchProperties, "properties.read", true, true, false),

            [PropertyCapabilityOperationNames.CreatePerson] = new(PropertyCapabilityOperationNames.CreatePerson, "people.create", false, false, false),
            [PropertyCapabilityOperationNames.RenamePerson] = new(PropertyCapabilityOperationNames.RenamePerson, "people.manage", false, false, true),
            [PropertyCapabilityOperationNames.ChangePersonStatus] = new(PropertyCapabilityOperationNames.ChangePersonStatus, "people.manage", false, false, false),
            [PropertyCapabilityOperationNames.AddContact] = new(PropertyCapabilityOperationNames.AddContact, "people.manage", false, false, true),
            [PropertyCapabilityOperationNames.RemoveContact] = new(PropertyCapabilityOperationNames.RemoveContact, "people.manage", false, false, true),
            [PropertyCapabilityOperationNames.AddIdentityDocument] = new(PropertyCapabilityOperationNames.AddIdentityDocument, "people.manage", false, false, true),
            [PropertyCapabilityOperationNames.AddRelationship] = new(PropertyCapabilityOperationNames.AddRelationship, "people.manage", false, false, false),
            [PropertyCapabilityOperationNames.GetPersonById] = new(PropertyCapabilityOperationNames.GetPersonById, "people.read", false, false, true),
            [PropertyCapabilityOperationNames.GetPersonByNumber] = new(PropertyCapabilityOperationNames.GetPersonByNumber, "people.read", false, false, false),
            [PropertyCapabilityOperationNames.SearchPeople] = new(PropertyCapabilityOperationNames.SearchPeople, "people.read", false, false, false),

            [PropertyCapabilityOperationNames.CreateLease] = new(PropertyCapabilityOperationNames.CreateLease, "leases.create", true, true, false),
            [PropertyCapabilityOperationNames.ActivateLease] = new(PropertyCapabilityOperationNames.ActivateLease, "leases.manage", true, true, false),
            [PropertyCapabilityOperationNames.RenewLease] = new(PropertyCapabilityOperationNames.RenewLease, "leases.manage", true, true, false),
            [PropertyCapabilityOperationNames.TerminateLease] = new(PropertyCapabilityOperationNames.TerminateLease, "leases.manage", true, true, false),
            [PropertyCapabilityOperationNames.ExpireLease] = new(PropertyCapabilityOperationNames.ExpireLease, "leases.manage", true, true, false),
            [PropertyCapabilityOperationNames.CloseLease] = new(PropertyCapabilityOperationNames.CloseLease, "leases.manage", true, true, false),
            [PropertyCapabilityOperationNames.ChangeCommercialTerms] = new(PropertyCapabilityOperationNames.ChangeCommercialTerms, "leases.manage", true, true, false),
            [PropertyCapabilityOperationNames.GetLeaseById] = new(PropertyCapabilityOperationNames.GetLeaseById, "leases.read", true, true, false),
            [PropertyCapabilityOperationNames.GetLeaseByNumber] = new(PropertyCapabilityOperationNames.GetLeaseByNumber, "leases.read", true, true, false),

            [PropertyCapabilityOperationNames.CreateTenancy] = new(PropertyCapabilityOperationNames.CreateTenancy, "tenancies.create", true, true, false),
            [PropertyCapabilityOperationNames.AddOccupant] = new(PropertyCapabilityOperationNames.AddOccupant, "tenancies.manage", true, true, false),
            [PropertyCapabilityOperationNames.RemoveOccupant] = new(PropertyCapabilityOperationNames.RemoveOccupant, "tenancies.manage", true, true, false),
            [PropertyCapabilityOperationNames.RecordMoveIn] = new(PropertyCapabilityOperationNames.RecordMoveIn, "tenancies.manage", true, true, false),
            [PropertyCapabilityOperationNames.RecordMoveOut] = new(PropertyCapabilityOperationNames.RecordMoveOut, "tenancies.manage", true, true, false),
            [PropertyCapabilityOperationNames.CloseTenancy] = new(PropertyCapabilityOperationNames.CloseTenancy, "tenancies.manage", true, true, false),
            [PropertyCapabilityOperationNames.ArchiveTenancy] = new(PropertyCapabilityOperationNames.ArchiveTenancy, "tenancies.manage", true, true, false),
            [PropertyCapabilityOperationNames.GetTenancyById] = new(PropertyCapabilityOperationNames.GetTenancyById, "tenancies.read", true, true, false),

            [PropertyCapabilityOperationNames.InstallMeter] = new(PropertyCapabilityOperationNames.InstallMeter, "metering.install", true, true, false),
            [PropertyCapabilityOperationNames.SubmitReading] = new(PropertyCapabilityOperationNames.SubmitReading, "metering.readings.submit", true, true, false),
            [PropertyCapabilityOperationNames.ApproveReading] = new(PropertyCapabilityOperationNames.ApproveReading, "metering.readings.approve", true, true, false),
            [PropertyCapabilityOperationNames.CorrectReading] = new(PropertyCapabilityOperationNames.CorrectReading, "metering.readings.correct", true, true, false),
            [PropertyCapabilityOperationNames.RetireMeter] = new(PropertyCapabilityOperationNames.RetireMeter, "metering.retire", true, true, false),
            [PropertyCapabilityOperationNames.GetMeterById] = new(PropertyCapabilityOperationNames.GetMeterById, "metering.read", true, true, false),
            [PropertyCapabilityOperationNames.GetMeterByNumber] = new(PropertyCapabilityOperationNames.GetMeterByNumber, "metering.read", true, true, false),

            [PropertyCapabilityOperationNames.GenerateBill] = new(PropertyCapabilityOperationNames.GenerateBill, "billing.generate", true, true, false),
            [PropertyCapabilityOperationNames.FinalizeBill] = new(PropertyCapabilityOperationNames.FinalizeBill, "billing.manage", true, true, false),
            [PropertyCapabilityOperationNames.AddAdjustment] = new(PropertyCapabilityOperationNames.AddAdjustment, "billing.manage", true, true, false),
            [PropertyCapabilityOperationNames.ApplyCredit] = new(PropertyCapabilityOperationNames.ApplyCredit, "billing.manage", true, true, false),
            [PropertyCapabilityOperationNames.VoidBill] = new(PropertyCapabilityOperationNames.VoidBill, "billing.manage", true, true, false),
            [PropertyCapabilityOperationNames.GetBillById] = new(PropertyCapabilityOperationNames.GetBillById, "billing.read", true, true, false),
            [PropertyCapabilityOperationNames.GetBillByNumber] = new(PropertyCapabilityOperationNames.GetBillByNumber, "billing.read", true, true, false),

            [PropertyCapabilityOperationNames.OpenLedger] = new(PropertyCapabilityOperationNames.OpenLedger, "financialledger.open", false, false, false),
            [PropertyCapabilityOperationNames.PostBillingJournal] = new(PropertyCapabilityOperationNames.PostBillingJournal, "financialledger.post", false, false, false),
            [PropertyCapabilityOperationNames.PostPaymentJournal] = new(PropertyCapabilityOperationNames.PostPaymentJournal, "financialledger.post", false, false, false),
            [PropertyCapabilityOperationNames.ReverseJournal] = new(PropertyCapabilityOperationNames.ReverseJournal, "financialledger.manage", false, false, false),
            [PropertyCapabilityOperationNames.CompletePostingBatch] = new(PropertyCapabilityOperationNames.CompletePostingBatch, "financialledger.manage", false, false, false),
            [PropertyCapabilityOperationNames.GetLedgerById] = new(PropertyCapabilityOperationNames.GetLedgerById, "financialledger.read", false, false, false),
            [PropertyCapabilityOperationNames.GetLedgerByCode] = new(PropertyCapabilityOperationNames.GetLedgerByCode, "financialledger.read", false, false, false),

            [PropertyCapabilityOperationNames.ReceivePayment] = new(PropertyCapabilityOperationNames.ReceivePayment, "payment.receive", false, false, false),
            [PropertyCapabilityOperationNames.AllocatePayment] = new(PropertyCapabilityOperationNames.AllocatePayment, "payment.allocate", false, false, false),
            [PropertyCapabilityOperationNames.ReversePayment] = new(PropertyCapabilityOperationNames.ReversePayment, "payment.manage", false, false, false),
            [PropertyCapabilityOperationNames.VoidPayment] = new(PropertyCapabilityOperationNames.VoidPayment, "payment.manage", false, false, false),
            [PropertyCapabilityOperationNames.GetPaymentById] = new(PropertyCapabilityOperationNames.GetPaymentById, "payment.read", false, false, false),
            [PropertyCapabilityOperationNames.GetPaymentByReference] = new(PropertyCapabilityOperationNames.GetPaymentByReference, "payment.read", false, false, false),

            [PropertyCapabilityOperationNames.GenerateReport] = new(PropertyCapabilityOperationNames.GenerateReport, "reports.read", false, false, false),

            [PropertyCapabilityOperationNames.GenerateNotification] = new(PropertyCapabilityOperationNames.GenerateNotification, "notifications.send", false, false, false),
            [PropertyCapabilityOperationNames.GetNotificationHistory] = new(PropertyCapabilityOperationNames.GetNotificationHistory, "notifications.read", false, false, false),

            [PropertyCapabilityOperationNames.GenerateDocument] = new(PropertyCapabilityOperationNames.GenerateDocument, "documents.generate", false, false, false),
            [PropertyCapabilityOperationNames.PreviewDocument] = new(PropertyCapabilityOperationNames.PreviewDocument, "documents.generate", false, false, false),
            [PropertyCapabilityOperationNames.DownloadDocument] = new(PropertyCapabilityOperationNames.DownloadDocument, "documents.read", false, false, false),
            [PropertyCapabilityOperationNames.RegenerateDocument] = new(PropertyCapabilityOperationNames.RegenerateDocument, "documents.generate", false, false, false),
            [PropertyCapabilityOperationNames.GetDocumentHistory] = new(PropertyCapabilityOperationNames.GetDocumentHistory, "documents.read", false, false, false)
        };

    public CapabilityAuthorizationPolicy GetPolicy(string operation)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(operation);

        if (!Policies.TryGetValue(operation, out var policy))
        {
            throw new InvalidOperationException($"No authorization policy is configured for operation '{operation}'.");
        }

        return policy;
    }
}
