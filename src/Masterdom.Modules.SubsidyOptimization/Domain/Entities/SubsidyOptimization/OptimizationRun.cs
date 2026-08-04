using Masterdom.Core.Common.Events;
using Masterdom.Core.Common.Interfaces;
using Masterdom.Core.Primitives;
using Masterdom.Modules.SubsidyOptimization.Domain.Entities.SubsidyOptimization.Events;

namespace Masterdom.Modules.SubsidyOptimization.Domain.Entities.SubsidyOptimization;

public sealed class OptimizationRun : AggregateRoot<OptimizationRunId>, IHasDomainEvents
{
    private readonly List<IDomainEvent> _domainEvents = [];
    private readonly List<OptimizationRecommendation> _recommendations = [];
    private readonly List<OptimizationSnapshot> _snapshots = [];
    private readonly List<OptimizationVersionRecord> _versionHistory = [];

    private OptimizationRun(
        OptimizationRunId id,
        SubsidyScenario scenario,
        MeterGroup meterGroup,
        RatingReference ratingReference,
        OptimizationPeriod optimizationPeriod,
        OptimizationVersion optimizationVersion,
        DateTime startedAtUtc)
        : base(id)
    {
        Scenario = scenario;
        MeterGroup = meterGroup;
        RatingReference = ratingReference;
        OptimizationPeriod = optimizationPeriod;
        OptimizationVersion = optimizationVersion;
        StartedAtUtc = startedAtUtc;
        OptimizationStatus = OptimizationStatus.Started;

        _versionHistory.Add(OptimizationVersionRecord.Create(optimizationVersion, startedAtUtc));
    }

    public SubsidyScenario Scenario { get; private set; }

    public MeterGroup MeterGroup { get; private set; }

    public RatingReference RatingReference { get; private set; }

    public OptimizationPeriod OptimizationPeriod { get; private set; }

    public OptimizationStatus OptimizationStatus { get; private set; }

    public OptimizationVersion OptimizationVersion { get; private set; }

    public OptimizationResult? OptimizationResult { get; private set; }

    public ConsumptionForecast? ConsumptionForecast { get; private set; }

    public RecommendationSet? RecommendationSet => _recommendations.Count == 0
        ? null
        : RecommendationSet.Create(_recommendations);

    public DateTime StartedAtUtc { get; private set; }

    public DateTime? CompletedAtUtc { get; private set; }

    public IReadOnlyCollection<OptimizationRecommendation> Recommendations => _recommendations.AsReadOnly();

    public IReadOnlyCollection<OptimizationSnapshot> Snapshots => _snapshots.AsReadOnly();

    public IReadOnlyCollection<OptimizationVersionRecord> VersionHistory => _versionHistory.AsReadOnly();

    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    public static OptimizationRun Start(
        OptimizationRunId id,
        SubsidyScenario scenario,
        MeterGroup meterGroup,
        RatingReference ratingReference,
        OptimizationPeriod optimizationPeriod,
        DateTime startedAtUtc)
    {
        ArgumentNullException.ThrowIfNull(id);
        ArgumentNullException.ThrowIfNull(scenario);
        ArgumentNullException.ThrowIfNull(meterGroup);
        ArgumentNullException.ThrowIfNull(ratingReference);
        ArgumentNullException.ThrowIfNull(optimizationPeriod);

        if (startedAtUtc.Kind != DateTimeKind.Utc)
        {
            throw new InvalidOperationException("Optimization start timestamp must be UTC.");
        }

        var run = new OptimizationRun(
            id,
            scenario,
            meterGroup,
            ratingReference,
            optimizationPeriod,
            OptimizationVersion.Initial,
            startedAtUtc);

        run.Raise(new OptimizationStartedDomainEvent(
            run.Id,
            run.Scenario.ScenarioId.Value,
            run.OptimizationVersion.Value,
            startedAtUtc));

        return run;
    }

    public void Complete(
        OptimizationResult optimizationResult,
        ConsumptionForecast consumptionForecast,
        RecommendationSet recommendationSet,
        DateTime completedAtUtc)
    {
        EnsureActive();

        ArgumentNullException.ThrowIfNull(optimizationResult);
        ArgumentNullException.ThrowIfNull(consumptionForecast);
        ArgumentNullException.ThrowIfNull(recommendationSet);

        if (completedAtUtc.Kind != DateTimeKind.Utc)
        {
            throw new InvalidOperationException("Optimization completion timestamp must be UTC.");
        }

        if (completedAtUtc < StartedAtUtc)
        {
            throw new InvalidOperationException("Optimization completion timestamp cannot be earlier than start timestamp.");
        }

        OptimizationResult = optimizationResult;
        ConsumptionForecast = consumptionForecast;

        _recommendations.Clear();
        _recommendations.AddRange(recommendationSet.Items);

        CompletedAtUtc = completedAtUtc;
        OptimizationStatus = OptimizationStatus.Completed;

        var snapshot = OptimizationSnapshot.Create(
            OptimizationVersion,
            completedAtUtc,
            optimizationResult,
            consumptionForecast,
            recommendationSet);

        _snapshots.Add(snapshot);

        foreach (var recommendation in recommendationSet.Items)
        {
            Raise(new RecommendationGeneratedDomainEvent(
                Id,
                recommendation.RecommendationId.Value,
                recommendation.Priority.Value,
                completedAtUtc));
        }

        Raise(new OptimizationCompletedDomainEvent(
            Id,
            Scenario.ScenarioId.Value,
            OptimizationVersion.Value,
            completedAtUtc));
    }

    public OptimizationRun CreateScenarioVersion(
        DateTime startedAtUtc,
        RatingReference ratingReference)
    {
        if (startedAtUtc.Kind != DateTimeKind.Utc)
        {
            throw new InvalidOperationException("Scenario version creation timestamp must be UTC.");
        }

        ArgumentNullException.ThrowIfNull(ratingReference);

        var nextVersion = OptimizationVersion.Next();

        var next = new OptimizationRun(
            OptimizationRunId.New(),
            Scenario,
            MeterGroup,
            ratingReference,
            OptimizationPeriod,
            nextVersion,
            startedAtUtc);

        next.Raise(new ScenarioVersionCreatedDomainEvent(
            Id,
            next.Id,
            Scenario.ScenarioId.Value,
            nextVersion.Value,
            startedAtUtc));

        next.Raise(new OptimizationStartedDomainEvent(
            next.Id,
            next.Scenario.ScenarioId.Value,
            next.OptimizationVersion.Value,
            startedAtUtc));

        return next;
    }

    public void ArchiveRecommendation(RecommendationId recommendationId, string reason, DateTime archivedAtUtc)
    {
        EnsureCompleted();

        ArgumentNullException.ThrowIfNull(recommendationId);
        ArgumentException.ThrowIfNullOrWhiteSpace(reason);

        if (archivedAtUtc.Kind != DateTimeKind.Utc)
        {
            throw new InvalidOperationException("Recommendation archive timestamp must be UTC.");
        }

        if (_recommendations.Count == 0)
        {
            throw new InvalidOperationException("Recommendation set was not created for this optimization run.");
        }

        var index = _recommendations.FindIndex(x => x.RecommendationId == recommendationId);
        if (index < 0)
        {
            throw new InvalidOperationException("Recommendation was not found.");
        }

        _recommendations[index] = _recommendations[index].Archive(reason, archivedAtUtc);

        Raise(new RecommendationArchivedDomainEvent(
            Id,
            recommendationId.Value,
            archivedAtUtc));
    }

    public void ArchiveRun(DateTime archivedAtUtc)
    {
        if (archivedAtUtc.Kind != DateTimeKind.Utc)
        {
            throw new InvalidOperationException("Run archive timestamp must be UTC.");
        }

        OptimizationStatus = OptimizationStatus.Archived;
    }

    public void ClearDomainEvents()
    {
        _domainEvents.Clear();
    }

    private void EnsureActive()
    {
        if (OptimizationStatus == OptimizationStatus.Archived)
        {
            throw new InvalidOperationException("Archived optimization runs cannot be modified.");
        }

        if (OptimizationStatus == OptimizationStatus.Completed)
        {
            throw new InvalidOperationException("Completed optimization runs are immutable.");
        }
    }

    private void EnsureCompleted()
    {
        if (OptimizationStatus != OptimizationStatus.Completed)
        {
            throw new InvalidOperationException("Recommendations can only be archived after optimization completion.");
        }
    }

    private void Raise(IDomainEvent domainEvent)
    {
        _domainEvents.Add(domainEvent);
    }
}
