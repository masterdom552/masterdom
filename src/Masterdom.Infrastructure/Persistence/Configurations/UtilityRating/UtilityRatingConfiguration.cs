using System.Text.Json;
using Masterdom.Infrastructure.Persistence.Extensions;
using Masterdom.Modules.UtilityRating.Domain.Entities.UtilityRating;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UtilityRatingAggregate = Masterdom.Modules.UtilityRating.Domain.Entities.UtilityRating.UtilityRating;

namespace Masterdom.Infrastructure.Persistence.Configurations.UtilityRating;

public sealed class UtilityRatingConfiguration : IEntityTypeConfiguration<UtilityRatingAggregate>
{
    public void Configure(EntityTypeBuilder<UtilityRatingAggregate> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.ToTable("utility_ratings");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasEntityIdConversion(UtilityRatingId.From)
            .ValueGeneratedNever();

        builder.Property(x => x.MeterReference)
            .HasConversion(
                value => JsonSerializer.Serialize(new MeterReferencePersistenceModel(value.MeterId), JsonSerializerOptions.Web),
                json => DeserializeMeterReference(json))
            .HasColumnName("meter_reference")
            .HasColumnType("jsonb")
            .IsRequired();

        builder.Property(x => x.ConsumptionReference)
            .HasConversion(
                value => JsonSerializer.Serialize(new ConsumptionReferencePersistenceModel(value.ReadingId, value.ConsumptionValue), JsonSerializerOptions.Web),
                json => DeserializeConsumptionReference(json))
            .HasColumnName("consumption_reference")
            .HasColumnType("jsonb")
            .IsRequired();

        builder.Property(x => x.RatingPeriod)
            .HasConversion(
                value => JsonSerializer.Serialize(new RatingPeriodPersistenceModel(value.StartDate, value.EndDate), JsonSerializerOptions.Web),
                json => DeserializeRatingPeriod(json))
            .HasColumnName("rating_period")
            .HasColumnType("jsonb")
            .IsRequired();

        builder.Property(x => x.TariffReference)
            .HasConversion(
                value => JsonSerializer.Serialize(new TariffReferencePersistenceModel(value.TariffCode, value.TariffVersion), JsonSerializerOptions.Web),
                json => DeserializeTariffReference(json))
            .HasColumnName("tariff_reference")
            .HasColumnType("jsonb")
            .IsRequired();

        builder.Property(x => x.RatedUnits)
            .HasConversion(
                value => value.Value,
                value => RatedUnits.Create(value))
            .HasColumnName("rated_units")
            .HasColumnType("numeric")
            .IsRequired();

        builder.Property(x => x.RatedAmount)
            .HasConversion(
                value => value.Value,
                value => RatedAmount.Create(value))
            .HasColumnName("rated_amount")
            .HasColumnType("numeric(18,2)")
            .IsRequired();

        builder.Property(x => x.RatingStatus)
            .HasValueObjectConversion(RatingStatus.Create)
            .HasColumnName("rating_status")
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(x => x.RatingVersion)
            .HasConversion(
                value => value.Value,
                value => RatingVersion.Create(value))
            .HasColumnName("rating_version")
            .IsRequired();

        builder.Property(x => x.RatingResult)
            .HasConversion(
                value => JsonSerializer.Serialize(RatingResultPersistenceModel.FromDomain(value), JsonSerializerOptions.Web),
                json => DeserializeRatingResult(json))
            .HasColumnName("rating_result")
            .HasColumnType("jsonb")
            .IsRequired();

        builder.Property(x => x.RatingSnapshot)
            .HasConversion(
                value => JsonSerializer.Serialize(RatingSnapshotPersistenceModel.FromDomain(value), JsonSerializerOptions.Web),
                json => DeserializeRatingSnapshot(json))
            .HasColumnName("rating_snapshot")
            .HasColumnType("jsonb")
            .IsRequired();

        builder.Property(x => x.RatedConsumption)
            .HasConversion(
                value => JsonSerializer.Serialize(new RatedConsumptionPersistenceModel(value.RatedUnits.Value, value.RatedAmount.Value), JsonSerializerOptions.Web),
                json => DeserializeRatedConsumption(json))
            .HasColumnName("rated_consumption")
            .HasColumnType("jsonb")
            .IsRequired();

        builder.Property(x => x.UtilityRate)
            .HasConversion(
                value => JsonSerializer.Serialize(UtilityRatePersistenceModel.FromDomain(value), JsonSerializerOptions.Web),
                json => DeserializeUtilityRate(json))
            .HasColumnName("utility_rate")
            .HasColumnType("jsonb")
            .IsRequired();

        builder.Property(x => x.TariffSchedule)
            .HasConversion(
                value => JsonSerializer.Serialize(TariffSchedulePersistenceModel.FromDomain(value), JsonSerializerOptions.Web),
                json => DeserializeTariffSchedule(json))
            .HasColumnName("tariff_schedule")
            .HasColumnType("jsonb")
            .IsRequired();

        builder.Property(x => x.RatedAtUtc)
            .HasColumnName("rated_at_utc")
            .IsRequired();

        builder.HasIndex(x => x.RatedAtUtc)
            .HasDatabaseName("ix_utility_ratings_rated_at_utc");

        builder.Ignore(x => x.DomainEvents);
    }

    private static MeterReference DeserializeMeterReference(string json)
    {
        var model = JsonSerializer.Deserialize<MeterReferencePersistenceModel>(json, JsonSerializerOptions.Web)
            ?? throw new InvalidOperationException("Unable to deserialize meter reference.");

        return MeterReference.Create(model.MeterId);
    }

    private static ConsumptionReference DeserializeConsumptionReference(string json)
    {
        var model = JsonSerializer.Deserialize<ConsumptionReferencePersistenceModel>(json, JsonSerializerOptions.Web)
            ?? throw new InvalidOperationException("Unable to deserialize consumption reference.");

        return ConsumptionReference.Create(model.ReadingId, model.ConsumptionValue);
    }

    private static RatingPeriod DeserializeRatingPeriod(string json)
    {
        var model = JsonSerializer.Deserialize<RatingPeriodPersistenceModel>(json, JsonSerializerOptions.Web)
            ?? throw new InvalidOperationException("Unable to deserialize rating period.");

        return RatingPeriod.Create(model.StartDate, model.EndDate);
    }

    private static TariffReference DeserializeTariffReference(string json)
    {
        var model = JsonSerializer.Deserialize<TariffReferencePersistenceModel>(json, JsonSerializerOptions.Web)
            ?? throw new InvalidOperationException("Unable to deserialize tariff reference.");

        return TariffReference.Create(model.TariffCode, model.TariffVersion);
    }

    private static RatingResult DeserializeRatingResult(string json)
    {
        var model = JsonSerializer.Deserialize<RatingResultPersistenceModel>(json, JsonSerializerOptions.Web)
            ?? throw new InvalidOperationException("Unable to deserialize rating result.");

        var components = model.Components
            .Select(x => RateComponent.Create(x.Name, x.Amount))
            .ToList();

        var breakdown = RatingBreakdown.Create(
            components,
            RatedAmount.Create(model.Subtotal),
            RatedAmount.Create(model.Total));

        return RatingResult.Create(breakdown, model.GeneratedAtUtc);
    }

    private static RatingSnapshot DeserializeRatingSnapshot(string json)
    {
        var model = JsonSerializer.Deserialize<RatingSnapshotPersistenceModel>(json, JsonSerializerOptions.Web)
            ?? throw new InvalidOperationException("Unable to deserialize rating snapshot.");

        var consumptionSnapshot = ConsumptionSnapshot.Create(
            MeterReference.Create(model.ConsumptionSnapshot.MeterId),
            ConsumptionReference.Create(
                model.ConsumptionSnapshot.ReadingId,
                model.ConsumptionSnapshot.ConsumptionValue),
            RatingPeriod.Create(
                model.ConsumptionSnapshot.PeriodStart,
                model.ConsumptionSnapshot.PeriodEnd),
            model.ConsumptionSnapshot.CapturedAtUtc);

        var tariffSchedule = DeserializeTariffSchedule(JsonSerializer.Serialize(model.TariffSchedule, JsonSerializerOptions.Web));

        return RatingSnapshot.Create(consumptionSnapshot, tariffSchedule, model.CapturedAtUtc);
    }

    private static RatedConsumption DeserializeRatedConsumption(string json)
    {
        var model = JsonSerializer.Deserialize<RatedConsumptionPersistenceModel>(json, JsonSerializerOptions.Web)
            ?? throw new InvalidOperationException("Unable to deserialize rated consumption.");

        return RatedConsumption.Create(
            RatedUnits.Create(model.RatedUnits),
            RatedAmount.Create(model.RatedAmount));
    }

    private static UtilityRate DeserializeUtilityRate(string json)
    {
        var model = JsonSerializer.Deserialize<UtilityRatePersistenceModel>(json, JsonSerializerOptions.Web)
            ?? throw new InvalidOperationException("Unable to deserialize utility rate.");

        var tariffReference = TariffReference.Create(model.TariffCode, model.TariffVersion);

        return UtilityRate.Create(
            tariffReference,
            FixedCharge.Create(model.FixedCharge),
            VariableCharge.Create(model.VariableRatePerUnit),
            MinimumCharge.Create(model.MinimumCharge),
            AdjustmentComponent.Create(model.Adjustment));
    }

    private static TariffSchedule DeserializeTariffSchedule(string json)
    {
        var model = JsonSerializer.Deserialize<TariffSchedulePersistenceModel>(json, JsonSerializerOptions.Web)
            ?? throw new InvalidOperationException("Unable to deserialize tariff schedule.");

        var utilityRate = UtilityRate.Create(
            TariffReference.Create(model.UtilityRate.TariffCode, model.UtilityRate.TariffVersion),
            FixedCharge.Create(model.UtilityRate.FixedCharge),
            VariableCharge.Create(model.UtilityRate.VariableRatePerUnit),
            MinimumCharge.Create(model.UtilityRate.MinimumCharge),
            AdjustmentComponent.Create(model.UtilityRate.Adjustment));

        return TariffSchedule.Create(
            TariffReference.Create(model.TariffCode, model.TariffVersion),
            model.EffectiveFrom,
            model.EffectiveTo,
            utilityRate);
    }

    private sealed record MeterReferencePersistenceModel(Guid MeterId);

    private sealed record ConsumptionReferencePersistenceModel(Guid ReadingId, decimal ConsumptionValue);

    private sealed record RatingPeriodPersistenceModel(DateOnly StartDate, DateOnly EndDate);

    private sealed record TariffReferencePersistenceModel(string TariffCode, int TariffVersion);

    private sealed record RateComponentPersistenceModel(string Name, decimal Amount);

    private sealed record RatingResultPersistenceModel(
        DateTime GeneratedAtUtc,
        decimal Subtotal,
        decimal Total,
        IReadOnlyList<RateComponentPersistenceModel> Components)
    {
        public static RatingResultPersistenceModel FromDomain(RatingResult result)
        {
            var components = result.Breakdown.Components
                .Select(x => new RateComponentPersistenceModel(x.Name, x.Amount))
                .ToList();

            return new RatingResultPersistenceModel(
                result.GeneratedAtUtc,
                result.Breakdown.Subtotal.Value,
                result.Breakdown.Total.Value,
                components);
        }
    }

    private sealed record ConsumptionSnapshotPersistenceModel(
        Guid MeterId,
        Guid ReadingId,
        decimal ConsumptionValue,
        DateOnly PeriodStart,
        DateOnly PeriodEnd,
        DateTime CapturedAtUtc)
    {
        public static ConsumptionSnapshotPersistenceModel FromDomain(ConsumptionSnapshot snapshot)
        {
            return new ConsumptionSnapshotPersistenceModel(
                snapshot.MeterReference.MeterId,
                snapshot.ConsumptionReference.ReadingId,
                snapshot.ConsumptionReference.ConsumptionValue,
                snapshot.RatingPeriod.StartDate,
                snapshot.RatingPeriod.EndDate,
                snapshot.CapturedAtUtc);
        }
    }

    private sealed record UtilityRatePersistenceModel(
        string TariffCode,
        int TariffVersion,
        decimal FixedCharge,
        decimal VariableRatePerUnit,
        decimal MinimumCharge,
        decimal Adjustment)
    {
        public static UtilityRatePersistenceModel FromDomain(UtilityRate rate)
        {
            return new UtilityRatePersistenceModel(
                rate.TariffReference.TariffCode,
                rate.TariffReference.TariffVersion,
                rate.FixedCharge.Amount,
                rate.VariableCharge.RatePerUnit,
                rate.MinimumCharge.Amount,
                rate.AdjustmentComponent.Amount);
        }
    }

    private sealed record TariffSchedulePersistenceModel(
        string TariffCode,
        int TariffVersion,
        DateOnly EffectiveFrom,
        DateOnly? EffectiveTo,
        UtilityRatePersistenceModel UtilityRate)
    {
        public static TariffSchedulePersistenceModel FromDomain(TariffSchedule schedule)
        {
            return new TariffSchedulePersistenceModel(
                schedule.TariffReference.TariffCode,
                schedule.TariffReference.TariffVersion,
                schedule.EffectiveFrom,
                schedule.EffectiveTo,
                UtilityRatePersistenceModel.FromDomain(schedule.UtilityRate));
        }
    }

    private sealed record RatingSnapshotPersistenceModel(
        DateTime CapturedAtUtc,
        ConsumptionSnapshotPersistenceModel ConsumptionSnapshot,
        TariffSchedulePersistenceModel TariffSchedule)
    {
        public static RatingSnapshotPersistenceModel FromDomain(RatingSnapshot snapshot)
        {
            return new RatingSnapshotPersistenceModel(
                snapshot.CapturedAtUtc,
                ConsumptionSnapshotPersistenceModel.FromDomain(snapshot.ConsumptionSnapshot),
                TariffSchedulePersistenceModel.FromDomain(snapshot.TariffSchedule));
        }
    }

    private sealed record RatedConsumptionPersistenceModel(decimal RatedUnits, decimal RatedAmount);
}
