using Masterdom.Modules.Metering.Domain.Entities.Metering;

namespace Masterdom.Modules.Metering.Application.Commands;

public sealed record CorrectReadingCommand(
    MeterId MeterId,
    Guid ReadingId,
    ReadingValue CorrectedValue,
    string Reason,
    SubmittedBy CorrectedBy,
    DateTime CorrectedAtUtc);
