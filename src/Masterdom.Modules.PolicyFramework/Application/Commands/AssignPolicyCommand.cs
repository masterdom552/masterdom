using Masterdom.Modules.PolicyFramework.Domain.Entities.PolicyFramework;

namespace Masterdom.Modules.PolicyFramework.Application.Commands;

public sealed record AssignPolicyCommand(
    PolicyId PolicyId,
    PolicyAssignment Assignment);
