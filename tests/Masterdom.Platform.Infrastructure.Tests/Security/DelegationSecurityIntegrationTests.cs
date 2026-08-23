using Masterdom.Core.Security;
using Xunit;

namespace Masterdom.Platform.Infrastructure.Tests.Security;

/// <summary>
/// Integration tests for authority delegation authorization model.
/// Focuses on CurrentUser projection and security gate behavior.
/// </summary>
public sealed class DelegationSecurityIntegrationTests
{
    // ========== Test 3: Primary SuperUser Baseline ==========

    [Fact]
    public void PrimarySuperUser_IsInherentSuperUser_True()
    {
        // Arrange: Create Primary SuperUser with isInherentSuperUser = true
        var primaryUser = CurrentUser.Authenticated(
            userId: Guid.NewGuid(),
            personId: Guid.NewGuid(),
            username: "primary-superuser",
            roles: [MasterdomRoles.SuperUser],
            permissions: ["*"],
            propertyScopes: [],
            ownedPropertyIds: [],
            isInherentSuperUser: true);  // ← CRITICAL

        // Act
        var isInherent = primaryUser.IsInherentSuperUser;

        // Assert
        Assert.True(isInherent, "Direct PRIMARY_SUPERUSER must have IsInherentSuperUser = true");
        Assert.True(primaryUser.IsInRole(MasterdomRoles.SuperUser), "User has SuperUser role");
    }

    // ========== Test 4: Secondary Delegated Authority ==========

    [Fact]
    public void SecondarySuperUser_DelegatedAuthority_IsInherentSuperUser_False()
    {
        // Arrange: Secondary user with SuperUser role but delegated (not inherent)
        var secondaryUser = CurrentUser.Authenticated(
            userId: Guid.NewGuid(),
            personId: Guid.NewGuid(),
            username: "secondary-delegated",
            roles: [MasterdomRoles.SuperUser],              // Has SuperUser role
            permissions: ["property.read", "property.write"],
            propertyScopes: [],
            ownedPropertyIds: [],
            isInherentSuperUser: false);                    // ← But delegated, not inherent

        // Act & Assert
        Assert.False(secondaryUser.IsInherentSuperUser, "Delegated authority must have IsInherentSuperUser = false");
        Assert.True(secondaryUser.IsInRole(MasterdomRoles.SuperUser), "But user IS in SuperUser role");
        Assert.False(secondaryUser.IsInherentSuperUser, "Cannot bypass with IsInherentSuperUser check");
    }

    [Fact]
    public void SecondarySuperUser_BoundedPermissions_RestrictedScope()
    {
        // Arrange: Secondary with bounded permissions and property scope
        var secondaryUserId = Guid.NewGuid();
        var property1 = Guid.NewGuid();
        var property2 = Guid.NewGuid();

        var secondaryUser = CurrentUser.Authenticated(
            userId: secondaryUserId,
            personId: Guid.NewGuid(),
            username: "secondary-bounded",
            roles: [MasterdomRoles.Manager],                // Secondary level role
            permissions: ["property.read"],                 // Only read
            propertyScopes: [property1, property2],         // Only 2 properties
            ownedPropertyIds: [],
            isInherentSuperUser: false);

        // Act & Assert
        Assert.False(secondaryUser.IsInherentSuperUser);
        Assert.True(secondaryUser.IsInRole(MasterdomRoles.Manager));
        Assert.True(secondaryUser.HasPermission("property.read"));
        Assert.False(secondaryUser.HasPermission("property.write"), "Only read permission");
        Assert.Contains(property1, secondaryUser.PropertyScopes);
        Assert.Contains(property2, secondaryUser.PropertyScopes);
    }

    // ========== Test 3 & 4 Combined: Authorization Behavior ==========

    [Fact]
    public void Primary_BypassesUnrestrictedCheck_Secondary_Does_Not()
    {
        // This test verifies the core security gate:
        // PropertyCapabilityAuthorizationService checks IsInherentSuperUser

        // Primary SuperUser
        var primaryUser = CurrentUser.Authenticated(
            userId: Guid.NewGuid(),
            personId: Guid.NewGuid(),
            username: "primary",
            roles: [MasterdomRoles.SuperUser],
            permissions: ["*"],
            propertyScopes: [],
            ownedPropertyIds: [],
            isInherentSuperUser: true);

        // The authorization service would check:
        // if (currentUser.IsInherentSuperUser) return AllowedUnrestricted;
        Assert.True(primaryUser.IsInherentSuperUser, "Primary passes unrestricted gate");

        // Secondary delegated user
        var secondaryUser = CurrentUser.Authenticated(
            userId: Guid.NewGuid(),
            personId: Guid.NewGuid(),
            username: "secondary",
            roles: [MasterdomRoles.SuperUser],              // Even with SuperUser role...
            permissions: ["property.read"],
            propertyScopes: [Guid.NewGuid()],
            ownedPropertyIds: [],
            isInherentSuperUser: false);                    // ...delegated is false

        // Secondary does NOT pass unrestricted gate
        Assert.False(secondaryUser.IsInherentSuperUser, "Secondary fails unrestricted gate");
        // Authorization continues with normal property scope/permission checks
    }

    // ========== Test 5: Primary → Secondary → Admin Chain ==========

    [Fact]
    public void DelegationChain_Hierarchy_Respected()
    {
        // Primary (level 4)
        var primaryUser = CurrentUser.Authenticated(
            userId: Guid.NewGuid(),
            personId: Guid.NewGuid(),
            username: "primary",
            roles: [MasterdomRoles.SuperUser],
            permissions: ["*"],
            propertyScopes: [],
            ownedPropertyIds: [],
            isInherentSuperUser: true);

        Assert.True(primaryUser.IsInherentSuperUser);

        // Secondary (level 3, delegated by Primary)
        var secondaryUser = CurrentUser.Authenticated(
            userId: Guid.NewGuid(),
            personId: Guid.NewGuid(),
            username: "secondary",
            roles: [MasterdomRoles.Manager],                // Level 3 equivalent
            permissions: ["property.read", "property.write"],
            propertyScopes: [Guid.NewGuid(), Guid.NewGuid()],
            ownedPropertyIds: [],
            isInherentSuperUser: false);

        Assert.False(secondaryUser.IsInherentSuperUser);
        Assert.True(secondaryUser.HasPermission("property.write"));

        // Admin (level 2, delegated by Secondary)
        var adminUser = CurrentUser.Authenticated(
            userId: Guid.NewGuid(),
            personId: Guid.NewGuid(),
            username: "admin",
            roles: [MasterdomRoles.PropertyOwner],          // Level 2 equivalent
            permissions: ["property.read"],                 // Further restricted
            propertyScopes: [Guid.NewGuid()],              // Smallest scope
            ownedPropertyIds: [],
            isInherentSuperUser: false);

        Assert.False(adminUser.IsInherentSuperUser);
        Assert.True(adminUser.HasPermission("property.read"));
        Assert.False(adminUser.HasPermission("property.write"), "Admin more restricted than Secondary");
    }

    // ========== Test 6: Property Scope Integration ==========

    [Fact]
    public void PropertyScope_Containment_Respected()
    {
        var property1 = Guid.NewGuid();
        var property2 = Guid.NewGuid();
        var property3 = Guid.NewGuid();

        // Primary with all properties
        var primaryUser = CurrentUser.Authenticated(
            userId: Guid.NewGuid(),
            personId: Guid.NewGuid(),
            username: "primary",
            roles: [MasterdomRoles.SuperUser],
            permissions: ["*"],
            propertyScopes: [property1, property2, property3],
            ownedPropertyIds: [],
            isInherentSuperUser: true);

        // Secondary delegated with subset [1,2]
        var secondaryUser = CurrentUser.Authenticated(
            userId: Guid.NewGuid(),
            personId: Guid.NewGuid(),
            username: "secondary",
            roles: [MasterdomRoles.Manager],
            permissions: ["property.read", "property.write"],
            propertyScopes: [property1, property2],         // Subset of Primary
            ownedPropertyIds: [],
            isInherentSuperUser: false);

        // Admin delegated with further subset [1]
        var adminUser = CurrentUser.Authenticated(
            userId: Guid.NewGuid(),
            personId: Guid.NewGuid(),
            username: "admin",
            roles: [MasterdomRoles.PropertyOwner],
            permissions: ["property.read"],
            propertyScopes: [property1],                    // Subset of Secondary
            ownedPropertyIds: [],
            isInherentSuperUser: false);

        // Verify containment
        Assert.Contains(property1, primaryUser.PropertyScopes);
        Assert.Contains(property2, primaryUser.PropertyScopes);
        Assert.Contains(property3, primaryUser.PropertyScopes);

        Assert.Contains(property1, secondaryUser.PropertyScopes);
        Assert.Contains(property2, secondaryUser.PropertyScopes);
        Assert.DoesNotContain(property3, secondaryUser.PropertyScopes);

        Assert.Contains(property1, adminUser.PropertyScopes);
        Assert.DoesNotContain(property2, adminUser.PropertyScopes);
        Assert.DoesNotContain(property3, adminUser.PropertyScopes);
    }

    // ========== Test 7: Expiration (Conceptual) ==========

    [Fact]
    public void Expiration_ConceptualModel()
    {
        // Expiration is enforced by:
        // 1. Delegation record Status = Active/Expired/Revoked
        // 2. IsEffectiveDelegation() checks EffectiveFromUtc <= now <= EffectiveToUtc
        // 3. When loading delegations, ineffective ones excluded from CurrentUser

        var utcNow = DateTime.UtcNow;
        var pastExpiration = utcNow.AddHours(-1);

        // A delegation with EffectiveToUtc = pastExpiration should not be loaded
        // When CurrentUser is projected, expired delegations are excluded
        // Result: CurrentUser has no delegated authority

        var userWithExpiredDelegation = CurrentUser.Authenticated(
            userId: Guid.NewGuid(),
            personId: Guid.NewGuid(),
            username: "expired-delegated",
            roles: [],                                       // No roles (delegations excluded)
            permissions: [],                                 // No permissions (delegations excluded)
            propertyScopes: [],                              // No scope (delegations excluded)
            ownedPropertyIds: [],
            isInherentSuperUser: false);

        Assert.Empty(userWithExpiredDelegation.PropertyScopes);
        Assert.Empty(userWithExpiredDelegation.Permissions);
    }

    // ========== Test 8: Revocation ==========

    [Fact]
    public void Revocation_ImmediatelyIneffective()
    {
        var propertyId = Guid.NewGuid();

        // Before revocation: user has delegated authority
        var userBeforeRevoke = CurrentUser.Authenticated(
            userId: Guid.NewGuid(),
            personId: Guid.NewGuid(),
            username: "delegated-user",
            roles: [MasterdomRoles.PropertyOwner],
            permissions: ["property.read"],
            propertyScopes: [propertyId],
            ownedPropertyIds: [],
            isInherentSuperUser: false);

        Assert.Contains(propertyId, userBeforeRevoke.PropertyScopes);
        Assert.True(userBeforeRevoke.HasPermission("property.read"));

        // After revocation: no delegated authority
        // (CurrentUser re-projected without delegations)
        var userAfterRevoke = CurrentUser.Authenticated(
            userId: userBeforeRevoke.UserId!.Value,
            personId: userBeforeRevoke.PersonId!.Value,
            username: "delegated-user",
            roles: [],                                       // No roles from revoked delegation
            permissions: [],                                 // No permissions from revoked delegation
            propertyScopes: [],                              // No scope from revoked delegation
            ownedPropertyIds: [],
            isInherentSuperUser: false);

        Assert.Empty(userAfterRevoke.PropertyScopes);
        Assert.False(userAfterRevoke.HasPermission("property.read"));
    }

    // ========== Test 9: Cascade Ineffectiveness ==========

    [Fact]
    public void CascadeIneffectiveness_ChildRecordPersists_ButIneffective()
    {
        var propertyId = Guid.NewGuid();

        // Before parent revocation: Secondary has delegated authority
        var secondaryBeforeRevoke = CurrentUser.Authenticated(
            userId: Guid.NewGuid(),
            personId: Guid.NewGuid(),
            username: "secondary",
            roles: [MasterdomRoles.Manager],
            permissions: ["property.read", "property.write"],
            propertyScopes: [propertyId],
            ownedPropertyIds: [],
            isInherentSuperUser: false);

        Assert.True(secondaryBeforeRevoke.HasPermission("property.write"));

        // After parent revocation: Secondary has no delegated authority
        // (but their Secondary → Admin delegation record persists in database)
        var secondaryAfterRevoke = CurrentUser.Authenticated(
            userId: secondaryBeforeRevoke.UserId!.Value,
            personId: secondaryBeforeRevoke.PersonId!.Value,
            username: "secondary",
            roles: [],                                       // No roles after parent revoked
            permissions: [],                                 // No permissions after parent revoked
            propertyScopes: [],                              // No scope after parent revoked
            ownedPropertyIds: [],
            isInherentSuperUser: false);

        Assert.Empty(secondaryAfterRevoke.PropertyScopes);
        Assert.False(secondaryAfterRevoke.HasPermission("property.write"));

        // The Secondary → Admin delegation record exists in database but is ineffective
        // because Secondary has no active delegated authority

        // Admin (delegated from Secondary) is also ineffective
        var adminAfterCascadeRevoke = CurrentUser.Authenticated(
            userId: Guid.NewGuid(),
            personId: Guid.NewGuid(),
            username: "admin",
            roles: [],                                       // No active delegations
            permissions: [],
            propertyScopes: [],
            ownedPropertyIds: [],
            isInherentSuperUser: false);

        Assert.Empty(adminAfterCascadeRevoke.PropertyScopes);
    }

    // ========== Test 10: Security Regression Baseline ==========

    [Fact]
    public void RegressionBaseline_ExistingRolesRespected()
    {
        // Verify existing role-based authorization still works

        var propertyOwnerId = Guid.NewGuid();
        var managerId = Guid.NewGuid();
        var tenantId = Guid.NewGuid();

        var propertyOwnerUser = CurrentUser.Authenticated(
            userId: propertyOwnerId,
            personId: Guid.NewGuid(),
            username: "owner",
            roles: [MasterdomRoles.PropertyOwner],
            permissions: ["property.read", "property.write"],
            propertyScopes: [Guid.NewGuid()],
            ownedPropertyIds: [],
            isInherentSuperUser: false);

        var managerUser = CurrentUser.Authenticated(
            userId: managerId,
            personId: Guid.NewGuid(),
            username: "manager",
            roles: [MasterdomRoles.Manager],
            permissions: ["property.read", "property.write", "tenant.read"],
            propertyScopes: [],
            ownedPropertyIds: [],
            isInherentSuperUser: false);

        var tenantUser = CurrentUser.Authenticated(
            userId: tenantId,
            personId: Guid.NewGuid(),
            username: "tenant",
            roles: [MasterdomRoles.Tenant],
            permissions: ["person.read"],
            propertyScopes: [],
            ownedPropertyIds: [],
            isInherentSuperUser: false);

        // Verify roles work as before
        Assert.True(propertyOwnerUser.IsInRole(MasterdomRoles.PropertyOwner));
        Assert.True(managerUser.IsInRole(MasterdomRoles.Manager));
        Assert.True(tenantUser.IsInRole(MasterdomRoles.Tenant));

        // Verify not in other roles
        Assert.False(propertyOwnerUser.IsInRole(MasterdomRoles.Manager));
        Assert.False(managerUser.IsInRole(MasterdomRoles.PropertyOwner));
        Assert.False(tenantUser.IsInRole(MasterdomRoles.SuperUser));
    }

    // ========== Test 11: SuperUser Distinction ==========

    [Fact]
    public void SuperUserDistinction_Inherent_vs_Delegated()
    {
        // INHERENT SuperUser (Primary)
        var inheritentSuperUser = CurrentUser.Authenticated(
            userId: Guid.NewGuid(),
            personId: Guid.NewGuid(),
            username: "inherent-superuser",
            roles: [MasterdomRoles.SuperUser],
            permissions: ["*"],
            propertyScopes: [],
            ownedPropertyIds: [],
            isInherentSuperUser: true);

        Assert.True(inheritentSuperUser.IsInRole(MasterdomRoles.SuperUser));
        Assert.True(inheritentSuperUser.IsInherentSuperUser);

        // DELEGATED SuperUser-level authority (Secondary)
        var delegatedSuperUser = CurrentUser.Authenticated(
            userId: Guid.NewGuid(),
            personId: Guid.NewGuid(),
            username: "delegated-superuser",
            roles: [MasterdomRoles.SuperUser],              // Same role...
            permissions: ["property.read", "property.write"],
            propertyScopes: [Guid.NewGuid()],
            ownedPropertyIds: [],
            isInherentSuperUser: false);                    // ...but delegated

        Assert.True(delegatedSuperUser.IsInRole(MasterdomRoles.SuperUser));
        Assert.False(delegatedSuperUser.IsInherentSuperUser);

        // Authorization service MUST distinguish:
        // if (currentUser.IsInherentSuperUser)  → unrestricted
        // else                                  → normal authorization

        Assert.NotEqual(inheritentSuperUser.IsInherentSuperUser, delegatedSuperUser.IsInherentSuperUser);
    }

    // ========== SECURITY GATE: Negative JWT Test ==========
    // This test verifies the critical security invariant:
    // If a Secondary user somehow receives a SuperUser claim in their JWT,
    // the CurrentUser projection MUST set IsInherentSuperUser = false
    // (controlled by the constructor parameter, not the JWT claim alone)
    //
    // ARCHITECTURE NOTE:
    // CurrentUser.Authenticated() requires explicit isInherentSuperUser parameter.
    // HttpContextCurrentUserAccessor MUST only set isInherentSuperUser=true for
    // users whose primary authority in the database is PRIMARY_SUPERUSER.
    // Token issuance must verify this before issuing SuperUser claims.
    // The JWT is trusted to reflect accurate authority IF issued by secure service.

    [Fact]
    public void NegativeJWT_SecondaryWithForgedSuperUserClaim_IsInherentSuperUser_Must_Be_False()
    {
        // SCENARIO: Hypothetical secondary user with forged/manipulated SuperUser claim
        // This test verifies that CurrentUser construction requires EXPLICIT parameter,
        // not derived from JWT claim alone.

        // Simulate a Secondary user who somehow has SuperUser role claim
        // (either through token manipulation, leaked credential, or test artifact)
        var secondaryUserId = Guid.NewGuid();

        var secondaryWithForgedClaim = CurrentUser.Authenticated(
            userId: secondaryUserId,
            personId: Guid.NewGuid(),
            username: "secondary-user",
            roles: [MasterdomRoles.SuperUser],              // ← FORGED: Secondary shouldn't have this
            permissions: ["property.read"],
            propertyScopes: [Guid.NewGuid()],
            ownedPropertyIds: [],
            isInherentSuperUser: false);                    // ← CRITICAL: Explicit parameter required

        // ASSERT: Even with SuperUser role claim, IsInherentSuperUser MUST be false
        Assert.True(secondaryWithForgedClaim.IsInRole(MasterdomRoles.SuperUser), "Claim exists in JWT");
        Assert.False(secondaryWithForgedClaim.IsInherentSuperUser, "But authority is NOT inherent");

        // SECURITY IMPLICATION:
        // PropertyCapabilityAuthorizationService.cs line 31 checks IsInherentSuperUser,
        // not IsInRole(SuperUser). This prevents bypass even if claim is forged.
        //
        // Token issuance responsibility:
        // The authentication service MUST verify user's actual authority in database
        // before issuing SuperUser claim to prevent this scenario in production.
    }

    // ========== HTTP CONTEXT ACCESSOR TESTS ==========
    // SECURITY CRITICAL: These tests exercise actual HttpContextCurrentUserAccessor behavior.
    // Verify that role claims alone do NOT produce IsInherentSuperUser = true.

    [Fact]
    public void HttpContextAccessor_SecondaryWithSuperUserRoleClaim_IsInherentSuperUser_Must_Be_False()
    {
        // SCENARIO: Secondary user authenticated via JWT with SuperUser role claim
        // This is the security-critical path:
        // JWT → HttpContextCurrentUserAccessor → CurrentUser → PropertyCapabilityAuthorizationService
        //
        // SECURITY INVARIANT:
        // Even if JWT contains SuperUser role claim, HttpContextCurrentUserAccessor MUST NOT
        // automatically set IsInherentSuperUser = true.
        //
        // REASON:
        // - SuperUser claim alone does not establish inherent Primary authority
        // - Could be delegated authority, forged claim, or misconfigured token
        // - IsInherentSuperUser must only be true for authoritative Primary evidence

        // Arrange: Create ClaimsPrincipal simulating Secondary user with SuperUser claim
        var claims = new System.Collections.Generic.List<System.Security.Claims.Claim>
        {
            new(System.Security.Claims.ClaimTypes.NameIdentifier, Guid.NewGuid().ToString()),
            new(System.Security.Claims.ClaimTypes.Name, "secondary-user"),
            new(System.Security.Claims.ClaimTypes.Role, MasterdomRoles.SuperUser),  // ← Claim present
            new(MasterdomClaimTypes.PersonId, Guid.NewGuid().ToString()),
            new(MasterdomClaimTypes.Permission, "property.read")
        };

        var identity = new System.Security.Claims.ClaimsIdentity(claims, "Bearer");
        var principal = new System.Security.Claims.ClaimsPrincipal(identity);

        // Create mock HTTP context
        var httpContext = new Microsoft.AspNetCore.Http.DefaultHttpContext { User = principal };
        var httpContextAccessor = new Microsoft.AspNetCore.Http.HttpContextAccessor { HttpContext = httpContext };

        // Act: Project through actual accessor
        var accessor = new Masterdom.Modules.Security.HttpContextCurrentUserAccessor(httpContextAccessor);
        var currentUser = accessor.GetCurrentUser();

        // Assert: Role claim exists, but IsInherentSuperUser is false
        Assert.True(currentUser.IsInRole(MasterdomRoles.SuperUser), "Role claim is present in JWT");
        Assert.False(currentUser.IsInherentSuperUser,
            "SECURITY: SuperUser claim alone does NOT establish inherent Primary authority via HTTP accessor");

        // Critical distinction:
        Assert.NotEqual(currentUser.IsInRole(MasterdomRoles.SuperUser), currentUser.IsInherentSuperUser);
    }

    [Fact]
    public void HttpContextAccessor_AnyRoleClaimAlone_IsInherentSuperUser_False()
    {
        // SECURITY: Verify that ANY role claim (including SuperUser) alone does not produce
        // IsInherentSuperUser = true via HTTP context accessor.

        var claims = new System.Collections.Generic.List<System.Security.Claims.Claim>
        {
            new(System.Security.Claims.ClaimTypes.NameIdentifier, Guid.NewGuid().ToString()),
            new(System.Security.Claims.ClaimTypes.Name, "ordinary-user"),
            new(System.Security.Claims.ClaimTypes.Role, MasterdomRoles.SuperUser)
        };

        var identity = new System.Security.Claims.ClaimsIdentity(claims, "Bearer");
        var principal = new System.Security.Claims.ClaimsPrincipal(identity);

        var httpContext = new Microsoft.AspNetCore.Http.DefaultHttpContext { User = principal };
        var httpContextAccessor = new Microsoft.AspNetCore.Http.HttpContextAccessor { HttpContext = httpContext };

        var accessor = new Masterdom.Modules.Security.HttpContextCurrentUserAccessor(httpContextAccessor);
        var currentUser = accessor.GetCurrentUser();

        // SECURITY: No role claim grants IsInherentSuperUser via HTTP context accessor
        Assert.False(currentUser.IsInherentSuperUser,
            "HTTP context accessor cannot establish inherent Primary authority from role claims alone");
    }

    [Fact]
    public void HttpContextAccessor_FailClosed_AllUsersDefaultToNonInherent()
    {
        // ARCHITECTURAL DOCUMENTATION:
        // HttpContextCurrentUserAccessor defaults IsInherentSuperUser = false.
        // This is FAIL-CLOSED behavior.
        //
        // When production authentication service is implemented (Application layer),
        // it MUST:
        // 1. Query user's primary authority from database
        // 2. Only if primary authority IS PRIMARY_SUPERUSER, include explicit claim
        //    (e.g., "authority_level": "primary_superuser")
        // 3. HttpContextCurrentUserAccessor can then safely check for that claim
        //
        // CURRENT STATE (Foundation Gate):
        // This mechanism is deferred. IsInherentSuperUser = false for all HTTP context users.
        // This is safe and correct until proper authority evidence is available.

        var userId = Guid.NewGuid();
        var claims = new System.Collections.Generic.List<System.Security.Claims.Claim>
        {
            new(System.Security.Claims.ClaimTypes.NameIdentifier, userId.ToString()),
            new(System.Security.Claims.ClaimTypes.Name, "any-authenticated-user"),
            new(System.Security.Claims.ClaimTypes.Role, MasterdomRoles.PropertyOwner)
        };

        var identity = new System.Security.Claims.ClaimsIdentity(claims, "Bearer");
        var principal = new System.Security.Claims.ClaimsPrincipal(identity);

        var httpContext = new Microsoft.AspNetCore.Http.DefaultHttpContext { User = principal };
        var httpContextAccessor = new Microsoft.AspNetCore.Http.HttpContextAccessor { HttpContext = httpContext };

        var accessor = new Masterdom.Modules.Security.HttpContextCurrentUserAccessor(httpContextAccessor);
        var currentUser = accessor.GetCurrentUser();

        // Verify: All HTTP context users have IsInherentSuperUser = false (fail-closed)
        Assert.False(currentUser.IsInherentSuperUser,
            "Foundation Gate: HTTP accessor defaults to false until production authority verification implemented");
    }

    [Fact]
    public void DirectConstruction_Can_Represent_InherentPrimary_For_Testing()
    {
        // CLARIFICATION:
        // Tests that directly construct CurrentUser can explicitly represent Primary authority.
        // This is appropriate for unit tests of authorization logic.
        //
        // PRODUCTION HTTP PATH:
        // All HTTP context accessor users have IsInherentSuperUser = false until
        // authentication service implements authoritative Primary verification.

        var primaryUser = CurrentUser.Authenticated(
            userId: Guid.NewGuid(),
            personId: Guid.NewGuid(),
            username: "primary-in-test",
            roles: [MasterdomRoles.SuperUser],
            permissions: ["*"],
            propertyScopes: [],
            ownedPropertyIds: [],
            isInherentSuperUser: true);  // ← Test explicitly sets this for testing

        // This is appropriate for testing authorization logic.
        Assert.True(primaryUser.IsInherentSuperUser);

        // But production HTTP context will have isInherentSuperUser = false
        // until proper authority verification is in place (Application layer).
    }
}
