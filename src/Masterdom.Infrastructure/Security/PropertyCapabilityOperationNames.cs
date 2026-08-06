namespace Masterdom.Infrastructure.Security;

internal static class PropertyCapabilityOperationNames
{
    public const string CreateProperty = "properties.create";
    public const string RenameProperty = "properties.rename";
    public const string ChangePropertyStatus = "properties.status.change";
    public const string CreateUnit = "properties.units.create";
    public const string RemoveUnit = "properties.units.remove";
    public const string GetPropertyById = "properties.read.by-id";
    public const string GetPropertyByCode = "properties.read.by-code";
    public const string ListUnits = "properties.units.list";
    public const string SearchProperties = "properties.search";

    public const string CreatePerson = "people.create";
    public const string RenamePerson = "people.rename";
    public const string ChangePersonStatus = "people.status.change";
    public const string AddContact = "people.contacts.add";
    public const string RemoveContact = "people.contacts.remove";
    public const string AddIdentityDocument = "people.documents.add";
    public const string AddRelationship = "people.relationships.add";
    public const string GetPersonById = "people.read.by-id";
    public const string GetPersonByNumber = "people.read.by-number";
    public const string SearchPeople = "people.search";

    public const string CreateLease = "leases.create";
    public const string ActivateLease = "leases.activate";
    public const string RenewLease = "leases.renew";
    public const string TerminateLease = "leases.terminate";
    public const string ExpireLease = "leases.expire";
    public const string CloseLease = "leases.close";
    public const string ChangeCommercialTerms = "leases.commercial-terms.change";
    public const string GetLeaseById = "leases.read.by-id";
    public const string GetLeaseByNumber = "leases.read.by-number";

    public const string CreateTenancy = "tenancies.create";
    public const string AddOccupant = "tenancies.occupants.add";
    public const string RemoveOccupant = "tenancies.occupants.remove";
    public const string RecordMoveIn = "tenancies.move-in.record";
    public const string RecordMoveOut = "tenancies.move-out.record";
    public const string CloseTenancy = "tenancies.close";
    public const string ArchiveTenancy = "tenancies.archive";
    public const string UpdateTenancyNotes = "tenancies.notes.update";
    public const string GetTenancyById = "tenancies.read.by-id";

    public const string InstallMeter = "metering.install";
    public const string SubmitReading = "metering.readings.submit";
    public const string ApproveReading = "metering.readings.approve";
    public const string CorrectReading = "metering.readings.correct";
    public const string RetireMeter = "metering.retire";
    public const string GetMeterById = "metering.read.by-id";
    public const string GetMeterByNumber = "metering.read.by-number";

    public const string CreateMaintenanceTicket = "maintenance.tickets.create";
    public const string AssignMaintenanceTicket = "maintenance.tickets.assign";
    public const string GetMaintenanceTicketById = "maintenance.tickets.read.by-id";

    public const string CreateInventoryItem = "inventory.items.create";

    public const string CreateIdentityRole = "identity.roles.create";

    public const string GenerateBill = "billing.generate";
    public const string FinalizeBill = "billing.finalize";
    public const string AddAdjustment = "billing.adjustments.add";
    public const string ApplyCredit = "billing.credits.apply";
    public const string VoidBill = "billing.void";
    public const string GetBillById = "billing.read.by-id";
    public const string GetBillByNumber = "billing.read.by-number";

    public const string OpenLedger = "financialledger.open";
    public const string PostBillingJournal = "financialledger.post.billing";
    public const string PostPaymentJournal = "financialledger.post.payment";
    public const string ReverseJournal = "financialledger.reverse";
    public const string CompletePostingBatch = "financialledger.batch.complete";
    public const string GetLedgerById = "financialledger.read.by-id";
    public const string GetLedgerByCode = "financialledger.read.by-code";

    public const string ReceivePayment = "payment.receive";
    public const string AllocatePayment = "payment.allocate";
    public const string ReversePayment = "payment.reverse";
    public const string VoidPayment = "payment.void";
    public const string GetPaymentById = "payment.read.by-id";
    public const string GetPaymentByReference = "payment.read.by-reference";

    public const string GenerateReport = "reporting.generate";

    public const string GenerateNotification = "notifications.generate";
    public const string GetNotificationHistory = "notifications.history";

    public const string GenerateDocument = "documents.generate";
    public const string PreviewDocument = "documents.preview";
    public const string DownloadDocument = "documents.download";
    public const string RegenerateDocument = "documents.regenerate";
    public const string GetDocumentHistory = "documents.history";
}
