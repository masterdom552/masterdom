using Masterdom.Core.Security;
using Masterdom.Infrastructure.Persistence;
using Masterdom.Modules.Lease.Application.Commands;
using Masterdom.Modules.Lease.Application.Queries;
using Masterdom.Modules.People.Application.Commands;
using Masterdom.Modules.People.Application.Queries;
using Masterdom.Modules.Properties.Application.Commands;
using Masterdom.Modules.Properties.Application.Queries;
using Masterdom.Modules.Tenancy.Application.Commands;
using Masterdom.Modules.Tenancy.Application.Queries;
using Masterdom.Modules.Metering.Application.Commands;
using Masterdom.Modules.Metering.Application.Queries;
using Masterdom.Modules.Billing.Application.Commands;
using Masterdom.Modules.Billing.Application.Queries;
using Masterdom.Modules.Inventory.Application.Commands;
using Masterdom.Modules.Maintenance.Application.Commands;
using Masterdom.Modules.Maintenance.Application.Queries;
using Masterdom.Modules.FinancialLedger.Application.Commands;
using Masterdom.Modules.FinancialLedger.Application.Queries;
using Masterdom.Modules.Payment.Application.Commands;
using Masterdom.Modules.Payment.Application.Queries;
using Masterdom.Modules.Notifications.Application.Commands;
using Masterdom.Modules.Notifications.Application.Queries;
using Masterdom.Modules.Documents.Application.Commands;
using Masterdom.Modules.Documents.Application.Queries;
using Masterdom.Modules.Reporting.Application.Queries;
using Microsoft.EntityFrameworkCore;

namespace Masterdom.Infrastructure.Security;

internal interface IRequestAuthorizationService
{
    AuthorizationResult Authorize(object request);
}

internal sealed class RequestAuthorizationService : IRequestAuthorizationService
{
    private readonly IPropertyCapabilityAuthorizationService _authorizationService;
    private readonly MasterdomDbContext _dbContext;

    public RequestAuthorizationService(
        IPropertyCapabilityAuthorizationService authorizationService,
        MasterdomDbContext dbContext)
    {
        _authorizationService = authorizationService ?? throw new ArgumentNullException(nameof(authorizationService));
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
    }

    public AuthorizationResult Authorize(object request)
    {
        ArgumentNullException.ThrowIfNull(request);

        var context = request switch
        {
            CreatePropertyCommand => new AuthorizationContext(PropertyCapabilityOperationNames.CreateProperty),
            RenamePropertyCommand command => new AuthorizationContext(PropertyCapabilityOperationNames.RenameProperty, command.PropertyId.Value),
            ChangePropertyStatusCommand command => new AuthorizationContext(PropertyCapabilityOperationNames.ChangePropertyStatus, command.PropertyId.Value),
            CreateUnitCommand command => new AuthorizationContext(PropertyCapabilityOperationNames.CreateUnit, command.PropertyId.Value),
            RemoveUnitCommand command => new AuthorizationContext(PropertyCapabilityOperationNames.RemoveUnit, command.PropertyId.Value),
            GetPropertyByIdQuery query => new AuthorizationContext(PropertyCapabilityOperationNames.GetPropertyById, query.PropertyId.Value),
            GetPropertyByCodeQuery query => new AuthorizationContext(PropertyCapabilityOperationNames.GetPropertyByCode, ResolvePropertyId(query.Code.Value)),
            ListUnitsQuery query => new AuthorizationContext(PropertyCapabilityOperationNames.ListUnits, query.PropertyId.Value),
            SearchPropertiesQuery => new AuthorizationContext(PropertyCapabilityOperationNames.SearchProperties),

            CreatePersonCommand => new AuthorizationContext(PropertyCapabilityOperationNames.CreatePerson),
            RenamePersonCommand command => new AuthorizationContext(PropertyCapabilityOperationNames.RenamePerson, PersonId: command.PersonId.Value),
            ChangePersonStatusCommand command => new AuthorizationContext(PropertyCapabilityOperationNames.ChangePersonStatus, PersonId: command.PersonId.Value),
            AddContactCommand command => new AuthorizationContext(PropertyCapabilityOperationNames.AddContact, PersonId: command.PersonId.Value),
            RemoveContactCommand command => new AuthorizationContext(PropertyCapabilityOperationNames.RemoveContact, PersonId: command.PersonId.Value),
            AddIdentityDocumentCommand command => new AuthorizationContext(PropertyCapabilityOperationNames.AddIdentityDocument, PersonId: command.PersonId.Value),
            Masterdom.Modules.People.Application.Commands.AddRelationshipCommand command => new AuthorizationContext(PropertyCapabilityOperationNames.AddRelationship, PersonId: command.PersonId.Value),
            GetPersonByIdQuery query => new AuthorizationContext(PropertyCapabilityOperationNames.GetPersonById, PersonId: query.PersonId.Value),
            GetPersonByNumberQuery => new AuthorizationContext(PropertyCapabilityOperationNames.GetPersonByNumber),
            SearchPeopleQuery => new AuthorizationContext(PropertyCapabilityOperationNames.SearchPeople),

            CreateLeaseCommand command => new AuthorizationContext(PropertyCapabilityOperationNames.CreateLease, command.Property.PropertyId),
            ActivateLeaseCommand command => new AuthorizationContext(PropertyCapabilityOperationNames.ActivateLease, ResolveLeasePropertyId(command.LeaseId.Value)),
            RenewLeaseCommand command => new AuthorizationContext(PropertyCapabilityOperationNames.RenewLease, ResolveLeasePropertyId(command.LeaseId.Value)),
            TerminateLeaseCommand command => new AuthorizationContext(PropertyCapabilityOperationNames.TerminateLease, ResolveLeasePropertyId(command.LeaseId.Value)),
            ExpireLeaseCommand command => new AuthorizationContext(PropertyCapabilityOperationNames.ExpireLease, ResolveLeasePropertyId(command.LeaseId.Value)),
            CloseLeaseCommand command => new AuthorizationContext(PropertyCapabilityOperationNames.CloseLease, ResolveLeasePropertyId(command.LeaseId.Value)),
            ChangeCommercialTermsCommand command => new AuthorizationContext(PropertyCapabilityOperationNames.ChangeCommercialTerms, ResolveLeasePropertyId(command.LeaseId.Value)),
            GetLeaseByIdQuery query => new AuthorizationContext(PropertyCapabilityOperationNames.GetLeaseById, ResolveLeasePropertyId(query.LeaseId.Value)),
            GetLeaseByNumberQuery query => new AuthorizationContext(PropertyCapabilityOperationNames.GetLeaseByNumber, ResolveLeasePropertyId(query.Number.Value)),

            CreateTenancyCommand command => new AuthorizationContext(PropertyCapabilityOperationNames.CreateTenancy, command.Property.PropertyId),
            AddOccupantCommand command => new AuthorizationContext(PropertyCapabilityOperationNames.AddOccupant, ResolveTenancyPropertyId(command.TenancyId.Value)),
            RemoveOccupantCommand command => new AuthorizationContext(PropertyCapabilityOperationNames.RemoveOccupant, ResolveTenancyPropertyId(command.TenancyId.Value)),
            RecordMoveInCommand command => new AuthorizationContext(PropertyCapabilityOperationNames.RecordMoveIn, ResolveTenancyPropertyId(command.TenancyId.Value)),
            RecordMoveOutCommand command => new AuthorizationContext(PropertyCapabilityOperationNames.RecordMoveOut, ResolveTenancyPropertyId(command.TenancyId.Value)),
            CloseTenancyCommand command => new AuthorizationContext(PropertyCapabilityOperationNames.CloseTenancy, ResolveTenancyPropertyId(command.TenancyId.Value)),
            ArchiveTenancyCommand command => new AuthorizationContext(PropertyCapabilityOperationNames.ArchiveTenancy, ResolveTenancyPropertyId(command.TenancyId.Value)),
            UpdateTenancyNotesCommand command => new AuthorizationContext(PropertyCapabilityOperationNames.UpdateTenancyNotes, ResolveTenancyPropertyId(command.TenancyId.Value)),
            GetTenancyByIdQuery query => new AuthorizationContext(PropertyCapabilityOperationNames.GetTenancyById, ResolveTenancyPropertyId(query.TenancyId.Value)),

            InstallMeterCommand command => new AuthorizationContext(PropertyCapabilityOperationNames.InstallMeter, command.MeterLocationReference.PropertyId),
            SubmitReadingCommand command => new AuthorizationContext(PropertyCapabilityOperationNames.SubmitReading, ResolveMeterPropertyId(command.MeterId.Value)),
            ApproveReadingCommand command => new AuthorizationContext(PropertyCapabilityOperationNames.ApproveReading, ResolveMeterPropertyId(command.MeterId.Value)),
            CorrectReadingCommand command => new AuthorizationContext(PropertyCapabilityOperationNames.CorrectReading, ResolveMeterPropertyId(command.MeterId.Value)),
            RetireMeterCommand command => new AuthorizationContext(PropertyCapabilityOperationNames.RetireMeter, ResolveMeterPropertyId(command.MeterId.Value)),
            GetMeterByIdQuery query => new AuthorizationContext(PropertyCapabilityOperationNames.GetMeterById, ResolveMeterPropertyId(query.MeterId.Value)),
            GetMeterByNumberQuery query => new AuthorizationContext(PropertyCapabilityOperationNames.GetMeterByNumber, ResolveMeterPropertyId(query.MeterNumber.Value)),

            CreateMaintenanceTicketCommand command => new AuthorizationContext(PropertyCapabilityOperationNames.CreateMaintenanceTicket, command.PropertyId),
            AssignMaintenanceTicketCommand command => new AuthorizationContext(PropertyCapabilityOperationNames.AssignMaintenanceTicket, ResolveMaintenanceTicketPropertyId(command.MaintenanceTicketId.Value)),
            GetMaintenanceTicketByIdQuery query => new AuthorizationContext(PropertyCapabilityOperationNames.GetMaintenanceTicketById, ResolveMaintenanceTicketPropertyId(query.MaintenanceTicketId.Value)),

            CreateInventoryItemCommand command => new AuthorizationContext(PropertyCapabilityOperationNames.CreateInventoryItem, command.PropertyId),

            GenerateBillCommand command => new AuthorizationContext(PropertyCapabilityOperationNames.GenerateBill, command.PropertyReference.PropertyId),
            FinalizeBillCommand command => new AuthorizationContext(PropertyCapabilityOperationNames.FinalizeBill, ResolveBillPropertyId(command.BillId.Value)),
            AddAdjustmentCommand command => new AuthorizationContext(PropertyCapabilityOperationNames.AddAdjustment, ResolveBillPropertyId(command.BillId.Value)),
            ApplyCreditCommand command => new AuthorizationContext(PropertyCapabilityOperationNames.ApplyCredit, ResolveBillPropertyId(command.BillId.Value)),
            VoidBillCommand command => new AuthorizationContext(PropertyCapabilityOperationNames.VoidBill, ResolveBillPropertyId(command.BillId.Value)),
            GetBillByIdQuery query => new AuthorizationContext(PropertyCapabilityOperationNames.GetBillById, ResolveBillPropertyId(query.BillId.Value)),
            GetBillByNumberQuery query => new AuthorizationContext(PropertyCapabilityOperationNames.GetBillByNumber, ResolveBillPropertyId(query.BillNumber.Value)),

            OpenLedgerCommand => new AuthorizationContext(PropertyCapabilityOperationNames.OpenLedger),
            PostBillingJournalCommand => new AuthorizationContext(PropertyCapabilityOperationNames.PostBillingJournal),
            PostPaymentJournalCommand => new AuthorizationContext(PropertyCapabilityOperationNames.PostPaymentJournal),
            ReverseJournalCommand => new AuthorizationContext(PropertyCapabilityOperationNames.ReverseJournal),
            CompletePostingBatchCommand => new AuthorizationContext(PropertyCapabilityOperationNames.CompletePostingBatch),
            GetLedgerByIdQuery => new AuthorizationContext(PropertyCapabilityOperationNames.GetLedgerById),
            GetLedgerByCodeQuery => new AuthorizationContext(PropertyCapabilityOperationNames.GetLedgerByCode),

            ReceivePaymentCommand => new AuthorizationContext(PropertyCapabilityOperationNames.ReceivePayment),
            AllocatePaymentCommand => new AuthorizationContext(PropertyCapabilityOperationNames.AllocatePayment),
            ReversePaymentCommand => new AuthorizationContext(PropertyCapabilityOperationNames.ReversePayment),
            VoidPaymentCommand => new AuthorizationContext(PropertyCapabilityOperationNames.VoidPayment),
            GetPaymentByIdQuery => new AuthorizationContext(PropertyCapabilityOperationNames.GetPaymentById),
            GetPaymentByReferenceQuery => new AuthorizationContext(PropertyCapabilityOperationNames.GetPaymentByReference),

            GenerateReportQuery => new AuthorizationContext(PropertyCapabilityOperationNames.GenerateReport),

            GenerateNotificationCommand command => new AuthorizationContext(PropertyCapabilityOperationNames.GenerateNotification, PersonId: command.RecipientId),
            GetNotificationHistoryQuery query => new AuthorizationContext(PropertyCapabilityOperationNames.GetNotificationHistory, PersonId: query.RecipientId),

            GenerateDocumentCommand command => new AuthorizationContext(PropertyCapabilityOperationNames.GenerateDocument, PersonId: command.RequestedBy),
            PreviewDocumentQuery query => new AuthorizationContext(PropertyCapabilityOperationNames.PreviewDocument, PersonId: query.RequestedBy),
            DownloadDocumentQuery => new AuthorizationContext(PropertyCapabilityOperationNames.DownloadDocument),
            RegenerateDocumentCommand command => new AuthorizationContext(PropertyCapabilityOperationNames.RegenerateDocument, PersonId: command.RequestedBy),
            GetDocumentHistoryQuery => new AuthorizationContext(PropertyCapabilityOperationNames.GetDocumentHistory),

            _ => throw new InvalidOperationException($"No request authorization mapping exists for '{request.GetType().FullName}'.")
        };

        return _authorizationService.Authorize(context);
    }

    private Guid? ResolvePropertyId(string propertyCode)
    {
        return _dbContext.Properties
            .AsNoTracking()
            .Where(x => x.Code.Value == propertyCode)
            .Select(x => (Guid?)x.Id.Value)
            .FirstOrDefault();
    }

    private Guid? ResolveLeasePropertyId(Guid leaseId)
    {
        return _dbContext.Leases
            .AsNoTracking()
            .Where(x => x.Id.Value == leaseId)
            .Select(x => (Guid?)x.Property.PropertyId)
            .FirstOrDefault();
    }

    private Guid? ResolveLeasePropertyId(string leaseNumber)
    {
        return _dbContext.Leases
            .AsNoTracking()
            .Where(x => x.Number.Value == leaseNumber)
            .Select(x => (Guid?)x.Property.PropertyId)
            .FirstOrDefault();
    }

    private Guid? ResolveTenancyPropertyId(Guid tenancyId)
    {
        return _dbContext.Tenancies
            .AsNoTracking()
            .Where(x => x.Id.Value == tenancyId)
            .Select(x => (Guid?)x.Property.PropertyId)
            .FirstOrDefault();
    }

    private Guid? ResolveMeterPropertyId(Guid meterId)
    {
        return _dbContext.Meters
            .AsNoTracking()
            .Where(x => x.Id.Value == meterId)
            .Select(x => (Guid?)x.MeterLocationReference.PropertyId)
            .FirstOrDefault();
    }

    private Guid? ResolveMeterPropertyId(string meterNumber)
    {
        return _dbContext.Meters
            .AsNoTracking()
            .Where(x => x.MeterNumber.Value == meterNumber)
            .Select(x => (Guid?)x.MeterLocationReference.PropertyId)
            .FirstOrDefault();
    }

    private Guid? ResolveBillPropertyId(Guid billId)
    {
        return _dbContext.Bills
            .AsNoTracking()
            .Where(x => x.Id.Value == billId)
            .Select(x => (Guid?)x.PropertyReference.PropertyId)
            .FirstOrDefault();
    }

    private Guid? ResolveBillPropertyId(string billNumber)
    {
        return _dbContext.Bills
            .AsNoTracking()
            .Where(x => x.BillNumber.Value == billNumber)
            .Select(x => (Guid?)x.PropertyReference.PropertyId)
            .FirstOrDefault();
    }

    private Guid? ResolveMaintenanceTicketPropertyId(Guid maintenanceTicketId)
    {
        return _dbContext.MaintenanceTickets
            .AsNoTracking()
            .Where(x => x.Id.Value == maintenanceTicketId)
            .Select(x => (Guid?)x.PropertyId)
            .FirstOrDefault();
    }
}
