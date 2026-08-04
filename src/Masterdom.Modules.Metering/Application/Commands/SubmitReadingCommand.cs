using Masterdom.Modules.Metering.Domain.Entities.Metering;

namespace Masterdom.Modules.Metering.Application.Commands;

public sealed record SubmitReadingCommand(
    MeterId MeterId,
    ReadingDate ReadingDate,
    ReadingValue ReadingValue,
    ReadingSource ReadingSource,
    SubmittedBy SubmittedBy,
    DateTime SubmittedAtUtc,
    bool AllowFutureReadings,
    bool IsRollover,
    ReadingNotes? ReadingNotes);
