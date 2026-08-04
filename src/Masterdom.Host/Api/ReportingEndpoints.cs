using Masterdom.Modules.Reporting.Application.Export;
using Masterdom.Modules.Reporting.Application.Models;
using Masterdom.Modules.Reporting.Application.Queries;
using Masterdom.Modules.Reporting.Application.Support;

namespace Masterdom.Host.Api;

internal static class ReportingEndpoints
{
    public static IEndpointRouteBuilder MapReportingEndpoints(this IEndpointRouteBuilder app)
    {
        ArgumentNullException.ThrowIfNull(app);

        var group = app.MapGroup("/api/reporting").WithTags("Reporting").RequireAuthorization();
        group.MapPost("/generate", GenerateReport);

        return app;
    }

    internal static IResult GenerateReport(
        GenerateReportRequest request,
        IQueryHandler<GenerateReportQuery, ExecutionResult<GeneratedReport>> handler)
    {
        var query = new GenerateReportQuery(
            request.ReportCode,
            request.SortBy,
            request.SortDescending,
            request.Page,
            request.PageSize,
            request.ExportFormat,
            request.TemplateName,
            request.CreateSnapshot,
            request.Filters);

        var result = handler.Handle(query);
        if (!result.IsSuccess || result.Value is null)
        {
            return ApiExecutionResults.ToErrorResult(result.ErrorCode, result.ErrorMessage);
        }

        var response = new GenerateReportResponse(
            result.Value.ReportCode,
            result.Value.MimeType,
            result.Value.ExportFileName,
            result.Value.ExportContent,
            result.Value.DataSet.TotalCount,
            result.Value.DataSet.Page,
            result.Value.DataSet.PageSize,
            result.Value.Snapshot?.SnapshotId,
            result.Value.Kpis,
            result.Value.DashboardSummaries);

        return TypedResults.Ok(response);
    }

    internal sealed record GenerateReportRequest(
        string ReportCode,
        string SortBy,
        bool SortDescending,
        int Page,
        int PageSize,
        ReportExportFormat ExportFormat,
        string? TemplateName,
        bool CreateSnapshot,
        IReadOnlyDictionary<string, string> Filters);

    internal sealed record GenerateReportResponse(
        string ReportCode,
        string MimeType,
        string ExportFileName,
        string ExportContent,
        int TotalCount,
        int Page,
        int PageSize,
        string? SnapshotId,
        IReadOnlyCollection<string> Kpis,
        IReadOnlyCollection<string> DashboardSummaries);
}
