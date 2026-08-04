using Masterdom.Modules.PolicyFramework.Domain.Entities.PolicyFramework;

namespace Masterdom.Modules.PolicyFramework.Application.Commands;

public sealed record ArchivePolicyCommand(
    PolicyId PolicyId,
    string Reason,
    DateTime ArchivedAtUtc);
