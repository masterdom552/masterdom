using System.Text.Json;
using Masterdom.Infrastructure.Persistence.Extensions;
using Masterdom.Modules.Metering.Domain.Entities.Metering;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Masterdom.Infrastructure.Persistence.Configurations.Metering;

public sealed class MeterConfiguration : IEntityTypeConfiguration<Meter>
{
    public void Configure(EntityTypeBuilder<Meter> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.ToTable("meters");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasEntityIdConversion(MeterId.From)
            .ValueGeneratedNever();

        builder.Property(x => x.MeterNumber)
            .HasValueObjectConversion(MeterNumber.Create)
            .HasColumnName("meter_number")
            .HasMaxLength(50)
            .IsRequired();

        builder.HasIndex(x => x.MeterNumber)
            .IsUnique();

        builder.Property(x => x.MeterCategory)
            .HasValueObjectConversion(MeterCategory.Create)
            .HasColumnName("meter_category")
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(x => x.MeterType)
            .HasValueObjectConversion(MeterType.Create)
            .HasColumnName("meter_type")
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(x => x.MeterStatus)
            .HasValueObjectConversion(MeterStatus.Create)
            .HasColumnName("meter_status")
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(x => x.MeterLocationReference)
            .HasConversion(
                value => JsonSerializer.Serialize(
                    new MeterLocationPersistenceModel(value.PropertyId, value.UnitId),
                    JsonSerializerOptions.Web),
                json => DeserializeMeterLocation(json))
            .HasColumnName("meter_location")
            .HasColumnType("jsonb")
            .IsRequired();

        builder.Property(x => x.InstallationDate)
            .HasConversion(
                value => value.Value,
                value => InstallationDate.Create(value))
            .HasColumnName("installation_date")
            .IsRequired();

        builder.Property(x => x.RemovalDate)
            .HasConversion(
                value => value == null ? (DateOnly?)null : value.Value,
                value => value.HasValue ? RemovalDate.Create(value.Value) : null)
            .HasColumnName("removal_date");

        builder.OwnsMany(x => x.HistoricalReadings, readingBuilder =>
        {
            readingBuilder.ToTable("meter_readings");

            readingBuilder.WithOwner()
                .HasForeignKey("meter_id");

            readingBuilder.Property<int>("id");
            readingBuilder.HasKey("id");

            readingBuilder.Property(x => x.ReadingId)
                .HasColumnName("reading_id")
                .IsRequired();

            readingBuilder.Property(x => x.ReadingDate)
                .HasConversion(
                    value => value.Value,
                    value => ReadingDate.Create(value))
                .HasColumnName("reading_date")
                .IsRequired();

            readingBuilder.Property(x => x.ReadingValue)
                .HasConversion(
                    value => value.Value,
                    value => ReadingValue.Create(value))
                .HasColumnName("reading_value")
                .IsRequired();

            readingBuilder.Property(x => x.ReadingSource)
                .HasValueObjectConversion(ReadingSource.Create)
                .HasColumnName("reading_source")
                .HasMaxLength(50)
                .IsRequired();

            readingBuilder.Property(x => x.SubmittedBy)
                .HasConversion(
                    value => value.Value,
                    value => SubmittedBy.Create(value))
                .HasColumnName("submitted_by")
                .HasMaxLength(100)
                .IsRequired();

            readingBuilder.Property(x => x.SubmittedAtUtc)
                .HasColumnName("submitted_at_utc")
                .IsRequired();

            readingBuilder.Property(x => x.ReadingStatus)
                .HasValueObjectConversion(ReadingStatus.Create)
                .HasColumnName("reading_status")
                .HasMaxLength(50)
                .IsRequired();

            readingBuilder.Property(x => x.ApprovalStatus)
                .HasValueObjectConversion(ApprovalStatus.Create)
                .HasColumnName("approval_status")
                .HasMaxLength(50)
                .IsRequired();

            readingBuilder.Property(x => x.IsRollover)
                .HasColumnName("is_rollover")
                .IsRequired();

            readingBuilder.Property(x => x.Consumption)
                .HasConversion(
                    value => value == null ? (decimal?)null : value.Value,
                    value => value.HasValue ? Consumption.Create(value.Value) : null)
                .HasColumnName("consumption");

            readingBuilder.Property(x => x.ReviewedBy)
                .HasConversion(
                    value => value == null ? null : value.Value,
                    value => string.IsNullOrWhiteSpace(value) ? null : ReviewedBy.Create(value))
                .HasColumnName("reviewed_by")
                .HasMaxLength(100);

            readingBuilder.Property(x => x.ReviewDate)
                .HasConversion(
                    value => value == null ? (DateTime?)null : value.ValueUtc,
                    value => value.HasValue ? ReviewDate.Create(value.Value) : null)
                .HasColumnName("review_date");

            readingBuilder.Property(x => x.ReadingNotes)
                .HasConversion(
                    value => value == null ? null : value.Value,
                    value => string.IsNullOrWhiteSpace(value) ? null : ReadingNotes.Create(value))
                .HasColumnName("reading_notes")
                .HasMaxLength(500);

            readingBuilder.Property(x => x.CorrectionHistory)
                .HasConversion(
                    value => JsonSerializer.Serialize(CorrectionHistoryPersistenceModel.FromDomain(value), JsonSerializerOptions.Web),
                    json => DeserializeCorrectionHistory(json))
                .HasColumnName("correction_history")
                .HasColumnType("jsonb")
                .IsRequired();

            readingBuilder.Property(x => x.Snapshot)
                .HasConversion(
                    value => JsonSerializer.Serialize(ReadingSnapshotPersistenceModel.FromDomain(value), JsonSerializerOptions.Web),
                    json => DeserializeReadingSnapshot(json))
                .HasColumnName("reading_snapshot")
                .HasColumnType("jsonb")
                .IsRequired();

            readingBuilder.HasIndex("meter_id", nameof(MeterReading.ReadingDate))
                .HasDatabaseName("ix_meter_readings_meter_reading_date");

            readingBuilder.HasIndex(nameof(MeterReading.ReadingId))
                .HasDatabaseName("ix_meter_readings_reading_id");
        });

        builder.Navigation(x => x.HistoricalReadings)
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.Ignore(x => x.CurrentReading);
        builder.Ignore(x => x.DomainEvents);
    }

    private static MeterLocationReference DeserializeMeterLocation(string json)
    {
        var model = JsonSerializer.Deserialize<MeterLocationPersistenceModel>(json, JsonSerializerOptions.Web)
            ?? throw new InvalidOperationException("Unable to deserialize meter location reference.");

        return MeterLocationReference.Create(model.PropertyId, model.UnitId);
    }

    private static CorrectionHistory DeserializeCorrectionHistory(string json)
    {
        var model = JsonSerializer.Deserialize<CorrectionHistoryPersistenceModel>(json, JsonSerializerOptions.Web)
            ?? throw new InvalidOperationException("Unable to deserialize correction history.");

        var corrections = model.Items
            .Select(x => ReadingCorrection.Create(
                x.PreviousValue,
                x.CorrectedValue,
                x.Reason,
                SubmittedBy.Create(x.CorrectedBy),
                x.CorrectedAtUtc))
            .ToList();

        return CorrectionHistory.Create(corrections);
    }

    private static ReadingSnapshot DeserializeReadingSnapshot(string json)
    {
        var model = JsonSerializer.Deserialize<ReadingSnapshotPersistenceModel>(json, JsonSerializerOptions.Web)
            ?? throw new InvalidOperationException("Unable to deserialize reading snapshot.");

        return ReadingSnapshot.Create(
            model.CapturedAtUtc,
            ReadingStatus.Create(model.ReadingStatus),
            ApprovalStatus.Create(model.ApprovalStatus));
    }

    private sealed record MeterLocationPersistenceModel(Guid PropertyId, Guid UnitId);

    private sealed record ReadingCorrectionPersistenceModel(
        decimal PreviousValue,
        decimal CorrectedValue,
        string Reason,
        string CorrectedBy,
        DateTime CorrectedAtUtc);

    private sealed record CorrectionHistoryPersistenceModel(IReadOnlyList<ReadingCorrectionPersistenceModel> Items)
    {
        public static CorrectionHistoryPersistenceModel FromDomain(CorrectionHistory history)
        {
            var items = history.Items
                .Select(x => new ReadingCorrectionPersistenceModel(
                    x.PreviousValue,
                    x.CorrectedValue,
                    x.Reason,
                    x.CorrectedBy.Value,
                    x.CorrectedAtUtc))
                .ToList();

            return new CorrectionHistoryPersistenceModel(items);
        }
    }

    private sealed record ReadingSnapshotPersistenceModel(
        DateTime CapturedAtUtc,
        string ReadingStatus,
        string ApprovalStatus)
    {
        public static ReadingSnapshotPersistenceModel FromDomain(ReadingSnapshot snapshot)
        {
            return new ReadingSnapshotPersistenceModel(
                snapshot.CapturedAtUtc,
                snapshot.ReadingStatus.Value,
                snapshot.ApprovalStatus.Value);
        }
    }
}
