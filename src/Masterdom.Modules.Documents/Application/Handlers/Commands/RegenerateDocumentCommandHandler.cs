using Masterdom.Modules.Documents.Application.Commands;
using Masterdom.Modules.Documents.Application.Models;
using Masterdom.Modules.Documents.Application.Services;
using Masterdom.Modules.Documents.Application.Support;

namespace Masterdom.Modules.Documents.Application.Handlers.Commands;

public sealed class RegenerateDocumentCommandHandler
    : ICommandHandler<RegenerateDocumentCommand, ExecutionResult<GeneratedDocument>>
{
    private readonly IDocumentApplicationService _applicationService;

    public RegenerateDocumentCommandHandler(IDocumentApplicationService applicationService)
    {
        _applicationService = applicationService ?? throw new ArgumentNullException(nameof(applicationService));
    }

    public ExecutionResult<GeneratedDocument> Handle(RegenerateDocumentCommand command)
    {
        ArgumentNullException.ThrowIfNull(command);

        try
        {
            var regenerated = _applicationService.Regenerate(command.DocumentId, command.RequestedBy, command.ExportFormat);
            return ExecutionResult<GeneratedDocument>.Success(regenerated);
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
