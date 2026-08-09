using Masterdom.Modules.CRM.Application.Commands;
using Masterdom.Modules.CRM.Application.Queries;
using Masterdom.Modules.CRM.Application.Support;
using Masterdom.Modules.CRM.Domain.Entities.Party;
using Masterdom.Modules.CRM.Domain.Repositories;

namespace Masterdom.Modules.CRM.Application.Services;

/// <summary>
/// Orchestrates CRM party use-cases through aggregate APIs.
/// </summary>
public sealed class PartyApplicationService : IPartyApplicationService
{
    private readonly IPartyRepository _repository;
    private readonly IPartyUnitOfWork _unitOfWork;
    private readonly IPartyPlatformOrchestrator _platformOrchestrator;

    public PartyApplicationService(
        IPartyRepository repository,
        IPartyUnitOfWork unitOfWork,
        IPartyPlatformOrchestrator platformOrchestrator)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _platformOrchestrator = platformOrchestrator ?? throw new ArgumentNullException(nameof(platformOrchestrator));
    }

    public Party CreateParty(CreatePartyCommand command)
    {
        ArgumentNullException.ThrowIfNull(command);

        var party = Party.Create(
            command.DisplayName,
            command.LegalName,
            command.PartyType,
            command.CreatedAtUtc,
            command.CreatedBy);

        _unitOfWork.Execute(() => _repository.Add(party));
        _platformOrchestrator.OnPartyMutated(party, nameof(CreateParty));

        return party;
    }

    public Party UpdateParty(UpdatePartyCommand command)
    {
        ArgumentNullException.ThrowIfNull(command);

        var party = GetRequiredParty(command.PartyId);
        party.Update(
            command.DisplayName,
            command.LegalName,
            command.PartyType,
            command.UpdatedAtUtc,
            command.UpdatedBy);

        PersistAndCoordinate(party, nameof(UpdateParty));
        return party;
    }

    public Party DeactivateParty(DeactivatePartyCommand command)
    {
        ArgumentNullException.ThrowIfNull(command);

        var party = GetRequiredParty(command.PartyId);
        party.Deactivate(command.UpdatedAtUtc, command.UpdatedBy);

        PersistAndCoordinate(party, nameof(DeactivateParty));
        return party;
    }

    public Party AddContactMethod(AddContactMethodCommand command)
    {
        ArgumentNullException.ThrowIfNull(command);

        var party = GetRequiredParty(command.PartyId);
        party.AddContactMethod(command.ContactMethod, command.UpdatedAtUtc, command.UpdatedBy);

        PersistAndCoordinate(party, nameof(AddContactMethod));
        return party;
    }

    public bool RemoveContactMethod(RemoveContactMethodCommand command)
    {
        ArgumentNullException.ThrowIfNull(command);

        var party = GetRequiredParty(command.PartyId);
        var removed = party.RemoveContactMethod(command.ContactMethod, command.UpdatedAtUtc, command.UpdatedBy);
        if (!removed)
        {
            return false;
        }

        PersistAndCoordinate(party, nameof(RemoveContactMethod));
        return true;
    }

    public Party AddAddress(AddAddressCommand command)
    {
        ArgumentNullException.ThrowIfNull(command);

        var party = GetRequiredParty(command.PartyId);
        party.AddAddress(command.Address, command.UpdatedAtUtc, command.UpdatedBy);

        PersistAndCoordinate(party, nameof(AddAddress));
        return party;
    }

    public bool RemoveAddress(RemoveAddressCommand command)
    {
        ArgumentNullException.ThrowIfNull(command);

        var party = GetRequiredParty(command.PartyId);
        var removed = party.RemoveAddress(command.Address, command.UpdatedAtUtc, command.UpdatedBy);
        if (!removed)
        {
            return false;
        }

        PersistAndCoordinate(party, nameof(RemoveAddress));
        return true;
    }

    public Party CreateRelationship(CreateRelationshipCommand command)
    {
        ArgumentNullException.ThrowIfNull(command);

        var party = GetRequiredParty(command.PartyId);
        party.AddRelationship(command.Relationship, command.UpdatedAtUtc, command.UpdatedBy);

        PersistAndCoordinate(party, nameof(CreateRelationship));
        return party;
    }

    public bool RemoveRelationship(RemoveRelationshipCommand command)
    {
        ArgumentNullException.ThrowIfNull(command);

        var party = GetRequiredParty(command.PartyId);
        var removed = party.RemoveRelationship(command.Relationship, command.UpdatedAtUtc, command.UpdatedBy);
        if (!removed)
        {
            return false;
        }

        PersistAndCoordinate(party, nameof(RemoveRelationship));
        return true;
    }

    public Party AssignPartyRole(AssignPartyRoleCommand command)
    {
        ArgumentNullException.ThrowIfNull(command);

        var party = GetRequiredParty(command.PartyId);
        party.AssignRole(
            command.RoleType,
            command.AssignedAtUtc,
            command.EffectiveFromUtc,
            command.EffectiveToUtc,
            command.AssignmentReason,
            command.UpdatedBy);

        PersistAndCoordinate(party, nameof(AssignPartyRole));
        return party;
    }

    public bool RemovePartyRole(RemovePartyRoleCommand command)
    {
        ArgumentNullException.ThrowIfNull(command);

        var party = GetRequiredParty(command.PartyId);
        var removed = party.RemoveRole(command.RoleAssignmentId, command.RemovedAtUtc, command.Reason, command.UpdatedBy);
        if (!removed)
        {
            return false;
        }

        PersistAndCoordinate(party, nameof(RemovePartyRole));
        return true;
    }

    public bool DeactivatePartyRole(DeactivatePartyRoleCommand command)
    {
        ArgumentNullException.ThrowIfNull(command);

        var party = GetRequiredParty(command.PartyId);
        var deactivated = party.DeactivateRole(command.RoleAssignmentId, command.DeactivatedAtUtc, command.Reason, command.UpdatedBy);
        if (!deactivated)
        {
            return false;
        }

        PersistAndCoordinate(party, nameof(DeactivatePartyRole));
        return true;
    }

    public bool ReactivatePartyRole(ReactivatePartyRoleCommand command)
    {
        ArgumentNullException.ThrowIfNull(command);

        var party = GetRequiredParty(command.PartyId);
        var reactivated = party.ReactivateRole(command.RoleAssignmentId, command.ReactivatedAtUtc, command.Reason, command.UpdatedBy);
        if (!reactivated)
        {
            return false;
        }

        PersistAndCoordinate(party, nameof(ReactivatePartyRole));
        return true;
    }

    public Party? GetParty(GetPartyByIdQuery query)
    {
        ArgumentNullException.ThrowIfNull(query);
        return _repository.GetById(query.PartyId);
    }

    public IReadOnlyCollection<PartyRoleAssignment> GetPartyRoles(GetPartyRolesQuery query)
    {
        ArgumentNullException.ThrowIfNull(query);

        var party = GetRequiredParty(query.PartyId);
        return party.GetRoles();
    }

    public IReadOnlyCollection<Party> SearchParties(SearchPartiesQuery query)
    {
        ArgumentNullException.ThrowIfNull(query);
        return _repository.Search(query.DisplayNameContains, query.PartyType, query.Take);
    }

    public IReadOnlyCollection<Party> SearchPartiesByRole(SearchPartiesByRoleQuery query)
    {
        ArgumentNullException.ThrowIfNull(query);
        return _repository.SearchByRole(query.RoleType, query.AsOfUtc, query.Take);
    }

    private Party GetRequiredParty(PartyId partyId)
    {
        var party = _repository.GetById(partyId);
        if (party is null)
        {
            throw new InvalidOperationException($"Party '{partyId}' was not found.");
        }

        return party;
    }

    private void PersistAndCoordinate(Party party, string operationName)
    {
        _unitOfWork.Execute(() => _repository.Update(party));
        _platformOrchestrator.OnPartyMutated(party, operationName);
    }
}
