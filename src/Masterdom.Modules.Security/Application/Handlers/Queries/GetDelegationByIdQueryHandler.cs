using Masterdom.Core.Identity.Entities.DelegatedAuthority;
using Masterdom.Infrastructure.Persistence.Identity;
using Masterdom.Modules.Security.Application.Queries;
using Masterdom.Modules.Security.Application.Support;

namespace Masterdom.Modules.Security.Application.Handlers.Queries;

/// <summary>
/// Handler for GetDelegationByIdQuery.
///
/// Retrieves a delegation from the repository.
/// </summary>
public sealed class GetDelegationByIdQueryHandler : IQueryHandler<GetDelegationByIdQuery, ExecutionResult<DelegatedAuthority>>
{
    private readonly IDelegatedAuthorityRepository _repository;

    public GetDelegationByIdQueryHandler(IDelegatedAuthorityRepository repository)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
    }

    public ExecutionResult<DelegatedAuthority> Handle(GetDelegationByIdQuery query)
    {
        ArgumentNullException.ThrowIfNull(query);

        var id = new DelegatedAuthorityId(query.DelegatedAuthorityId);
        var delegation = _repository.GetByIdAsync(id).Result;

        if (delegation is null)
        {
            return ExecutionResult<DelegatedAuthority>.Failure(
                "not_found",
                $"Delegation with ID '{id}' not found.");
        }

        return ExecutionResult<DelegatedAuthority>.Success(delegation);
    }
}
