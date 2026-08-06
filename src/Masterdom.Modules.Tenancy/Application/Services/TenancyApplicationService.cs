using Masterdom.Modules.Tenancy.Application.Commands;
using Masterdom.Modules.Tenancy.Application.Queries;
using Masterdom.Modules.Tenancy.Application.Support;
using Masterdom.Modules.Tenancy.Domain.Entities.Tenancy;
using Masterdom.Modules.Tenancy.Domain.Repositories;
using TenancyAggregate = Masterdom.Modules.Tenancy.Domain.Entities.Tenancy.Tenancy;

namespace Masterdom.Modules.Tenancy.Application.Services;

/// <summary>
/// Orchestrates tenancy use-cases through aggregate APIs.
/// </summary>
public sealed class TenancyApplicationService : ITenancyApplicationService
{
    private readonly ITenancyRepository _repository;
    private readonly ITenancyUnitOfWork _unitOfWork;
    private readonly ITenancyPlatformOrchestrator _platformOrchestrator;

    public TenancyApplicationService(
        ITenancyRepository repository,
        ITenancyUnitOfWork unitOfWork,
        ITenancyPlatformOrchestrator platformOrchestrator)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _platformOrchestrator = platformOrchestrator ?? throw new ArgumentNullException(nameof(platformOrchestrator));
    }

    public TenancyAggregate CreateTenancy(CreateTenancyCommand command)
    {
        ArgumentNullException.ThrowIfNull(command);

        if (_repository.HasActiveTenancyForUnit(command.Unit))
        {
            throw new InvalidOperationException(
                $"Unit '{command.Unit.UnitId}' already has an active tenancy.");
        }

        var tenancy = TenancyAggregate.Create(
            command.Number,
            command.Property,
            command.Unit,
            command.MoveInDate,
            OccupantReference.Create(command.PrimaryOccupantPersonId, true),
            command.Notes);

        _unitOfWork.Execute(() =>
        {
            _repository.Add(tenancy);
        });

        _platformOrchestrator.OnTenancyMutated(tenancy, "CreateTenancy");

        return tenancy;
    }

    public TenancyAggregate AddOccupant(AddOccupantCommand command)
    {
        ArgumentNullException.ThrowIfNull(command);

        var tenancy = GetRequiredTenancy(command.TenancyId);
        tenancy.AddOccupant(command.PersonId, command.IsPrimary);

        PersistAndCoordinate(tenancy, "AddOccupant");
        return tenancy;
    }

    public bool RemoveOccupant(RemoveOccupantCommand command)
    {
        ArgumentNullException.ThrowIfNull(command);

        var tenancy = GetRequiredTenancy(command.TenancyId);

        tenancy.RemoveOccupant(command.PersonId);
        PersistAndCoordinate(tenancy, "RemoveOccupant");

        return true;
    }

    public TenancyAggregate RecordMoveIn(RecordMoveInCommand command)
    {
        ArgumentNullException.ThrowIfNull(command);

        var tenancy = GetRequiredTenancy(command.TenancyId);
        tenancy.RecordMoveIn(command.MoveInDate);

        PersistAndCoordinate(tenancy, "RecordMoveIn");
        return tenancy;
    }

    public TenancyAggregate RecordMoveOut(RecordMoveOutCommand command)
    {
        ArgumentNullException.ThrowIfNull(command);

        var tenancy = GetRequiredTenancy(command.TenancyId);
        tenancy.RecordMoveOut(command.MoveOutDate);

        PersistAndCoordinate(tenancy, "RecordMoveOut");
        return tenancy;
    }

    public TenancyAggregate CloseTenancy(CloseTenancyCommand command)
    {
        ArgumentNullException.ThrowIfNull(command);

        var tenancy = GetRequiredTenancy(command.TenancyId);
        tenancy.Close(command.ClosedOn, command.Reason);

        PersistAndCoordinate(tenancy, "CloseTenancy");
        return tenancy;
    }

    public TenancyAggregate ArchiveTenancy(ArchiveTenancyCommand command)
    {
        ArgumentNullException.ThrowIfNull(command);

        var tenancy = GetRequiredTenancy(command.TenancyId);
        tenancy.Archive();

        PersistAndCoordinate(tenancy, "ArchiveTenancy");
        return tenancy;
    }

    public TenancyAggregate UpdateNotes(UpdateTenancyNotesCommand command)
    {
        ArgumentNullException.ThrowIfNull(command);

        var tenancy = GetRequiredTenancy(command.TenancyId);
        tenancy.UpdateNotes(command.Notes);

        PersistAndCoordinate(tenancy, "UpdateNotes");
        return tenancy;
    }

    public TenancyAggregate? GetTenancy(GetTenancyByIdQuery query)
    {
        ArgumentNullException.ThrowIfNull(query);
        return _repository.GetById(query.TenancyId);
    }

    private TenancyAggregate GetRequiredTenancy(TenancyId tenancyId)
    {
        var tenancy = _repository.GetById(tenancyId);
        if (tenancy is null)
        {
            throw new InvalidOperationException($"Tenancy '{tenancyId}' was not found.");
        }

        return tenancy;
    }

    private void PersistAndCoordinate(TenancyAggregate tenancy, string operationName)
    {
        _unitOfWork.Execute(() =>
        {
            _repository.Update(tenancy);
        });

        _platformOrchestrator.OnTenancyMutated(tenancy, operationName);
    }
}
