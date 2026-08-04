namespace Masterdom.Modules.Documents.Application.Queries;

public sealed record GetDocumentHistoryQuery(string DocumentType, int Page, int PageSize);
