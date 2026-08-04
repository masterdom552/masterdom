using Masterdom.Modules.UtilityRating.Domain.Entities.UtilityRating;
using Masterdom.Modules.UtilityRating.Domain.Repositories;
using Microsoft.EntityFrameworkCore;
using UtilityRatingAggregate = Masterdom.Modules.UtilityRating.Domain.Entities.UtilityRating.UtilityRating;

namespace Masterdom.Infrastructure.Persistence.UtilityRating;

public sealed class UtilityRatingRepository : IUtilityRatingRepository
{
    private readonly MasterdomDbContext _dbContext;

    public UtilityRatingRepository(MasterdomDbContext dbContext)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
    }

    public void Add(UtilityRatingAggregate rating)
    {
        ArgumentNullException.ThrowIfNull(rating);
        _dbContext.UtilityRatings.Add(rating);
    }

    public void Update(UtilityRatingAggregate rating)
    {
        ArgumentNullException.ThrowIfNull(rating);
        _dbContext.UtilityRatings.Update(rating);
    }

    public UtilityRatingAggregate? GetById(UtilityRatingId id)
    {
        ArgumentNullException.ThrowIfNull(id);

        return _dbContext.UtilityRatings
            .FirstOrDefault(x => x.Id == id);
    }

    public UtilityRatingAggregate? GetByMeterPeriodAndVersion(
        MeterReference meterReference,
        RatingPeriod ratingPeriod,
        RatingVersion ratingVersion)
    {
        ArgumentNullException.ThrowIfNull(meterReference);
        ArgumentNullException.ThrowIfNull(ratingPeriod);
        ArgumentNullException.ThrowIfNull(ratingVersion);

        return _dbContext.UtilityRatings
            .AsEnumerable()
            .FirstOrDefault(x =>
                x.MeterReference == meterReference &&
                x.RatingPeriod == ratingPeriod &&
                x.RatingVersion == ratingVersion);
    }

    public UtilityRatingAggregate? GetLatestByMeterAndPeriod(
        MeterReference meterReference,
        RatingPeriod ratingPeriod)
    {
        ArgumentNullException.ThrowIfNull(meterReference);
        ArgumentNullException.ThrowIfNull(ratingPeriod);

        return _dbContext.UtilityRatings
            .AsEnumerable()
            .Where(x => x.MeterReference == meterReference && x.RatingPeriod == ratingPeriod)
            .OrderByDescending(x => x.RatingVersion.Value)
            .FirstOrDefault();
    }
}
