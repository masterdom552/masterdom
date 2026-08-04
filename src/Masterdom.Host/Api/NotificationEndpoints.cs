using Masterdom.Modules.Notifications.Application.Commands;
using Masterdom.Modules.Notifications.Application.Models;
using Masterdom.Modules.Notifications.Application.Queries;
using Masterdom.Modules.Notifications.Application.Support;

namespace Masterdom.Host.Api;

internal static class NotificationEndpoints
{
    public static IEndpointRouteBuilder MapNotificationEndpoints(this IEndpointRouteBuilder app)
    {
        ArgumentNullException.ThrowIfNull(app);

        var group = app.MapGroup("/api/notifications").WithTags("Notifications").RequireAuthorization();
        group.MapPost("/generate", Generate);
        group.MapGet("/history/{recipientId:guid}", History);

        return app;
    }

    internal static IResult Generate(
        GenerateNotificationRequest request,
        ICommandHandler<GenerateNotificationCommand, ExecutionResult<GeneratedNotification>> handler)
    {
        var command = new GenerateNotificationCommand(
            request.EventCode,
            request.RecipientId,
            request.RequestedAtUtc,
            request.Parameters,
            request.RequestedDeliveryAtUtc);

        var result = handler.Handle(command);
        if (!result.IsSuccess || result.Value is null)
        {
            return ApiExecutionResults.ToErrorResult(result.ErrorCode, result.ErrorMessage);
        }

        return TypedResults.Ok(result.Value);
    }

    internal static IResult History(
        Guid recipientId,
        int page,
        int pageSize,
        IQueryHandler<GetNotificationHistoryQuery, ExecutionResult<IReadOnlyCollection<NotificationHistoryEntry>>> handler)
    {
        var result = handler.Handle(new GetNotificationHistoryQuery(recipientId, page, pageSize));
        if (!result.IsSuccess || result.Value is null)
        {
            return ApiExecutionResults.ToErrorResult(result.ErrorCode, result.ErrorMessage);
        }

        return TypedResults.Ok(result.Value);
    }

    internal sealed record GenerateNotificationRequest(
        string EventCode,
        Guid RecipientId,
        DateTime RequestedAtUtc,
        IReadOnlyDictionary<string, string> Parameters,
        DateTime? RequestedDeliveryAtUtc);
}
