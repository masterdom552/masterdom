using Masterdom.Modules.PolicyFramework.Domain.Entities.PolicyFramework;

namespace Masterdom.Modules.PolicyFramework.Application.Commands;

public sealed record ActivatePolicyVersionCommand(
    PolicyId PolicyId,
    int VersionNumber,
    DateTime ActivatedAtUtc);
