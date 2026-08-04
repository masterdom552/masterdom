namespace Masterdom.Modules.People.Application.Queries;

/// <summary>
/// Query entry point for search-ready person retrieval.
/// </summary>
public sealed record SearchPeopleQuery(string? NumberContains, int Take = 50);
