using Masterdom.Core.Identity.Entities.ApiKey;
using Masterdom.Core.Identity.Entities.EmailVerification;
using Masterdom.Core.Identity.Entities.ExternalLogin;
using Masterdom.Core.Identity.Entities.IdentityProfile;
using Masterdom.Core.Identity.Entities.LoginAttempt;
using Masterdom.Core.Identity.Entities.MfaDevice;
using Masterdom.Core.Identity.Entities.Organization;
using Masterdom.Core.Identity.Entities.PasswordReset;
using Masterdom.Core.Identity.Entities.Permission;
using Masterdom.Modules.FinancialLedger.Domain.Entities.FinancialLedger;
using Masterdom.Modules.Metering.Domain.Entities.Metering;
using Masterdom.Modules.Payment.Domain.Entities.Payment;
using Masterdom.Modules.PolicyFramework.Domain.Entities.PolicyFramework;
using Masterdom.Modules.SubsidyOptimization.Domain.Entities.SubsidyOptimization;
using Masterdom.Modules.UtilityRating.Domain.Entities.UtilityRating;
using Masterdom.Modules.Billing.Domain.Entities.Billing;
using Masterdom.Modules.People.Domain.Entities.Person;
using Masterdom.Core.Identity.Entities.RefreshToken;
using Masterdom.Core.Identity.Entities.Relationship;
using Masterdom.Core.Identity.Entities.Role;
using Masterdom.Core.Identity.Entities.RolePermission;
using Masterdom.Core.Identity.Entities.User;
using Masterdom.Core.Identity.Entities.UserRole;
using Masterdom.Core.Identity.Entities.UserSession;
using Masterdom.Modules.Lease.Domain.Entities.Lease;
using Masterdom.Modules.Properties.Domain.Entities.Property;
using Masterdom.Modules.Tenancy.Domain.Entities.Tenancy;
using Masterdom.Infrastructure.Persistence.Configuration;
using Masterdom.Infrastructure.Persistence.FinancialLedger;
using Masterdom.Infrastructure.Persistence.Metadata;
using Masterdom.Infrastructure.Persistence.Rules;
using Masterdom.Infrastructure.Persistence.Workflow;
using Microsoft.EntityFrameworkCore;
using BillAggregate = Masterdom.Modules.Billing.Domain.Entities.Billing.Bill;
using LeaseAggregate = Masterdom.Modules.Lease.Domain.Entities.Lease.Lease;
using PropertyAggregate = Masterdom.Modules.Properties.Domain.Entities.Property.Property;
using TenancyAggregate = Masterdom.Modules.Tenancy.Domain.Entities.Tenancy.Tenancy;
using MeterAggregate = Masterdom.Modules.Metering.Domain.Entities.Metering.Meter;
using LedgerAggregate = Masterdom.Modules.FinancialLedger.Domain.Entities.FinancialLedger.Ledger;
using PaymentAggregate = Masterdom.Modules.Payment.Domain.Entities.Payment.Payment;
using PolicyAggregate = Masterdom.Modules.PolicyFramework.Domain.Entities.PolicyFramework.Policy;
using UtilityRatingAggregate = Masterdom.Modules.UtilityRating.Domain.Entities.UtilityRating.UtilityRating;
using OptimizationRunAggregate = Masterdom.Modules.SubsidyOptimization.Domain.Entities.SubsidyOptimization.OptimizationRun;

namespace Masterdom.Infrastructure.Persistence;

/// <summary>
/// Represents the application's primary EF Core database context.
/// </summary>
public sealed class MasterdomDbContext : DbContext
{
    /// <summary>
    /// Initializes a new instance of the <see cref="MasterdomDbContext"/> class.
    /// </summary>
    /// <param name="options">The EF Core DbContext options.</param>
    public MasterdomDbContext(DbContextOptions<MasterdomDbContext> options)
        : base(options)
    {
    }

    #region Property Domain

    /// <summary>
    /// Gets the properties.
    /// </summary>
    public DbSet<PropertyAggregate> Properties => Set<PropertyAggregate>();

    /// <summary>
    /// Gets the leases.
    /// </summary>
    public DbSet<LeaseAggregate> Leases => Set<LeaseAggregate>();

    /// <summary>
    /// Gets the tenancies.
    /// </summary>
    public DbSet<TenancyAggregate> Tenancies => Set<TenancyAggregate>();

    /// <summary>
    /// Gets the meters.
    /// </summary>
    public DbSet<MeterAggregate> Meters => Set<MeterAggregate>();

    /// <summary>
    /// Gets the bills.
    /// </summary>
    public DbSet<BillAggregate> Bills => Set<BillAggregate>();

    /// <summary>
    /// Gets the payments.
    /// </summary>
    public DbSet<PaymentAggregate> Payments => Set<PaymentAggregate>();

    /// <summary>
    /// Gets the ledgers.
    /// </summary>
    public DbSet<LedgerAggregate> Ledgers => Set<LedgerAggregate>();

    /// <summary>
    /// Gets durable prepared journal lifecycle records.
    /// </summary>
    public DbSet<PersistedPreparedJournalEntity> PreparedJournals => Set<PersistedPreparedJournalEntity>();

    /// <summary>
    /// Gets the utility ratings.
    /// </summary>
    public DbSet<UtilityRatingAggregate> UtilityRatings => Set<UtilityRatingAggregate>();

    /// <summary>
    /// Gets the subsidy optimization runs.
    /// </summary>
    public DbSet<OptimizationRunAggregate> OptimizationRuns => Set<OptimizationRunAggregate>();

    /// <summary>
    /// Gets the policies.
    /// </summary>
    public DbSet<PolicyAggregate> Policies => Set<PolicyAggregate>();

    #endregion

    #region Identity Domain

    public DbSet<Person> Persons => Set<Person>();

    public DbSet<Organization> Organizations => Set<Organization>();

    public DbSet<IdentityProfile> IdentityProfiles => Set<IdentityProfile>();

    public DbSet<Relationship> Relationships => Set<Relationship>();

    public DbSet<User> Users => Set<User>();

    public DbSet<Role> Roles => Set<Role>();

    public DbSet<Permission> Permissions => Set<Permission>();

    public DbSet<UserRole> UserRoles => Set<UserRole>();

    public DbSet<RolePermission> RolePermissions => Set<RolePermission>();

    public DbSet<UserSession> UserSessions => Set<UserSession>();

    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

    public DbSet<LoginAttempt> LoginAttempts => Set<LoginAttempt>();

    public DbSet<PasswordReset> PasswordResets => Set<PasswordReset>();

    public DbSet<EmailVerification> EmailVerifications => Set<EmailVerification>();

    public DbSet<MfaDevice> MfaDevices => Set<MfaDevice>();

    public DbSet<ExternalLogin> ExternalLogins => Set<ExternalLogin>();

    public DbSet<ApiKey> ApiKeys => Set<ApiKey>();

    #endregion

    #region Platform Configuration

    public DbSet<PlatformConfigurationRecordEntity> PlatformConfigurationRecords =>
        Set<PlatformConfigurationRecordEntity>();

    public DbSet<PlatformMetadataDefinitionEntity> PlatformMetadataDefinitions =>
        Set<PlatformMetadataDefinitionEntity>();

    public DbSet<PlatformRuleSetEntity> PlatformRuleSets =>
        Set<PlatformRuleSetEntity>();

    public DbSet<PlatformRuleDefinitionEntity> PlatformRuleDefinitions =>
        Set<PlatformRuleDefinitionEntity>();

    public DbSet<PlatformWorkflowEntity> PlatformWorkflows =>
        Set<PlatformWorkflowEntity>();

    public DbSet<PlatformWorkflowVersionEntity> PlatformWorkflowVersions =>
        Set<PlatformWorkflowVersionEntity>();

    public DbSet<PlatformWorkflowStepEntity> PlatformWorkflowSteps =>
        Set<PlatformWorkflowStepEntity>();

    public DbSet<PlatformWorkflowTransitionEntity> PlatformWorkflowTransitions =>
        Set<PlatformWorkflowTransitionEntity>();

    #endregion

    /// <inheritdoc />
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        ArgumentNullException.ThrowIfNull(modelBuilder);

        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(
            typeof(MasterdomDbContext).Assembly);

        modelBuilder.ApplyConfigurationsFromAssembly(
            typeof(PropertyAggregate).Assembly);
    }
}
