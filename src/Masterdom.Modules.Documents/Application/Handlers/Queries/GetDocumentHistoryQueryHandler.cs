using Masterdom.Modules.Documents.Application.Models;
using Masterdom.Modules.Documents.Application.Queries;
using Masterdom.Modules.Documents.Application.Services;
using Masterdom.Modules.Documents.Application.Support;

namespace Masterdom.Modules.Documents.Application.Handlers.Queries;

public sealed class GetDocumentHistoryQueryHandler
    : IQueryHandler<GetDocumentHistoryQuery, ExecutionResult<IReadOnlyCollection<DocumentHistoryEntry>>>
{
    private readonly IDocumentApplicationService _applicationService;

    public GetDocumentHistoryQueryHandler(IDocumentApplicationService applicationService)
    {
        _applicationService = applicationService ?? throw new ArgumentNullException(nameof(applicationService));
    }

    public ExecutionResult<IReadOnlyCollection<DocumentHistoryEntry>> Handle(GetDocumentHistoryQuery query)
    {
        ArgumentNullException.ThrowIfNull(query);

        try
        {
            var history = _applicationService.History(query.DocumentType, query.Page, query.PageSize);
            return ExecutionResult<IReadOnlyCollection<DocumentHistoryEntry>>.Success(history);
        }
        catch (ArgumentException ex)
        {
            return ExecutionResult<IReadOnlyCollection<DocumentHistoryEntry>>.Failure("validation_failed", ex.Message);
        }
    }
}
