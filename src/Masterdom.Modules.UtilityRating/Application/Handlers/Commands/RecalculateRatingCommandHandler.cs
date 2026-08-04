using Masterdom.Modules.UtilityRating.Application.Commands;
using Masterdom.Modules.UtilityRating.Application.Services;
using Masterdom.Modules.UtilityRating.Application.Support;
using UtilityRatingAggregate = Masterdom.Modules.UtilityRating.Domain.Entities.UtilityRating.UtilityRating;

namespace Masterdom.Modules.UtilityRating.Application.Handlers.Commands;

public sealed class RecalculateRatingCommandHandler : ICommandHandler<RecalculateRatingCommand, ExecutionResult<UtilityRatingAggregate>>
{
    private readonly IUtilityRatingApplicationService _applicationService;

    public RecalculateRatingCommandHandler(IUtilityRatingApplicationService applicationService)
    {
        _applicationService = applicationService ?? throw new ArgumentNullException(nameof(applicationService));
    }

    public ExecutionResult<UtilityRatingAggregate> Handle(RecalculateRatingCommand command)
    {
        ArgumentNullException.ThrowIfNull(command);

        try
        {
            var rating = _applicationService.RecalculateRating(command);
            return ExecutionResult<UtilityRatingAggregate>.Success(rating);
        }
        catch (ArgumentException ex)
        {
            return ExecutionResult<UtilityRatingAggregate>.Failure("validation_failed", ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            return ExecutionResult<UtilityRatingAggregate>.Failure("conflict", ex.Message);
        }
    }
}
