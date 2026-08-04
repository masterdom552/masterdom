using Masterdom.Modules.FinancialLedger.Application.Queries;
using Masterdom.Modules.FinancialLedger.Application.Services;
using Masterdom.Modules.FinancialLedger.Application.Support;
using LedgerAggregate = Masterdom.Modules.FinancialLedger.Domain.Entities.FinancialLedger.Ledger;

namespace Masterdom.Modules.FinancialLedger.Application.Handlers.Queries;

public sealed class GetLedgerByIdQueryHandler : IQueryHandler<GetLedgerByIdQuery, ExecutionResult<LedgerAggregate>>
{
    private readonly ILedgerApplicationService _applicationService;

    public GetLedgerByIdQueryHandler(ILedgerApplicationService applicationService)
    {
        _applicationService = applicationService ?? throw new ArgumentNullException(nameof(applicationService));
    }

    public ExecutionResult<LedgerAggregate> Handle(GetLedgerByIdQuery query)
    {
        ArgumentNullException.ThrowIfNull(query);

        var ledger = _applicationService.GetLedger(query);
        return ledger is null
            ? ExecutionResult<LedgerAggregate>.Failure("not_found", $"Ledger '{query.LedgerId}' was not found.")
            : ExecutionResult<LedgerAggregate>.Success(ledger);
    }
}
