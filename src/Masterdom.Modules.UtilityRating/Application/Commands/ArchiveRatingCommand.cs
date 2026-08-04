using Masterdom.Modules.UtilityRating.Domain.Entities.UtilityRating;

namespace Masterdom.Modules.UtilityRating.Application.Commands;

public sealed record ArchiveRatingCommand(
    UtilityRatingId UtilityRatingId,
    string Reason,
    DateTime ArchivedAtUtc);
