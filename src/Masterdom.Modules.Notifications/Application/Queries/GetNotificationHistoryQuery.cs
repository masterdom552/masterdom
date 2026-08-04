namespace Masterdom.Modules.Notifications.Application.Queries;

public sealed record GetNotificationHistoryQuery(Guid RecipientId, int Page, int PageSize);
