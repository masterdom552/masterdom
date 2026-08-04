using Masterdom.Modules.Documents.Application.Commands;
using Masterdom.Modules.Documents.Application.Models;
using Masterdom.Modules.Documents.Application.Queries;
using Masterdom.Modules.Documents.Application.Support;

namespace Masterdom.Host.Api;

internal static class DocumentEndpoints
{
    public static IEndpointRouteBuilder MapDocumentEndpoints(this IEndpointRouteBuilder app)
    {
        ArgumentNullException.ThrowIfNull(app);

        var group = app.MapGroup("/api/documents").WithTags("Documents").RequireAuthorization();
        group.MapPost("/generate", Generate);
        group.MapPost("/preview", Preview);
        group.MapGet("/{documentId}/download", Download);
        group.MapPost("/{documentId}/regenerate", Regenerate);
        group.MapGet("/{documentType}/history", History);

        return app;
    }

    internal static IResult Generate(
        GenerateDocumentRequest request,
        ICommandHandler<GenerateDocumentCommand, ExecutionResult<GeneratedDocument>> handler)
    {
        var result = handler.Handle(new GenerateDocumentCommand(
            request.DocumentType,
            request.RequestedBy,
            request.Parameters,
            request.TemplateCode,
            request.TemplateVersion,
            request.ExportFormat));

        if (!result.IsSuccess || result.Value is null)
        {
            return ApiExecutionResults.ToErrorResult(result.ErrorCode, result.ErrorMessage);
        }

        return TypedResults.Ok(result.Value);
    }

    internal static IResult Preview(
        PreviewDocumentRequest request,
        IQueryHandler<PreviewDocumentQuery, ExecutionResult<GeneratedDocument>> handler)
    {
        var result = handler.Handle(new PreviewDocumentQuery(
            request.DocumentType,
            request.RequestedBy,
            request.Parameters,
            request.TemplateCode,
            request.TemplateVersion));

        if (!result.IsSuccess || result.Value is null)
        {
            return ApiExecutionResults.ToErrorResult(result.ErrorCode, result.ErrorMessage);
        }

        return TypedResults.Ok(result.Value);
    }

    internal static IResult Download(
        string documentId,
        IQueryHandler<DownloadDocumentQuery, ExecutionResult<GeneratedDocument>> handler)
    {
        var result = handler.Handle(new DownloadDocumentQuery(documentId));
        if (!result.IsSuccess || result.Value is null)
        {
            return ApiExecutionResults.ToErrorResult(result.ErrorCode, result.ErrorMessage);
        }

        return TypedResults.Ok(result.Value);
    }

    internal static IResult Regenerate(
        string documentId,
        RegenerateDocumentRequest request,
        ICommandHandler<RegenerateDocumentCommand, ExecutionResult<GeneratedDocument>> handler)
    {
        var result = handler.Handle(new RegenerateDocumentCommand(documentId, request.RequestedBy, request.ExportFormat));
        if (!result.IsSuccess || result.Value is null)
        {
            return ApiExecutionResults.ToErrorResult(result.ErrorCode, result.ErrorMessage);
        }

        return TypedResults.Ok(result.Value);
    }

    internal static IResult History(
        string documentType,
        int page,
        int pageSize,
        IQueryHandler<GetDocumentHistoryQuery, ExecutionResult<IReadOnlyCollection<DocumentHistoryEntry>>> handler)
    {
        var result = handler.Handle(new GetDocumentHistoryQuery(documentType, page, pageSize));
        if (!result.IsSuccess || result.Value is null)
        {
            return ApiExecutionResults.ToErrorResult(result.ErrorCode, result.ErrorMessage);
        }

        return TypedResults.Ok(result.Value);
    }

    internal sealed record GenerateDocumentRequest(
        string DocumentType,
        Guid RequestedBy,
        IReadOnlyDictionary<string, string> Parameters,
        string? TemplateCode,
        int? TemplateVersion,
        DocumentExportFormat ExportFormat);

    internal sealed record PreviewDocumentRequest(
        string DocumentType,
        Guid RequestedBy,
        IReadOnlyDictionary<string, string> Parameters,
        string? TemplateCode,
        int? TemplateVersion);

    internal sealed record RegenerateDocumentRequest(
        Guid RequestedBy,
        DocumentExportFormat ExportFormat);
}
