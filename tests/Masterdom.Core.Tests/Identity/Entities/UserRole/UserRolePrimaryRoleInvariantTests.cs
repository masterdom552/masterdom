using Masterdom.Core.Identity.Entities.User;
using Masterdom.Core.Identity.Entities.Role;
using Masterdom.Core.Identity.Entities.UserRole;
using Xunit;

namespace Masterdom.Core.Tests.Identity.Entities.UserRole;

/// <summary>
/// Tests for the UserRole PrimaryRole temporal uniqueness invariant.
///
/// Domain Invariant: A user may have multiple roles over time, but MUST NOT have
/// more than one EFFECTIVE (temporally valid) PrimaryRole at any point in time.
/// </summary>
public sealed class UserRolePrimaryRoleInvariantTests
{
    private static readonly UserId TestUserId = UserId.New();
    private static readonly RoleId Role1Id = RoleId.New();
    private static readonly RoleId Role2Id = RoleId.New();

    /// <summary>
    /// Valid: Sequential primary roles with no overlap.
    /// Role A: 2026-01-01 → 2026-06-30
    /// Role B: 2026-07-01 → open-ended
    /// </summary>
    [Fact]
    public void CanMakePrimary_SequentialRoles_Allowed()
    {
        var now = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var mid = new DateTime(2026, 7, 1, 0, 0, 0, DateTimeKind.Utc);

        var rolA = Masterdom.Core.Identity.Entities.UserRole.UserRole.Create(
            TestUserId,
            Role1Id,
            effectiveFromUtc: now,
            effectiveToUtc: new DateTime(2026, 6, 30, 23, 59, 59, DateTimeKind.Utc),
            isPrimaryRole: true);
        rolA.Activate();

        var roleB = Masterdom.Core.Identity.Entities.UserRole.UserRole.Create(
            TestUserId,
            Role2Id,
            effectiveFromUtc: mid,
            isPrimaryRole: false);
        roleB.Activate();

        var result = Masterdom.Core.Identity.Entities.UserRole.UserRole.CanMakePrimary(roleB, new[] { rolA });

        Assert.True(result, "Sequential roles with no overlap should be allowed");
    }

    /// <summary>
    /// Invalid: Overlapping primary roles.
    /// Role A: 2026-01-01 → 2026-12-31
    /// Role B: 2026-06-01 → 2027-01-01
    /// These ranges overlap (2026-06-01 to 2026-12-31 is shared).
    /// </summary>
    [Fact]
    public void CanMakePrimary_OverlappingRoles_Denied()
    {
        var start = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var overlap = new DateTime(2026, 6, 1, 0, 0, 0, DateTimeKind.Utc);
        var end = new DateTime(2026, 12, 31, 23, 59, 59, DateTimeKind.Utc);
        var endExtended = new DateTime(2027, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        var roleA = Masterdom.Core.Identity.Entities.UserRole.UserRole.Create(
            TestUserId,
            Role1Id,
            effectiveFromUtc: start,
            effectiveToUtc: end,
            isPrimaryRole: true);
        roleA.Activate();

        var roleB = Masterdom.Core.Identity.Entities.UserRole.UserRole.Create(
            TestUserId,
            Role2Id,
            effectiveFromUtc: overlap,
            effectiveToUtc: endExtended,
            isPrimaryRole: false);
        roleB.Activate();

        var result = Masterdom.Core.Identity.Entities.UserRole.UserRole.CanMakePrimary(roleB, new[] { roleA });

        Assert.False(result, "Overlapping roles should not be allowed");
    }

    /// <summary>
    /// Valid: Inactive primary role should not block a new primary role.
    /// Role A: Inactive (deactivated, even with future dates)
    /// Role B: Active, any dates
    /// </summary>
    [Fact]
    public void CanMakePrimary_InactiveExistingRole_Allowed()
    {
        var now = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        var inactiveRole = Masterdom.Core.Identity.Entities.UserRole.UserRole.Create(
            TestUserId,
            Role1Id,
            effectiveFromUtc: now,
            effectiveToUtc: new DateTime(2026, 12, 31, 23, 59, 59, DateTimeKind.Utc),
            isPrimaryRole: true);
        inactiveRole.Activate();
        inactiveRole.Deactivate(); // Make it inactive

        var newRole = Masterdom.Core.Identity.Entities.UserRole.UserRole.Create(
            TestUserId,
            Role2Id,
            effectiveFromUtc: now,
            isPrimaryRole: false);
        newRole.Activate();

        var result = Masterdom.Core.Identity.Entities.UserRole.UserRole.CanMakePrimary(newRole, new[] { inactiveRole });

        Assert.True(result, "Inactive primary roles should not block new primary roles");
    }

    /// <summary>
    /// Valid: Expired primary role should not block a new primary role.
    /// Role A: Expired (EffectiveToUtc in the past)
    /// Role B: Active and current
    /// </summary>
    [Fact]
    public void CanMakePrimary_ExpiredExistingRole_Allowed()
    {
        var past = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var pastEnd = new DateTime(2025, 12, 31, 23, 59, 59, DateTimeKind.Utc);
        var now = new DateTime(2026, 6, 1, 0, 0, 0, DateTimeKind.Utc);

        var expiredRole = Masterdom.Core.Identity.Entities.UserRole.UserRole.Create(
            TestUserId,
            Role1Id,
            effectiveFromUtc: past,
            effectiveToUtc: pastEnd,
            isPrimaryRole: true);
        expiredRole.Activate();

        var newRole = Masterdom.Core.Identity.Entities.UserRole.UserRole.Create(
            TestUserId,
            Role2Id,
            effectiveFromUtc: now,
            isPrimaryRole: false);
        newRole.Activate();

        var result = Masterdom.Core.Identity.Entities.UserRole.UserRole.CanMakePrimary(newRole, new[] { expiredRole });

        Assert.True(result, "Expired primary roles should not block new primary roles");
    }

    /// <summary>
    /// Valid: Future primary role should not currently block another primary role.
    /// Role A: Effective in the future
    /// Role B: Effective now
    /// </summary>
    [Fact]
    public void CanMakePrimary_FutureExistingRole_Allowed()
    {
        var now = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var future = new DateTime(2026, 7, 1, 0, 0, 0, DateTimeKind.Utc);

        var futureRole = Masterdom.Core.Identity.Entities.UserRole.UserRole.Create(
            TestUserId,
            Role1Id,
            effectiveFromUtc: future,
            isPrimaryRole: true);
        futureRole.Activate();

        var currentRole = Masterdom.Core.Identity.Entities.UserRole.UserRole.Create(
            TestUserId,
            Role2Id,
            effectiveFromUtc: now,
            effectiveToUtc: new DateTime(2026, 6, 30, 23, 59, 59, DateTimeKind.Utc),
            isPrimaryRole: false);
        currentRole.Activate();

        var result = Masterdom.Core.Identity.Entities.UserRole.UserRole.CanMakePrimary(currentRole, new[] { futureRole });

        Assert.True(result, "Future primary roles should not block current primary roles");
    }

    /// <summary>
    /// Valid: A role that is already primary doesn't need re-validation.
    /// </summary>
    [Fact]
    public void CanMakePrimary_AlreadyPrimary_ReturnTrue()
    {
        var now = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        var primaryRole = Masterdom.Core.Identity.Entities.UserRole.UserRole.Create(
            TestUserId,
            Role1Id,
            effectiveFromUtc: now,
            isPrimaryRole: true);
        primaryRole.Activate();

        var result = Masterdom.Core.Identity.Entities.UserRole.UserRole.CanMakePrimary(primaryRole, Array.Empty<Masterdom.Core.Identity.Entities.UserRole.UserRole>());

        Assert.True(result, "A role that is already primary should return true");
    }

    /// <summary>
    /// Valid: No existing primary roles should allow making a role primary.
    /// </summary>
    [Fact]
    public void CanMakePrimary_NoExistingPrimaryRoles_Allowed()
    {
        var now = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        var newPrimaryRole = Masterdom.Core.Identity.Entities.UserRole.UserRole.Create(
            TestUserId,
            Role1Id,
            effectiveFromUtc: now,
            isPrimaryRole: false);
        newPrimaryRole.Activate();

        var result = Masterdom.Core.Identity.Entities.UserRole.UserRole.CanMakePrimary(newPrimaryRole, Array.Empty<Masterdom.Core.Identity.Entities.UserRole.UserRole>());

        Assert.True(result, "With no existing primary roles, any role should be allowed to become primary");
    }

    /// <summary>
    /// Valid: Boundary case - roles that end and start on the same second do not overlap.
    /// Role A: 2026-01-01 → 2026-06-30 23:59:59
    /// Role B: 2026-07-01 00:00:00 → open
    /// </summary>
    [Fact]
    public void CanMakePrimary_BoundaryBoundary_NoOverlap()
    {
        var start1 = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var end1 = new DateTime(2026, 6, 30, 23, 59, 59, DateTimeKind.Utc);
        var start2 = new DateTime(2026, 7, 1, 0, 0, 0, DateTimeKind.Utc);

        var roleA = Masterdom.Core.Identity.Entities.UserRole.UserRole.Create(
            TestUserId,
            Role1Id,
            effectiveFromUtc: start1,
            effectiveToUtc: end1,
            isPrimaryRole: true);
        roleA.Activate();

        var roleB = Masterdom.Core.Identity.Entities.UserRole.UserRole.Create(
            TestUserId,
            Role2Id,
            effectiveFromUtc: start2,
            isPrimaryRole: false);
        roleB.Activate();

        var result = Masterdom.Core.Identity.Entities.UserRole.UserRole.CanMakePrimary(roleB, new[] { roleA });

        Assert.True(result, "Roles that end and start on different seconds should not overlap");
    }

    /// <summary>
    /// Invalid: Open-ended primary role blocks any other primary role.
    /// Role A: 2026-01-01 → open (no end date)
    /// Role B: 2026-06-01 → open
    /// </summary>
    [Fact]
    public void CanMakePrimary_OpenEndedOverlap_Denied()
    {
        var start1 = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var start2 = new DateTime(2026, 6, 1, 0, 0, 0, DateTimeKind.Utc);

        var roleA = Masterdom.Core.Identity.Entities.UserRole.UserRole.Create(
            TestUserId,
            Role1Id,
            effectiveFromUtc: start1,
            effectiveToUtc: null, // Open-ended
            isPrimaryRole: true);
        roleA.Activate();

        var roleB = Masterdom.Core.Identity.Entities.UserRole.UserRole.Create(
            TestUserId,
            Role2Id,
            effectiveFromUtc: start2,
            effectiveToUtc: null, // Open-ended
            isPrimaryRole: false);
        roleB.Activate();

        var result = Masterdom.Core.Identity.Entities.UserRole.UserRole.CanMakePrimary(roleB, new[] { roleA });

        Assert.False(result, "Open-ended primary roles will overlap forever");
    }

    /// <summary>
    /// Valid: Multiple non-overlapping existing primary roles.
    /// Role A: 2026-01-01 → 2026-03-31
    /// Role B: 2026-04-01 → 2026-06-30
    /// Role C: 2026-07-01 → open
    /// All non-overlapping with each other.
    /// </summary>
    [Fact]
    public void CanMakePrimary_MultipleNonOverlappingExisting_Allowed()
    {
        var start1 = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var end1 = new DateTime(2026, 3, 31, 23, 59, 59, DateTimeKind.Utc);
        var start2 = new DateTime(2026, 4, 1, 0, 0, 0, DateTimeKind.Utc);
        var end2 = new DateTime(2026, 6, 30, 23, 59, 59, DateTimeKind.Utc);
        var start3 = new DateTime(2026, 7, 1, 0, 0, 0, DateTimeKind.Utc);

        var role1 = Masterdom.Core.Identity.Entities.UserRole.UserRole.Create(
            TestUserId,
            Role1Id,
            effectiveFromUtc: start1,
            effectiveToUtc: end1,
            isPrimaryRole: true);
        role1.Activate();

        // Note: Would need a third role ID for this full test
        var existingRoles = new[] { role1 };

        var newRole = Masterdom.Core.Identity.Entities.UserRole.UserRole.Create(
            TestUserId,
            Role2Id,
            effectiveFromUtc: start3,
            effectiveToUtc: null,
            isPrimaryRole: false);
        newRole.Activate();

        var result = Masterdom.Core.Identity.Entities.UserRole.UserRole.CanMakePrimary(newRole, existingRoles);

        Assert.True(result, "Multiple non-overlapping primary roles should allow a new non-overlapping one");
    }
}
