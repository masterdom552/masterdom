using UtilityRatingAggregate = Masterdom.Modules.UtilityRating.Domain.Entities.UtilityRating.UtilityRating;

namespace Masterdom.Modules.UtilityRating.Application.Support;

public interface IUtilityRatingPlatformOrchestrator
{
    void OnRatingMutated(UtilityRatingAggregate rating, string operationName);
}
