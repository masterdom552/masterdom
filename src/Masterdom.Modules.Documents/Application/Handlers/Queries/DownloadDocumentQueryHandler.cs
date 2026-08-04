using Masterdom.Modules.Documents.Application.Models;
using Masterdom.Modules.Documents.Application.Queries;
using Masterdom.Modules.Documents.Application.Services;
using Masterdom.Modules.Documents.Application.Support;

namespace Masterdom.Modules.Documents.Application.Handlers.Queries;

public sealed class DownloadDocumentQueryHandler
    : IQueryHandler<DownloadDocumentQuery, ExecutionResult<GeneratedDocument>>
{
    private readonly IDocumentApplicationService _applicationService;

    public DownloadDocumentQueryHandler(IDocumentApplicationService applicationService)
    {
        _applicationService = applicationService ?? throw new ArgumentNullException(nameof(applicationService));
    }

    public ExecutionResult<GeneratedDocument> Handle(DownloadDocumentQuery query)
    {
        ArgumentNullException.ThrowIfNull(query);

        try
        {
            var document = _applicationService.Download(query.DocumentId);
            return ExecutionResult<GeneratedDocument>.Success(document);
        }
        catch (ArgumentException ex)
        {
            return ExecutionResult<GeneratedDocument>.Failure("validation_failed", ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            return ExecutionResult<GeneratedDocument>.Failure("not_found", ex.Message);
        }
    }
}
