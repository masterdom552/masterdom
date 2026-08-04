using Masterdom.Modules.FinancialLedger.Application.Commands;
using Masterdom.Modules.FinancialLedger.Application.Queries;
using Masterdom.Modules.FinancialLedger.Application.Support;
using Masterdom.Modules.FinancialLedger.Domain.Entities.FinancialLedger;
using Masterdom.Modules.FinancialLedger.Domain.Repositories;
using LedgerAggregate = Masterdom.Modules.FinancialLedger.Domain.Entities.FinancialLedger.Ledger;

namespace Masterdom.Modules.FinancialLedger.Application.Services;

public sealed class LedgerApplicationService : ILedgerApplicationService
{
    private readonly ILedgerRepository _repository;
    private readonly ILedgerUnitOfWork _unitOfWork;
    private readonly ILedgerPlatformOrchestrator _platformOrchestrator;

    public LedgerApplicationService(
        ILedgerRepository repository,
        ILedgerUnitOfWork unitOfWork,
        ILedgerPlatformOrchestrator platformOrchestrator)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _platformOrchestrator = platformOrchestrator ?? throw new ArgumentNullException(nameof(platformOrchestrator));
    }

    public LedgerAggregate OpenLedger(OpenLedgerCommand command)
    {
        ArgumentNullException.ThrowIfNull(command);

        var existing = _repository.GetByCode(command.LedgerCode);
        if (existing is not null)
        {
            throw new InvalidOperationException($"Ledger '{command.LedgerCode}' already exists.");
        }

        var ledger = LedgerAggregate.Open(LedgerId.New(), command.LedgerCode, command.LedgerName, command.CreatedAtUtc);

        _unitOfWork.Execute(() => _repository.Add(ledger));
        _platformOrchestrator.OnLedgerMutated(ledger, "OpenLedger");

        return ledger;
    }

    public LedgerAggregate PostBillingJournal(PostBillingJournalCommand command)
    {
        ArgumentNullException.ThrowIfNull(command);

        var ledger = GetRequiredLedger(command.LedgerId);
        ledger.PostBillingTransaction(command.Contract, command.PostedAtUtc);

        _unitOfWork.Execute(() => _repository.Update(ledger));
        _platformOrchestrator.OnLedgerMutated(ledger, "PostBillingJournal");

        return ledger;
    }

    public LedgerAggregate PostPaymentJournal(PostPaymentJournalCommand command)
    {
        ArgumentNullException.ThrowIfNull(command);

        var ledger = GetRequiredLedger(command.LedgerId);
        ledger.PostPaymentTransaction(command.Contract, command.PostedAtUtc);

        _unitOfWork.Execute(() => _repository.Update(ledger));
        _platformOrchestrator.OnLedgerMutated(ledger, "PostPaymentJournal");

        return ledger;
    }

    public LedgerAggregate ReverseJournal(ReverseJournalCommand command)
    {
        ArgumentNullException.ThrowIfNull(command);

        var ledger = GetRequiredLedger(command.LedgerId);
        ledger.ReverseJournal(command.LedgerTransactionId, command.ReversalJournalNumber, command.Reason, command.ReversedAtUtc);

        _unitOfWork.Execute(() => _repository.Update(ledger));
        _platformOrchestrator.OnLedgerMutated(ledger, "ReverseJournal");

        return ledger;
    }

    public LedgerAggregate CompletePostingBatch(CompletePostingBatchCommand command)
    {
        ArgumentNullException.ThrowIfNull(command);

        var ledger = GetRequiredLedger(command.LedgerId);
        ledger.CompletePostingBatch(command.BatchReference, command.CompletedAtUtc);

        _unitOfWork.Execute(() => _repository.Update(ledger));
        _platformOrchestrator.OnLedgerMutated(ledger, "CompletePostingBatch");

        return ledger;
    }

    public LedgerAggregate? GetLedger(GetLedgerByIdQuery query)
    {
        ArgumentNullException.ThrowIfNull(query);
        return _repository.GetById(query.LedgerId);
    }

    public LedgerAggregate? GetLedger(GetLedgerByCodeQuery query)
    {
        ArgumentNullException.ThrowIfNull(query);
        return _repository.GetByCode(query.LedgerCode);
    }

    private LedgerAggregate GetRequiredLedger(LedgerId ledgerId)
    {
        var ledger = _repository.GetById(ledgerId);
        if (ledger is null)
        {
            throw new InvalidOperationException($"Ledger '{ledgerId}' was not found.");
        }

        return ledger;
    }
}
