using System.Text.Json;
using Masterdom.Core.Financial.ValueObjects;
using Masterdom.Core.Identifiers;
using Masterdom.Infrastructure.Persistence.Extensions;
using Masterdom.Modules.Billing.Domain.Entities.Billing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using BillAggregate = Masterdom.Modules.Billing.Domain.Entities.Billing.Bill;

namespace Masterdom.Infrastructure.Persistence.Configurations.Billing;

/// <summary>
/// EF Core configuration for bill aggregate.
/// </summary>
public sealed class BillConfiguration : IEntityTypeConfiguration<BillAggregate>
{
    public void Configure(EntityTypeBuilder<BillAggregate> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.ToTable("bills");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasEntityIdConversion(BillId.From)
            .ValueGeneratedNever();

        builder.Property(x => x.BillNumber)
            .HasValueObjectConversion(BillNumber.Create)
            .HasColumnName("bill_number")
            .HasMaxLength(50)
            .IsRequired();

        builder.HasIndex(x => x.BillNumber)
            .IsUnique();

        builder.Property(x => x.Status)
            .HasValueObjectConversion(BillStatus.Create)
            .HasColumnName("status")
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(x => x.TenancyReference)
            .HasConversion(
                value => value.TenancyId,
                value => TenancyReference.Create(value))
            .HasColumnName("tenancy_id")
            .IsRequired();

        builder.Property(x => x.LeaseReference)
            .HasConversion(
                value => value.LeaseId,
                value => LeaseReference.Create(value))
            .HasColumnName("lease_id")
            .IsRequired();

        builder.Property(x => x.PropertyReference)
            .HasConversion(
                value => value.PropertyId,
                value => PropertyReference.Create(value))
            .HasColumnName("property_id")
            .IsRequired();

        builder.Property(x => x.BilledParty)
            .HasConversion(
                value => value.PersonId.Value,
                value => PersonReference.Create(PersonId.From(value)))
            .HasColumnName("billed_party_id")
            .IsRequired();

        builder.OwnsMany(x => x.Versions, versionBuilder =>
        {
            versionBuilder.ToTable("bill_versions");

            versionBuilder.WithOwner()
                .HasForeignKey("bill_id");

            versionBuilder.Property<int>("id");
            versionBuilder.HasKey("id");

            versionBuilder.Property(x => x.CreatedAt)
                .HasColumnName("created_at")
                .IsRequired();

            versionBuilder.Property(x => x.Snapshot)
                .HasConversion(
                    value => JsonSerializer.Serialize(
                        BillSnapshotPersistenceModel.FromDomain(value),
                        JsonSerializerOptions.Web),
                    json => DeserializeBillSnapshot(json))
                .HasColumnName("snapshot")
                .HasColumnType("jsonb")
                .IsRequired();

            versionBuilder.HasIndex("bill_id", nameof(BillingVersion.CreatedAt))
                .HasDatabaseName("ix_bill_versions_bill_created_at");
        });

        builder.Navigation(x => x.Versions)
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.Ignore(x => x.DomainEvents);
    }

    private static BillSnapshot DeserializeBillSnapshot(string json)
    {
        var model = JsonSerializer.Deserialize<BillSnapshotPersistenceModel>(json, JsonSerializerOptions.Web)
            ?? throw new InvalidOperationException("Unable to deserialize bill snapshot.");

        var charges = model.Charges
            .Select(x => ChargeLine.Create(
                ChargeKind.Create(x.Kind),
                x.Description,
                x.Amount,
                x.ExternalReference))
            .ToList();

        var adjustments = model.Adjustments
            .Select(x => AdjustmentLine.Create(
                AdjustmentKind.Create(x.Kind),
                x.Description,
                x.Amount))
            .ToList();

        var credits = model.Credits
            .Select(x => CreditLine.Create(
                x.Description,
                x.Amount,
                x.SourceReference))
            .ToList();

        return BillSnapshot.Create(
            SnapshotVersion.Create(model.Version),
            BillingPeriod.Create(model.PeriodStartDate, model.PeriodEndDate),
            BillingCycle.Create(model.BillingCycle),
            GeneratedDate.Create(model.GeneratedDate),
            IssueDate.Create(model.IssueDate),
            DueDate.Create(model.DueDate),
            Currency.Create(model.CurrencyCode),
            ChargeCollection.Create(charges),
            AdjustmentCollection.Create(adjustments),
            CreditCollection.Create(credits));
    }

    private sealed record ChargeLinePersistenceModel(
        string Kind,
        string Description,
        decimal Amount,
        string? ExternalReference);

    private sealed record AdjustmentLinePersistenceModel(
        string Kind,
        string Description,
        decimal Amount);

    private sealed record CreditLinePersistenceModel(
        string Description,
        decimal Amount,
        string? SourceReference);

    private sealed record BillSnapshotPersistenceModel(
        int Version,
        DateOnly PeriodStartDate,
        DateOnly PeriodEndDate,
        string BillingCycle,
        DateOnly GeneratedDate,
        DateOnly IssueDate,
        DateOnly DueDate,
        string CurrencyCode,
        IReadOnlyList<ChargeLinePersistenceModel> Charges,
        IReadOnlyList<AdjustmentLinePersistenceModel> Adjustments,
        IReadOnlyList<CreditLinePersistenceModel> Credits)
    {
        public static BillSnapshotPersistenceModel FromDomain(BillSnapshot snapshot)
        {
            var charges = snapshot.Charges.Items
                .Select(x => new ChargeLinePersistenceModel(
                    x.Kind.Value,
                    x.Description,
                    x.Amount,
                    x.ExternalReference))
                .ToList();

            var adjustments = snapshot.Adjustments.Items
                .Select(x => new AdjustmentLinePersistenceModel(
                    x.Kind.Value,
                    x.Description,
                    x.Amount))
                .ToList();

            var credits = snapshot.Credits.Items
                .Select(x => new CreditLinePersistenceModel(
                    x.Description,
                    x.Amount,
                    x.SourceReference))
                .ToList();

            return new BillSnapshotPersistenceModel(
                snapshot.Version.Value,
                snapshot.BillingPeriod.StartDate,
                snapshot.BillingPeriod.EndDate,
                snapshot.BillingCycle.Value,
                snapshot.GeneratedDate.Value,
                snapshot.IssueDate.Value,
                snapshot.DueDate.Value,
                snapshot.Currency.Code,
                charges,
                adjustments,
                credits);
        }
    }
}
