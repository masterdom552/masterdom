using Masterdom.Modules.Billing.Application.Commands;
using Masterdom.Modules.Billing.Application.Queries;
using Masterdom.Modules.Billing.Application.Support;
using Masterdom.Modules.Billing.Domain.Entities.Billing;
using Masterdom.Modules.Billing.Domain.Repositories;
using BillAggregate = Masterdom.Modules.Billing.Domain.Entities.Billing.Bill;

namespace Masterdom.Modules.Billing.Application.Services;

public sealed class BillingApplicationService : IBillingApplicationService
{
    private readonly IBillRepository _repository;
    private readonly IBillingUnitOfWork _unitOfWork;
    private readonly IBillingPlatformOrchestrator _platformOrchestrator;

    public BillingApplicationService(
        IBillRepository repository,
        IBillingUnitOfWork unitOfWork,
        IBillingPlatformOrchestrator platformOrchestrator)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _platformOrchestrator = platformOrchestrator ?? throw new ArgumentNullException(nameof(platformOrchestrator));
    }

    public BillAggregate GenerateBill(GenerateBillCommand command)
    {
        ArgumentNullException.ThrowIfNull(command);

        if (_repository.GetByNumber(command.BillNumber) is not null)
        {
            throw new InvalidOperationException($"Bill number '{command.BillNumber.Value}' already exists.");
        }

        var bill = BillAggregate.Generate(
            BillId.New(),
            command.BillNumber,
            command.TenancyReference,
            command.LeaseReference,
            command.PropertyReference,
            command.BilledParty,
            command.BillingPeriod,
            command.BillingCycle,
            command.GeneratedDate,
            command.IssueDate,
            command.DueDate,
            command.Currency,
            command.Charges);

        _unitOfWork.Execute(() =>
        {
            _repository.Add(bill);
        });

        _platformOrchestrator.OnBillMutated(bill, "GenerateBill");

        return bill;
    }

    public BillAggregate FinalizeBill(FinalizeBillCommand command)
    {
        ArgumentNullException.ThrowIfNull(command);

        var bill = GetRequiredBill(command.BillId);
        bill.FinalizeBill();

        PersistAndCoordinate(bill, "FinalizeBill");
        return bill;
    }

    public BillAggregate AddAdjustment(AddAdjustmentCommand command)
    {
        ArgumentNullException.ThrowIfNull(command);

        var bill = GetRequiredBill(command.BillId);
        bill.AddAdjustment(
            command.Adjustment,
            command.GeneratedDate,
            command.IssueDate,
            command.DueDate);

        PersistAndCoordinate(bill, "AddAdjustment");
        return bill;
    }

    public BillAggregate ApplyCredit(ApplyCreditCommand command)
    {
        ArgumentNullException.ThrowIfNull(command);

        var bill = GetRequiredBill(command.BillId);
        bill.ApplyCredit(
            command.Credit,
            command.GeneratedDate,
            command.IssueDate,
            command.DueDate);

        PersistAndCoordinate(bill, "ApplyCredit");
        return bill;
    }

    public BillAggregate VoidBill(VoidBillCommand command)
    {
        ArgumentNullException.ThrowIfNull(command);

        var bill = GetRequiredBill(command.BillId);
        bill.Void(command.Reason);

        PersistAndCoordinate(bill, "VoidBill");
        return bill;
    }

    public BillAggregate? GetBill(GetBillByIdQuery query)
    {
        ArgumentNullException.ThrowIfNull(query);
        return _repository.GetById(query.BillId);
    }

    public BillAggregate? GetBillByNumber(GetBillByNumberQuery query)
    {
        ArgumentNullException.ThrowIfNull(query);
        return _repository.GetByNumber(query.BillNumber);
    }

    private BillAggregate GetRequiredBill(BillId billId)
    {
        var bill = _repository.GetById(billId);
        if (bill is null)
        {
            throw new InvalidOperationException($"Bill '{billId}' was not found.");
        }

        return bill;
    }

    private void PersistAndCoordinate(BillAggregate bill, string operationName)
    {
        _unitOfWork.Execute(() =>
        {
            _repository.Update(bill);
        });

        _platformOrchestrator.OnBillMutated(bill, operationName);
    }
}
