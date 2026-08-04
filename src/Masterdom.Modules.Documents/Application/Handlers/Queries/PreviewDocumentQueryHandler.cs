using Masterdom.Modules.Documents.Application.Models;
using Masterdom.Modules.Documents.Application.Queries;
using Masterdom.Modules.Documents.Application.Services;
using Masterdom.Modules.Documents.Application.Support;

namespace Masterdom.Modules.Documents.Application.Handlers.Queries;

public sealed class PreviewDocumentQueryHandler
    : IQueryHandler<PreviewDocumentQuery, ExecutionResult<GeneratedDocument>>
{
    private readonly IDocumentApplicationService _applicationService;

    public PreviewDocumentQueryHandler(IDocumentApplicationService applicationService)
    {
        _applicationService = applicationService ?? throw new ArgumentNullException(nameof(applicationService));
    }

    public ExecutionResult<GeneratedDocument> Handle(PreviewDocumentQuery query)
    {
        ArgumentNullException.ThrowIfNull(query);

        try
        {
            var preview = _applicationService.Preview(
                query.DocumentType,
                query.RequestedBy,
                query.Parameters,
                query.TemplateCode,
                query.TemplateVersion);

            return ExecutionResult<GeneratedDocument>.Success(preview);
        }
        catch (ArgumentException ex)
        {
            return ExecutionResult<GeneratedDocument>.Failure("validation_failed", ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            return ExecutionResult<GeneratedDocument>.Failure("forbidden", ex.Message);
        }
    }
}
