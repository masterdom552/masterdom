namespace Masterdom.Modules.Properties.Application.Queries;

/// <summary>
/// Query entry point for search-ready property retrieval.
/// </summary>
public sealed record SearchPropertiesQuery(string? CodeContains, int Take = 50);
