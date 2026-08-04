using Masterdom.Modules.PolicyFramework.Application.Commands;
using Masterdom.Modules.PolicyFramework.Application.Queries;
using PolicyAggregate = Masterdom.Modules.PolicyFramework.Domain.Entities.PolicyFramework.Policy;

namespace Masterdom.Modules.PolicyFramework.Application.Services;

public interface IPolicyFrameworkApplicationService
{
    PolicyAggregate CreatePolicy(CreatePolicyCommand command);

    PolicyAggregate CreatePolicyVersion(CreatePolicyVersionCommand command);

    PolicyAggregate ActivatePolicyVersion(ActivatePolicyVersionCommand command);

    PolicyAggregate ExpirePolicy(ExpirePolicyCommand command);

    PolicyAggregate ArchivePolicy(ArchivePolicyCommand command);

    PolicyAggregate AssignPolicy(AssignPolicyCommand command);

    PolicyAggregate? GetPolicy(GetPolicyByIdQuery query);

    PolicyAggregate? GetApplicablePolicy(GetApplicablePolicyQuery query);
}
