using Masterdom.Modules.PolicyFramework.Domain.Entities.PolicyFramework;

namespace Masterdom.Modules.PolicyFramework.Domain.Repositories;

public interface IPolicyRepository
{
    void Add(Policy policy);

    void Update(Policy policy);

    Policy? GetById(PolicyId id);

    Policy? GetApplicable(PolicyType policyType, PolicyScope scope, DateOnly asOfDate);
}
