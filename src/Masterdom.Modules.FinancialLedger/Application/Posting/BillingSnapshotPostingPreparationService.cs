using Masterdom.Modules.Billing.Contracts.Published.Models;
using Masterdom.Modules.FinancialLedger.Application.Translation;

namespace Masterdom.Modules.FinancialLedger.Application.Posting;

internal sealed class BillingSnapshotPostingPreparationService
{
    private readonly BillingSnapshotTranslator _translator;
    private readonly BillingSnapshotPostingValidator _validator;
    private readonly PostingLineGenerator _lineGenerator;
    private readonly JournalPreparationService _journalPreparationService;
    private readonly BillingFinancialPostingRequestFactory _requestFactory;
    private readonly LegacyPostingAdapter _legacyAdapter;

    public BillingSnapshotPostingPreparationService(
        BillingSnapshotTranslator translator,
        BillingSnapshotPostingValidator validator,
        PostingLineGenerator lineGenerator,
        JournalPreparationService journalPreparationService,
        BillingFinancialPostingRequestFactory requestFactory,
        LegacyPostingAdapter legacyAdapter)
    {
        _translator = translator ?? throw new ArgumentNullException(nameof(translator));
        _validator = validator ?? throw new ArgumentNullException(nameof(validator));
        _lineGenerator = lineGenerator ?? throw new ArgumentNullException(nameof(lineGenerator));
        _journalPreparationService = journalPreparationService ?? throw new ArgumentNullException(nameof(journalPreparationService));
        _requestFactory = requestFactory ?? throw new ArgumentNullException(nameof(requestFactory));
        _legacyAdapter = legacyAdapter ?? throw new ArgumentNullException(nameof(legacyAdapter));
    }

    public BillingSnapshotPostingPreparationResult Prepare(
        BillSnapshotModel billSnapshot,
        DateTimeOffset occurredAtUtc,
        DateOnly postingDate,
        string? batchReference = null)
    {
        ArgumentNullException.ThrowIfNull(billSnapshot);

        var source = _translator.Translate(billSnapshot);
        var validationResult = _validator.Validate(source);

        if (!validationResult.IsValid)
        {
            throw new InvalidOperationException($"Invalid billing snapshot posting input: {string.Join(" | ", validationResult.Errors)}");
        }

        var generatedLines = _lineGenerator.Generate(source);
        var preparedJournal = _journalPreparationService.Prepare(source, generatedLines, postingDate, batchReference);
        var postingRequest = _requestFactory.Create(source, generatedLines, occurredAtUtc);
        var legacyContract = _legacyAdapter.Adapt(source, generatedLines, postingDate, batchReference);

        return new BillingSnapshotPostingPreparationResult(postingRequest, legacyContract, generatedLines, preparedJournal);
    }
}
