using Masterdom.Modules.UtilityRating.Domain.Entities.UtilityRating;

namespace Masterdom.Modules.UtilityRating.Application.Commands;

public sealed record ApproveRatingCommand(
    UtilityRatingId UtilityRatingId,
    DateTime ApprovedAtUtc);
