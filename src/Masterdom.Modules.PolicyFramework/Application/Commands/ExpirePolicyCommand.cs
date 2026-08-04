using Masterdom.Modules.PolicyFramework.Domain.Entities.PolicyFramework;

namespace Masterdom.Modules.PolicyFramework.Application.Commands;

public sealed record ExpirePolicyCommand(
    PolicyId PolicyId,
    DateTime ExpiredAtUtc);
