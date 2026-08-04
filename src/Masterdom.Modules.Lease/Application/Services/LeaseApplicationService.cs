using Masterdom.Modules.Lease.Application.Commands;
using Masterdom.Modules.Lease.Application.Queries;
using Masterdom.Modules.Lease.Application.Support;
using Masterdom.Modules.Lease.Domain.Repositories;
using LeaseAggregate = Masterdom.Modules.Lease.Domain.Entities.Lease.Lease;

namespace Masterdom.Modules.Lease.Application.Services;

public sealed class LeaseApplicationService : ILeaseApplicationService
{
    private readonly ILeaseRepository _repository;
    private readonly ILeaseUnitOfWork _unitOfWork;
    private readonly ILeasePlatformOrchestrator _platformOrchestrator;

    public LeaseApplicationService(
        ILeaseRepository repository,
        ILeaseUnitOfWork unitOfWork,
        ILeasePlatformOrchestrator platformOrchestrator)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _platformOrchestrator = platformOrchestrator ?? throw new ArgumentNullException(nameof(platformOrchestrator));
    }

    public LeaseAggregate CreateLease(CreateLeaseCommand command)
    {
        ArgumentNullException.ThrowIfNull(command);

        if (_repository.GetByNumber(command.Number) is not null)
        {
            throw new InvalidOperationException($"Lease number '{command.Number.Value}' already exists.");
        }

        if (_repository.HasActiveLeaseForTenancy(command.Tenancy))
        {
            throw new InvalidOperationException($"Tenancy '{command.Tenancy.TenancyId}' already has an active lease.");
        }

        var lease = LeaseAggregate.Create(
            command.Number,
            command.Type,
            command.Tenancy,
            command.Property,
            command.Unit,
            command.Person,
            command.EffectivePeriod,
            command.CommercialTerms,
            command.LeaseClauses);

        _unitOfWork.Execute(() =>
        {
            _repository.Add(lease);
        });

        _platformOrchestrator.OnLeaseMutated(lease, "CreateLease");

        return lease;
    }

    public LeaseAggregate ActivateLease(ActivateLeaseCommand command)
    {
        ArgumentNullException.ThrowIfNull(command);

        var lease = GetRequiredLease(command.LeaseId);
        lease.Activate();

        PersistAndCoordinate(lease, "ActivateLease");
        return lease;
    }

    public LeaseAggregate RenewLease(RenewLeaseCommand command)
    {
        ArgumentNullException.ThrowIfNull(command);

        var lease = GetRequiredLease(command.LeaseId);
        lease.Renew(command.RenewalDate, command.EffectivePeriod, command.CommercialTerms, command.LeaseClauses);

        PersistAndCoordinate(lease, "RenewLease");
        return lease;
    }

    public LeaseAggregate TerminateLease(TerminateLeaseCommand command)
    {
        ArgumentNullException.ThrowIfNull(command);

        var lease = GetRequiredLease(command.LeaseId);
        lease.Terminate(command.Reason);

        PersistAndCoordinate(lease, "TerminateLease");
        return lease;
    }

    public LeaseAggregate ExpireLease(ExpireLeaseCommand command)
    {
        ArgumentNullException.ThrowIfNull(command);

        var lease = GetRequiredLease(command.LeaseId);
        lease.Expire();

        PersistAndCoordinate(lease, "ExpireLease");
        return lease;
    }

    public LeaseAggregate ChangeCommercialTerms(ChangeCommercialTermsCommand command)
    {
        ArgumentNullException.ThrowIfNull(command);

        var lease = GetRequiredLease(command.LeaseId);
        lease.ChangeCommercialTerms(command.CommercialTerms, command.EffectivePeriod);

        PersistAndCoordinate(lease, "ChangeCommercialTerms");
        return lease;
    }

    public LeaseAggregate CloseLease(CloseLeaseCommand command)
    {
        ArgumentNullException.ThrowIfNull(command);

        var lease = GetRequiredLease(command.LeaseId);
        lease.Close();

        PersistAndCoordinate(lease, "CloseLease");
        return lease;
    }

    public LeaseAggregate? GetLease(GetLeaseByIdQuery query)
    {
        ArgumentNullException.ThrowIfNull(query);
        return _repository.GetById(query.LeaseId);
    }

    public LeaseAggregate? GetLeaseByNumber(GetLeaseByNumberQuery query)
    {
        ArgumentNullException.ThrowIfNull(query);
        return _repository.GetByNumber(query.Number);
    }

    private LeaseAggregate GetRequiredLease(Masterdom.Modules.Lease.Domain.Entities.Lease.LeaseId leaseId)
    {
        var lease = _repository.GetById(leaseId);
        if (lease is null)
        {
            throw new InvalidOperationException($"Lease '{leaseId}' was not found.");
        }

        return lease;
    }

    private void PersistAndCoordinate(LeaseAggregate lease, string operationName)
    {
        _unitOfWork.Execute(() =>
        {
            _repository.Update(lease);
        });

        _platformOrchestrator.OnLeaseMutated(lease, operationName);
    }
}
