using Masterdom.Modules.CRM.Application.Commands;
using Masterdom.Modules.CRM.Application.Handlers.Commands;
using Masterdom.Modules.CRM.Application.Handlers.Queries;
using Masterdom.Modules.CRM.Application.Queries;
using Masterdom.Modules.CRM.Application.Services;
using Masterdom.Modules.CRM.Application.Support;
using Masterdom.Modules.CRM.Domain.Entities.Party;
using Masterdom.Modules.CRM.Domain.Repositories;

namespace Masterdom.Core.Tests.CRM;

public sealed class PartyRoleApplicationHandlersTests
{
    [Fact]
    public void AssignPartyRoleHandler_ShouldAddRoleThroughAggregateBehavior()
    {
        var aggregate = CreateParty();
        var repository = new InMemoryPartyRepository(aggregate);
        var unitOfWork = new SpyUnitOfWork();
        var orchestrator = new SpyPlatformOrchestrator();

        var service = new PartyApplicationService(repository, unitOfWork, orchestrator);
        var handler = new AssignPartyRoleCommandHandler(service);

        var result = handler.Handle(new AssignPartyRoleCommand(
            aggregate.Id,
            PartyRoleType.Tenant,
            DateTime.UtcNow,
            EffectiveFromUtc: null,
            EffectiveToUtc: null,
            AssignmentReason: "Signed lease",
            UpdatedBy: "operator"));

        Assert.True(result.IsSuccess);
        Assert.Single(aggregate.RoleAssignments);
        Assert.Equal(1, unitOfWork.ExecuteCount);
        Assert.Equal(1, orchestrator.MutationCount);
    }

    [Fact]
    public void ReactivatePartyRoleHandler_ShouldRespectExpiredRoleRule()
    {
        var aggregate = CreateParty();
        var assignedAtUtc = DateTime.UtcNow;
        var assignment = aggregate.AssignRole(
            PartyRoleType.Contractor,
            assignedAtUtc,
            effectiveToUtc: assignedAtUtc.AddHours(4),
            assignmentReason: "One-off repair");

        aggregate.DeactivateRole(assignment.Id, assignedAtUtc.AddHours(1), "Paused");

        var repository = new InMemoryPartyRepository(aggregate);
        var service = new PartyApplicationService(repository, new SpyUnitOfWork(), new SpyPlatformOrchestrator());
        var handler = new ReactivatePartyRoleCommandHandler(service);

        var result = handler.Handle(new ReactivatePartyRoleCommand(
            aggregate.Id,
            assignment.Id,
            assignedAtUtc.AddDays(1),
            "Resume",
            UpdatedBy: "operator"));

        Assert.False(result.IsSuccess);
        Assert.Equal("domain_rule_violation", result.ErrorCode);
    }

    [Fact]
    public void SearchPartiesByRoleQueryHandler_ShouldReturnPartiesWithEffectiveRole()
    {
        var asOfUtc = DateTime.UtcNow;

        var tenant = Party.Create("Tenant Party", null, PartyType.Person, asOfUtc, createdBy: "tester");
        tenant.AssignRole(PartyRoleType.Tenant, asOfUtc, assignmentReason: "Current tenancy");

        var vendor = Party.Create("Vendor Party", null, PartyType.Organization, asOfUtc, createdBy: "tester");
        vendor.AssignRole(PartyRoleType.Vendor, asOfUtc, assignmentReason: "Service provider");

        var repository = new InMemoryPartyRepository(tenant, vendor);
        var service = new PartyApplicationService(repository, new SpyUnitOfWork(), new SpyPlatformOrchestrator());
        var handler = new SearchPartiesByRoleQueryHandler(service);

        var result = handler.Handle(new SearchPartiesByRoleQuery(PartyRoleType.Tenant, asOfUtc, 50));

        Assert.True(result.IsSuccess);
        Assert.Single(result.Value!);
        Assert.Equal("Tenant Party", result.Value!.Single().DisplayName);
    }

    private static Party CreateParty()
    {
        return Party.Create(
            "Role Test Party",
            null,
            PartyType.Person,
            DateTime.UtcNow,
            createdBy: "tester");
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
