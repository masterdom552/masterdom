using System.Text.Json;
using Masterdom.Core.Identifiers;
using Masterdom.Infrastructure.Persistence.Extensions;
using Masterdom.Modules.Lease.Domain.Entities.Lease;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using LeaseAggregate = Masterdom.Modules.Lease.Domain.Entities.Lease.Lease;

namespace Masterdom.Infrastructure.Persistence.Configurations.Lease;

/// <summary>
/// EF Core configuration for lease aggregate.
/// </summary>
public sealed class LeaseConfiguration : IEntityTypeConfiguration<LeaseAggregate>
{
    public void Configure(EntityTypeBuilder<LeaseAggregate> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.ToTable("leases");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasEntityIdConversion(LeaseId.From)
            .ValueGeneratedNever();

        builder.Property(x => x.Number)
            .HasValueObjectConversion(LeaseNumber.Create)
            .HasColumnName("lease_number")
            .HasMaxLength(50)
            .IsRequired();

        builder.HasIndex(x => x.Number)
            .IsUnique();

        builder.Property(x => x.Type)
            .HasValueObjectConversion(LeaseType.Create)
            .HasColumnName("lease_type")
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(x => x.Status)
            .HasValueObjectConversion(LeaseStatus.Create)
            .HasColumnName("status")
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(x => x.Tenancy)
            .HasConversion(
                value => value.TenancyId,
                value => TenancyReference.Create(value))
            .HasColumnName("tenancy_id")
            .IsRequired();

        builder.Property(x => x.Property)
            .HasConversion(
                value => value.PropertyId,
                value => PropertyReference.Create(value))
            .HasColumnName("property_id")
            .IsRequired();

        builder.Property(x => x.Unit)
            .HasConversion(
                value => value.UnitId,
                value => UnitReference.Create(value))
            .HasColumnName("unit_id")
            .IsRequired();

        builder.Property(x => x.Person)
            .HasConversion(
                value => value.PersonId.Value,
                value => PersonReference.Create(PersonId.From(value)))
            .HasColumnName("person_id")
            .IsRequired();

        builder.Property(x => x.TerminationReason)
            .HasConversion(
                value => value == null ? null : value.Value,
                value => string.IsNullOrWhiteSpace(value) ? null : TerminationReason.Create(value))
            .HasColumnName("termination_reason")
            .HasMaxLength(200);

        builder.OwnsMany(x => x.Versions, versionBuilder =>
        {
            versionBuilder.ToTable("lease_versions");

            versionBuilder.WithOwner()
                .HasForeignKey("lease_id");

            versionBuilder.Property<int>("id");
            versionBuilder.HasKey("id");

            versionBuilder.Property(x => x.VersionNumber)
                .HasColumnName("version_number")
                .IsRequired();

            versionBuilder.Property(x => x.IsActive)
                .HasColumnName("is_active")
                .IsRequired();

            versionBuilder.Property(x => x.RenewalDate)
                .HasConversion(
                    value => value == null ? (DateOnly?)null : value.Value,
                    value => value.HasValue ? RenewalDate.Create(value.Value) : null)
                .HasColumnName("renewal_date");

            versionBuilder.Property(x => x.EffectivePeriod)
                .HasConversion(
                    value => JsonSerializer.Serialize(
                        new EffectivePeriodPersistenceModel(
                            value.EffectiveDate.Value,
                            value.ExpiryDate.Value),
                        JsonSerializerOptions.Web),
                    json => DeserializeEffectivePeriod(json))
                .HasColumnName("effective_period")
                .HasColumnType("jsonb")
                .IsRequired();

            versionBuilder.Property(x => x.CommercialTerms)
                .HasConversion(
                    value => JsonSerializer.Serialize(
                        CommercialTermsPersistenceModel.FromDomain(value),
                        JsonSerializerOptions.Web),
                    json => DeserializeCommercialTerms(json))
                .HasColumnName("commercial_terms")
                .HasColumnType("jsonb")
                .IsRequired();

            versionBuilder.Property(x => x.LeaseClauses)
                .HasConversion(
                    value => JsonSerializer.Serialize(
                        LeaseClausesPersistenceModel.FromDomain(value),
                        JsonSerializerOptions.Web),
                    json => DeserializeLeaseClauses(json))
                .HasColumnName("lease_clauses")
                .HasColumnType("jsonb")
                .IsRequired();

            versionBuilder.HasIndex(x => new { x.VersionNumber, x.IsActive })
                .HasDatabaseName("ix_lease_versions_version_active");
        });

        builder.Navigation(x => x.Versions)
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.Ignore(x => x.DomainEvents);
    }

    private static EffectivePeriod DeserializeEffectivePeriod(string json)
    {
        var model = JsonSerializer.Deserialize<EffectivePeriodPersistenceModel>(json, JsonSerializerOptions.Web)
            ?? throw new InvalidOperationException("Unable to deserialize effective period.");

        return EffectivePeriod.Create(
            EffectiveDate.Create(model.EffectiveDate),
            ExpiryDate.Create(model.ExpiryDate));
    }

    private static CommercialTerms DeserializeCommercialTerms(string json)
    {
        var model = JsonSerializer.Deserialize<CommercialTermsPersistenceModel>(json, JsonSerializerOptions.Web)
            ?? throw new InvalidOperationException("Unable to deserialize commercial terms.");

        return CommercialTerms.Create(
            RentTerms.Create(
                model.MonthlyRent,
                BillingFrequency.Create(model.BillingFrequency),
                model.RentDueDay,
                model.GracePeriodDays),
            DepositTerms.Create(
                model.DepositAmount,
                model.IsDepositRefundable,
                SecurityDepositReference.Create(model.SecurityDepositReference),
                model.DepositRulesReference),
            RenewalTerms.Create(
                model.AutoRenew,
                model.RenewalNoticePeriodDays,
                model.RenewalPolicyReference),
            TerminationTerms.Create(
                model.TerminationNoticePeriodDays,
                model.TerminationPolicyReference,
                model.LateFeePolicyReference));
    }

    private static LeaseClauses DeserializeLeaseClauses(string json)
    {
        var model = JsonSerializer.Deserialize<LeaseClausesPersistenceModel>(json, JsonSerializerOptions.Web)
            ?? throw new InvalidOperationException("Unable to deserialize lease clauses.");

        var clauses = model.Items
            .Select(x => LeaseClause.Create(x.Code, x.Text))
            .ToList();

        return LeaseClauses.Create(ClauseCollection.Create(clauses));
    }

    private sealed record EffectivePeriodPersistenceModel(DateOnly EffectiveDate, DateOnly ExpiryDate);

    private sealed record LeaseClausePersistenceModel(string Code, string Text);

    private sealed record LeaseClausesPersistenceModel(IReadOnlyList<LeaseClausePersistenceModel> Items)
    {
        public static LeaseClausesPersistenceModel FromDomain(LeaseClauses leaseClauses)
        {
            var items = leaseClauses.Collection.Items
                .Select(x => new LeaseClausePersistenceModel(x.Code, x.Text))
                .ToList();

            return new LeaseClausesPersistenceModel(items);
        }
    }

    private sealed record CommercialTermsPersistenceModel(
        decimal MonthlyRent,
        string BillingFrequency,
        int RentDueDay,
        int GracePeriodDays,
        decimal DepositAmount,
        bool IsDepositRefundable,
        string SecurityDepositReference,
        string DepositRulesReference,
        bool AutoRenew,
        int RenewalNoticePeriodDays,
        string RenewalPolicyReference,
        int TerminationNoticePeriodDays,
        string TerminationPolicyReference,
        string LateFeePolicyReference)
    {
        public static CommercialTermsPersistenceModel FromDomain(CommercialTerms commercialTerms)
        {
            return new CommercialTermsPersistenceModel(
                commercialTerms.RentTerms.MonthlyRent,
                commercialTerms.RentTerms.BillingFrequency.Value,
                commercialTerms.RentTerms.RentDueDay,
                commercialTerms.RentTerms.GracePeriodDays,
                commercialTerms.DepositTerms.DepositAmount,
                commercialTerms.DepositTerms.IsRefundable,
                commercialTerms.DepositTerms.SecurityDepositReference.Value,
                commercialTerms.DepositTerms.DepositRulesReference,
                commercialTerms.RenewalTerms.AutoRenew,
                commercialTerms.RenewalTerms.NoticePeriodDays,
                commercialTerms.RenewalTerms.RenewalPolicyReference,
                commercialTerms.TerminationTerms.NoticePeriodDays,
                commercialTerms.TerminationTerms.TerminationPolicyReference,
                commercialTerms.TerminationTerms.LateFeePolicyReference);
        }
    }
}
