using Masterdom.Modules.UtilityRating.Domain.Entities.UtilityRating;
using UtilityRatingAggregate = Masterdom.Modules.UtilityRating.Domain.Entities.UtilityRating.UtilityRating;

namespace Masterdom.Modules.UtilityRating.Application.Support;

public interface IUtilityRatingPlatformOrchestrator
{
    /// <summary>
    /// Resolves the governed tariff schedule effective for a rating request.
    /// </summary>
    /// <param name="tariffCode">The tariff code requested by the caller.</param>
    /// <param name="meterId">The meter used as the configuration scope.</param>
    /// <param name="asOfUtc">The timestamp used for effective-version resolution.</param>
    /// <returns>The effective governed tariff schedule, or <see langword="null"/> when none is valid.</returns>
    TariffSchedule? ResolveTariffSchedule(string tariffCode, Guid meterId, DateTime asOfUtc);

    void OnRatingMutated(UtilityRatingAggregate rating, string operationName);
}
