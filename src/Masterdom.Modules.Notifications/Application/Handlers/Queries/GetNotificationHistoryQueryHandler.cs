using Masterdom.Modules.Notifications.Application.Models;
using Masterdom.Modules.Notifications.Application.Queries;
using Masterdom.Modules.Notifications.Application.Services;
using Masterdom.Modules.Notifications.Application.Support;

namespace Masterdom.Modules.Notifications.Application.Handlers.Queries;

public sealed class GetNotificationHistoryQueryHandler
    : IQueryHandler<GetNotificationHistoryQuery, ExecutionResult<IReadOnlyCollection<NotificationHistoryEntry>>>
{
    private readonly INotificationApplicationService _applicationService;

    public GetNotificationHistoryQueryHandler(INotificationApplicationService applicationService)
    {
        _applicationService = applicationService ?? throw new ArgumentNullException(nameof(applicationService));
    }

    public ExecutionResult<IReadOnlyCollection<NotificationHistoryEntry>> Handle(GetNotificationHistoryQuery query)
    {
        ArgumentNullException.ThrowIfNull(query);

        var history = _applicationService.History(query.RecipientId, query.Page, query.PageSize);
        return ExecutionResult<IReadOnlyCollection<NotificationHistoryEntry>>.Success(history);
    }
}
