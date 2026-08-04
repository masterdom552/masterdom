using Masterdom.Modules.Documents.Application.Commands;
using Masterdom.Modules.Documents.Application.Models;
using Masterdom.Modules.Documents.Application.Services;
using Masterdom.Modules.Documents.Application.Support;

namespace Masterdom.Modules.Documents.Application.Handlers.Commands;

public sealed class GenerateDocumentCommandHandler
    : ICommandHandler<GenerateDocumentCommand, ExecutionResult<GeneratedDocument>>
{
    private readonly IDocumentApplicationService _applicationService;

    public GenerateDocumentCommandHandler(IDocumentApplicationService applicationService)
    {
        _applicationService = applicationService ?? throw new ArgumentNullException(nameof(applicationService));
    }

    public ExecutionResult<GeneratedDocument> Handle(GenerateDocumentCommand command)
    {
        ArgumentNullException.ThrowIfNull(command);

        try
        {
            var generated = _applicationService.Generate(
                command.DocumentType,
                command.RequestedBy,
                command.Parameters,
                command.TemplateCode,
                command.TemplateVersion,
                command.ExportFormat);

            return ExecutionResult<GeneratedDocument>.Success(generated);
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
