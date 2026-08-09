using Masterdom.Modules.PolicyFramework.Application.Commands;
using Masterdom.Modules.PolicyFramework.Application.Queries;
using Masterdom.Modules.PolicyFramework.Application.Support;
using Masterdom.Modules.PolicyFramework.Domain.Entities.PolicyFramework;
using Masterdom.Modules.PolicyFramework.Domain.Repositories;
using PolicyAggregate = Masterdom.Modules.PolicyFramework.Domain.Entities.PolicyFramework.Policy;

namespace Masterdom.Modules.PolicyFramework.Application.Services;

public sealed class PolicyFrameworkApplicationService : IPolicyFrameworkApplicationService
{
    private readonly IPolicyRepository _repository;
    private readonly IPolicyFrameworkUnitOfWork _unitOfWork;
    private readonly IPolicyFrameworkPlatformOrchestrator _platformOrchestrator;

    public PolicyFrameworkApplicationService(
        IPolicyRepository repository,
        IPolicyFrameworkUnitOfWork unitOfWork,
        IPolicyFrameworkPlatformOrchestrator platformOrchestrator)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _platformOrchestrator = platformOrchestrator ?? throw new ArgumentNullException(nameof(platformOrchestrator));
    }

    public PolicyAggregate CreatePolicy(CreatePolicyCommand command)
    {
        ArgumentNullException.ThrowIfNull(command);

        var policy = PolicyAggregate.Create(
            PolicyId.New(),
            command.PolicyType,
            command.PolicyCategory,
            command.PolicyReference,
            command.Scope,
            command.Condition,
            command.Metadata,
            command.EffectiveDateRange,
            command.CreatedAtUtc);

        _unitOfWork.Execute(() => _repository.Add(policy));
        _platformOrchestrator.OnPolicyMutated(policy, "CreatePolicy");

        return policy;
    }

    public PolicyAggregate CreatePolicyVersion(CreatePolicyVersionCommand command)
    {
        ArgumentNullException.ThrowIfNull(command);

        var policy = GetRequiredPolicy(command.PolicyId);
        policy.CreateVersion(
            command.Condition,
            command.Metadata,
            command.EffectiveDateRange,
            command.CreatedAtUtc);

        _unitOfWork.Execute(() => _repository.Update(policy));
        _platformOrchestrator.OnPolicyMutated(policy, "CreatePolicyVersion");

        return policy;
    }

    public PolicyAggregate ActivatePolicyVersion(ActivatePolicyVersionCommand command)
    {
        ArgumentNullException.ThrowIfNull(command);

        var policy = GetRequiredPolicy(command.PolicyId);
        policy.ActivateVersion(command.VersionNumber, command.ActivatedAtUtc);

        _unitOfWork.Execute(() => _repository.Update(policy));
        _platformOrchestrator.OnPolicyMutated(policy, "ActivatePolicyVersion");

        return policy;
    }

    public PolicyAggregate ExpirePolicy(ExpirePolicyCommand command)
    {
        ArgumentNullException.ThrowIfNull(command);

        var policy = GetRequiredPolicy(command.PolicyId);
        policy.Expire(command.ExpiredAtUtc);

        _unitOfWork.Execute(() => _repository.Update(policy));
        _platformOrchestrator.OnPolicyMutated(policy, "ExpirePolicy");

        return policy;
    }

    public PolicyAggregate ArchivePolicy(ArchivePolicyCommand command)
    {
        ArgumentNullException.ThrowIfNull(command);

        var policy = GetRequiredPolicy(command.PolicyId);
        policy.Archive(command.ArchivedAtUtc, command.Reason);

        _unitOfWork.Execute(() => _repository.Update(policy));
        _platformOrchestrator.OnPolicyMutated(policy, "ArchivePolicy");

        return policy;
    }

    public PolicyAggregate AssignPolicy(AssignPolicyCommand command)
    {
        ArgumentNullException.ThrowIfNull(command);

        var policy = GetRequiredPolicy(command.PolicyId);
        policy.Assign(command.Assignment);

        _unitOfWork.Execute(() => _repository.Update(policy));
        _platformOrchestrator.OnPolicyMutated(policy, "AssignPolicy");

        return policy;
    }

    public PolicyAggregate? GetPolicy(GetPolicyByIdQuery query)
    {
        ArgumentNullException.ThrowIfNull(query);
        return _repository.GetById(query.PolicyId);
    }

    public PolicyAggregate? GetApplicablePolicy(GetApplicablePolicyQuery query)
    {
        ArgumentNullException.ThrowIfNull(query);
        return _repository.GetApplicable(query.PolicyType, query.Scope, query.AsOfDate, query.PolicyCode);
    }

    private PolicyAggregate GetRequiredPolicy(PolicyId policyId)
    {
        var policy = _repository.GetById(policyId);
        if (policy is null)
        {
            throw new InvalidOperationException($"Policy '{policyId}' was not found.");
        }

        return policy;
    }
}
