using Masterdom.Modules.FinancialLedger.Application.Commands;
using Masterdom.Modules.FinancialLedger.Application.Services;
using Masterdom.Modules.FinancialLedger.Application.Support;
using LedgerAggregate = Masterdom.Modules.FinancialLedger.Domain.Entities.FinancialLedger.Ledger;

namespace Masterdom.Modules.FinancialLedger.Application.Handlers.Commands;

public sealed class CompletePostingBatchCommandHandler : ICommandHandler<CompletePostingBatchCommand, ExecutionResult<LedgerAggregate>>
{
    private readonly ILedgerApplicationService _applicationService;

    public CompletePostingBatchCommandHandler(ILedgerApplicationService applicationService)
    {
        _applicationService = applicationService ?? throw new ArgumentNullException(nameof(applicationService));
    }

    public ExecutionResult<LedgerAggregate> Handle(CompletePostingBatchCommand command)
    {
        ArgumentNullException.ThrowIfNull(command);

        try
        {
            var ledger = _applicationService.CompletePostingBatch(command);
            return ExecutionResult<LedgerAggregate>.Success(ledger);
        }
        catch (ArgumentException ex)
        {
            return ExecutionResult<LedgerAggregate>.Failure("validation_failed", ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            return ExecutionResult<LedgerAggregate>.Failure("conflict", ex.Message);
        }
    }
}
