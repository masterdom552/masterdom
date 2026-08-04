using PolicyAggregate = Masterdom.Modules.PolicyFramework.Domain.Entities.PolicyFramework.Policy;

namespace Masterdom.Modules.PolicyFramework.Application.Support;

public interface IPolicyFrameworkPlatformOrchestrator
{
    void OnPolicyMutated(PolicyAggregate policy, string operationName);
}
