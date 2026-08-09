using Masterdom.Modules.CRM.Application.Commands;
using Masterdom.Modules.CRM.Application.Handlers.Commands;
using Masterdom.Modules.CRM.Application.Services;
using Masterdom.Modules.CRM.Application.Support;
using Masterdom.Modules.CRM.Domain.Entities.Party;
using Masterdom.Modules.CRM.Domain.Repositories;

namespace Masterdom.Core.Tests.CRM;

public sealed class PartyApplicationHandlersTests
{
    [Fact]
    public void UpdatePartyHandler_ShouldUpdateThroughAggregateBehavior()
    {
        var aggregate = Party.Create(
            "Original Name",
            null,
            PartyType.Person,
            DateTime.UtcNow,
            createdBy: "tester");

        var repository = new InMemoryPartyRepository(aggregate);
        var unitOfWork = new SpyUnitOfWork();
        var orchestrator = new SpyPlatformOrchestrator();

        var service = new PartyApplicationService(repository, unitOfWork, orchestrator);
        var handler = new UpdatePartyCommandHandler(service);

        var result = handler.Handle(
            new UpdatePartyCommand(
                aggregate.Id,
                "Updated Name",
                "Updated Legal Name",
                PartyType.Organization,
                DateTime.UtcNow,
                UpdatedBy: "operator"));

        Assert.True(result.IsSuccess);
        Assert.Equal("Updated Name", aggregate.DisplayName);
        Assert.Equal("Updated Legal Name", aggregate.LegalName);
        Assert.Equal(PartyType.Organization, aggregate.PartyType);
        Assert.Equal(1, unitOfWork.ExecuteCount);
        Assert.Equal(1, orchestrator.MutationCount);
    }

    [Fact]
    public void CreateRelationshipHandler_ShouldRespectDuplicatePreventionRule()
    {
        var aggregate = Party.Create(
            "Relationship Holder",
            null,
            PartyType.Organization,
            DateTime.UtcNow,
            createdBy: "tester");

        var relatedPartyId = PartyId.New();
        aggregate.AddRelationship(Relationship.Create(relatedPartyId, RelationshipType.TenantOf), DateTime.UtcNow);

        var repository = new InMemoryPartyRepository(aggregate);
        var service = new PartyApplicationService(repository, new SpyUnitOfWork(), new SpyPlatformOrchestrator());
        var handler = new CreateRelationshipCommandHandler(service);

        var result = handler.Handle(
            new CreateRelationshipCommand(
                aggregate.Id,
                Relationship.Create(relatedPartyId, RelationshipType.TenantOf),
                DateTime.UtcNow,
                UpdatedBy: "operator"));

        Assert.False(result.IsSuccess);
        Assert.Equal("domain_rule_violation", result.ErrorCode);
    }

    private sealed class InMemoryPartyRepository : IPartyRepository
    {
        private readonly Dictionary<Guid, Party> _parties;

        public InMemoryPartyRepository(params Party[] parties)
        {
            _parties = parties.ToDictionary(x => x.Id.Value, x => x);
        }

        public Party? GetById(PartyId id)
        {
            return _parties.TryGetValue(id.Value, out var party) ? party : null;
        }

        public IReadOnlyCollection<Party> Search(string? displayNameContains, PartyType? partyType, int take)
        {
            var effectiveTake = take <= 0 ? 50 : take;
            var query = _parties.Values.AsEnumerable();

            if (!string.IsNullOrWhiteSpace(displayNameContains))
            {
                query = query.Where(x => x.DisplayName.Contains(displayNameContains, StringComparison.OrdinalIgnoreCase));
            }

            if (partyType is not null)
            {
                query = query.Where(x => x.PartyType == partyType);
            }

            return query.Take(effectiveTake).ToList();
        }

        public IReadOnlyCollection<Party> SearchByRole(PartyRoleType roleType, DateTime asOfUtc, int take)
        {
            var effectiveTake = take <= 0 ? 50 : take;

            return _parties.Values
                .Where(x => x.RoleAssignments.Any(role => role.MatchesActiveRoleType(roleType, asOfUtc)))
                .Take(effectiveTake)
                .ToList();
        }

        public void Add(Party party)
        {
            _parties[party.Id.Value] = party;
        }

        public void Update(Party party)
        {
            _parties[party.Id.Value] = party;
        }
    }

    private sealed class SpyUnitOfWork : IPartyUnitOfWork
    {
        public int ExecuteCount { get; private set; }

        public void Execute(Action operation)
        {
            ExecuteCount++;
            operation();
        }
    }

    private sealed class SpyPlatformOrchestrator : IPartyPlatformOrchestrator
    {
        public int MutationCount { get; private set; }

        public void OnPartyMutated(Party party, string operationName)
        {
            MutationCount++;
        }
    }
}
