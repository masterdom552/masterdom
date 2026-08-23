namespace Masterdom.Modules.Security.Application.Queries;

/// <summary>
/// Query to retrieve a delegation by its ID.
/// </summary>
public sealed record GetDelegationByIdQuery(Guid DelegatedAuthorityId);
