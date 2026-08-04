using System.Text.Json;
using Masterdom.Abstractions.Financial.Posting;
using Masterdom.Modules.FinancialLedger.Application.Posting;
using Masterdom.Modules.FinancialLedger.Domain.Entities.FinancialLedger;

namespace Masterdom.Infrastructure.Persistence.FinancialLedger;

internal sealed class PersistedPreparedJournalRepository : IPersistedPreparedJournalRepository
{
    private static readonly JsonSerializerOptions SerializerOptions = JsonSerializerOptions.Web;
    private readonly MasterdomDbContext _dbContext;

    public PersistedPreparedJournalRepository(MasterdomDbContext dbContext)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
    }

    public PersistedPreparedJournal? GetByPostingReference(LedgerId ledgerId, string postingReference)
    {
        ArgumentNullException.ThrowIfNull(ledgerId);
        ArgumentException.ThrowIfNullOrWhiteSpace(postingReference);

        var normalized = postingReference.Trim();
        var entity = _dbContext.Set<PersistedPreparedJournalEntity>()
            .FirstOrDefault(x => x.LedgerId == ledgerId.Value && x.PostingReference == normalized);

        return entity is null ? null : MapToDomain(entity);
    }

    public void Add(PersistedPreparedJournal journal)
    {
        ArgumentNullException.ThrowIfNull(journal);
        _dbContext.Set<PersistedPreparedJournalEntity>().Add(MapToEntity(journal));
    }

    public void Update(PersistedPreparedJournal journal)
    {
        ArgumentNullException.ThrowIfNull(journal);

        var existing = _dbContext.Set<PersistedPreparedJournalEntity>()
            .FirstOrDefault(x => x.Id == journal.PersistenceId);

        if (existing is null)
        {
            throw new InvalidOperationException($"Prepared journal persistence record '{journal.PersistenceId}' was not found.");
        }

        Apply(existing, journal);
    }

    private static PersistedPreparedJournalEntity MapToEntity(PersistedPreparedJournal journal)
    {
        var entity = new PersistedPreparedJournalEntity
        {
            Id = journal.PersistenceId,
            LedgerId = journal.LedgerId.Value
        };

        Apply(entity, journal);
        return entity;
    }

    private static void Apply(PersistedPreparedJournalEntity entity, PersistedPreparedJournal journal)
    {
        entity.PostingReference = journal.PostingReference;
        entity.JournalReference = journal.JournalReference;
        entity.JournalNumber = journal.JournalNumber;
        entity.PostingDate = journal.PostingDate;
        entity.CurrencyCode = journal.PreparedJournal.CurrencyCode;
        entity.Description = journal.PreparedJournal.Description;
        entity.BatchReference = journal.PreparedJournal.BatchReference;
        entity.SourceModule = journal.PreparedJournal.SourceModule;
        entity.BillId = journal.PreparedJournal.BillId;
        entity.BillNumber = journal.PreparedJournal.BillNumber;
        entity.DebitTotal = journal.PreparedJournal.DebitTotal;
        entity.CreditTotal = journal.PreparedJournal.CreditTotal;
        entity.State = journal.State;
        entity.CreatedAtUtc = journal.CreatedAtUtc;
        entity.ValidatedAtUtc = journal.ValidatedAtUtc;
        entity.PostedAtUtc = journal.PostedAtUtc;
        entity.ReversedAtUtc = journal.ReversedAtUtc;
        entity.CancelledAtUtc = journal.CancelledAtUtc;
        entity.CancellationReason = journal.CancellationReason;
        entity.LedgerTransactionId = journal.LedgerTransactionId;
        entity.LinesJson = SerializeLines(journal.PreparedJournal.Lines);
        entity.MetadataJson = JsonSerializer.Serialize(journal.PreparedJournal.Metadata, SerializerOptions);
    }

    private static PersistedPreparedJournal MapToDomain(PersistedPreparedJournalEntity entity)
    {
        var lines = DeserializeLines(entity.LinesJson);
        var metadata = JsonSerializer.Deserialize<Dictionary<string, string>>(entity.MetadataJson, SerializerOptions)
            ?? new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        var state = ParseState(entity.State);

        var preparedJournal = new PreparedJournal(
            entity.JournalReference,
            entity.PostingReference,
            entity.JournalNumber,
            entity.PostingDate,
            entity.CurrencyCode,
            entity.Description,
            entity.BatchReference,
            entity.SourceModule,
            entity.BillId,
            entity.BillNumber,
            lines,
            state,
            entity.ValidatedAtUtc,
            entity.PostedAtUtc,
            entity.ReversedAtUtc,
            entity.CancelledAtUtc,
            entity.CancellationReason,
            metadata);

        return PersistedPreparedJournal.Rehydrate(
            entity.Id,
            LedgerId.From(entity.LedgerId),
            entity.PostingReference,
            entity.JournalReference,
            entity.JournalNumber,
            entity.PostingDate,
            entity.State,
            DateTime.SpecifyKind(entity.CreatedAtUtc, DateTimeKind.Utc),
            entity.ValidatedAtUtc,
            entity.PostedAtUtc,
            entity.ReversedAtUtc,
            entity.CancelledAtUtc,
            entity.CancellationReason,
            entity.LedgerTransactionId,
            preparedJournal);
    }

    private static JournalLifecycleState ParseState(string state)
    {
        if (Enum.TryParse<JournalLifecycleState>(state, ignoreCase: true, out var parsed))
        {
            return parsed;
        }

        throw new InvalidOperationException($"Unsupported journal lifecycle state '{state}'.");
    }

    private static string SerializeLines(IReadOnlyCollection<PreparedJournalLine> lines)
    {
        var payload = lines.Select(x => new PersistedPreparedJournalLineModel(
            x.LineId,
            x.AccountCode,
            x.AccountName,
            x.Direction.ToString(),
            x.Amount,
            x.CurrencyCode,
            x.Description,
            x.Metadata)).ToList();

        return JsonSerializer.Serialize(payload, SerializerOptions);
    }

    private static IReadOnlyCollection<PreparedJournalLine> DeserializeLines(string json)
    {
        var payload = JsonSerializer.Deserialize<List<PersistedPreparedJournalLineModel>>(json, SerializerOptions)
            ?? [];

        return payload
            .Select(x => new PreparedJournalLine(
                x.LineId,
                x.AccountCode,
                x.AccountName,
                Enum.TryParse<FinancialPostingDirection>(x.Direction, ignoreCase: true, out var parsedDirection)
                    ? parsedDirection
                    : throw new InvalidOperationException($"Unsupported posting direction '{x.Direction}'."),
                x.Amount,
                x.CurrencyCode,
                x.Description,
                x.Metadata))
            .ToList()
            .AsReadOnly();
    }

    private sealed record PersistedPreparedJournalLineModel(
        string LineId,
        string AccountCode,
        string AccountName,
        string Direction,
        decimal Amount,
        string CurrencyCode,
        string Description,
        IReadOnlyDictionary<string, string> Metadata);
}
