using Masterdom.Modules.PolicyFramework.Domain.Entities.PolicyFramework;
using Masterdom.Modules.PolicyFramework.Domain.Entities.PolicyFramework.Events;

namespace Masterdom.Core.Tests.PolicyFramework;

public sealed class PolicyFrameworkDomainTests
{
    [Fact]
    public void Create_ShouldCreateInitialDraftVersion_AndRaiseCreatedEvent()
    {
        var policy = CreateDraftPolicy();

        Assert.Equal(1, policy.CurrentVersion.VersionNumber);
        Assert.Equal(PolicyStatus.Draft, policy.Status);
        Assert.Contains(policy.DomainEvents, x => x is PolicyCreatedDomainEvent);
    }

    [Fact]
    public void ActivateVersion_ShouldEnsureOnlyOneActiveVersionPerScope()
    {
        var policy = CreateDraftPolicy();

        policy.CreateVersion(
            PolicyCondition.Create("policy.selector.revision", "module = lease and action = renewal"),
            PolicyMetadata.Create(new Dictionary<string, string>
            {
                ["source"] = "revision"
            }),
            EffectiveDateRange.Create(DateOnly.FromDateTime(DateTime.UtcNow.Date.AddDays(10)), null),
            DateTime.UtcNow);

        policy.ActivateVersion(1, DateTime.UtcNow);
        policy.ActivateVersion(2, DateTime.UtcNow.AddMinutes(1));

        Assert.Single(policy.Versions.Where(x => x.Status == PolicyStatus.Active));
    }

    [Fact]
    public void Expire_ShouldPreserveHistoricalVersion()
    {
        var policy = CreateDraftPolicy();
        policy.ActivateVersion(1, DateTime.UtcNow);

        policy.Expire(DateTime.UtcNow.AddMinutes(1));

        Assert.Equal(PolicyStatus.Expired, policy.Status);
        Assert.Contains(policy.DomainEvents, x => x is PolicyExpiredDomainEvent);
        Assert.Equal(PolicyStatus.Expired, policy.Versions.Single(x => x.VersionNumber == 1).Status);
    }

    [Fact]
    public void Archive_ShouldMakePolicyImmutable()
    {
        var policy = CreateDraftPolicy();
        policy.Archive(DateTime.UtcNow, "Superseded by future policy family");

        var exception = Assert.Throws<InvalidOperationException>(() =>
            policy.CreateVersion(
                PolicyCondition.Create("policy.selector.archived", "should-not-change"),
                PolicyMetadata.Empty(),
                EffectiveDateRange.Create(DateOnly.FromDateTime(DateTime.UtcNow.Date), null),
                DateTime.UtcNow));

        Assert.Equal("Archived policies are immutable.", exception.Message);
        Assert.Contains(policy.DomainEvents, x => x is PolicyArchivedDomainEvent);
    }

    [Fact]
    public void ResolveApplicableVersion_ShouldReturnActiveVersionWithoutExecutingRules()
    {
        var policy = CreateDraftPolicy();
        policy.ActivateVersion(1, DateTime.UtcNow);

        var applicable = policy.ResolveApplicableVersion(
            PolicyScope.Create(PolicyScopeKind.Module, "lease"),
            DateOnly.FromDateTime(DateTime.UtcNow.Date));

        Assert.NotNull(applicable);
        Assert.Equal(1, applicable!.VersionNumber);
    }

    [Fact]
    public void Assign_ShouldRejectOverlappingAssignmentsForSameEntityAndScope()
    {
        var policy = CreateDraftPolicy();

        var assignmentA = PolicyAssignment.Create(
            Guid.CreateVersion7(),
            PolicyScope.Create(PolicyScopeKind.Module, "lease"),
            "Tenant",
            "TENANT-1",
            EffectiveDateRange.Create(DateOnly.FromDateTime(DateTime.UtcNow.Date), null),
            DateTime.UtcNow);

        var assignmentB = PolicyAssignment.Create(
            Guid.CreateVersion7(),
            PolicyScope.Create(PolicyScopeKind.Module, "lease"),
            "Tenant",
            "TENANT-1",
            EffectiveDateRange.Create(DateOnly.FromDateTime(DateTime.UtcNow.Date.AddDays(1)), null),
            DateTime.UtcNow.AddMinutes(1));

        policy.Assign(assignmentA);

        var exception = Assert.Throws<InvalidOperationException>(() => policy.Assign(assignmentB));
        Assert.Equal("Policy assignment overlaps an existing assignment for the same scope and entity.", exception.Message);
    }

    private static Policy CreateDraftPolicy()
    {
        return Policy.Create(
            PolicyId.New(),
            PolicyType.Create("selection"),
            PolicyCategory.Create("platform"),
            PolicyReference.Create("policy.default.selection", "Default Selection Policy"),
            PolicyScope.Create(PolicyScopeKind.Module, "lease"),
            PolicyCondition.Create("policy.selector.default", "module = lease"),
            PolicyMetadata.Create(new Dictionary<string, string>
            {
                ["owner"] = "platform",
                ["visibility"] = "internal"
            }),
            EffectiveDateRange.Create(DateOnly.FromDateTime(DateTime.UtcNow.Date), null),
            DateTime.UtcNow);
    }
}
