using Masterdom.Core.Primitives;
using Masterdom.Modules.PolicyFramework.Application.Commands;
using Masterdom.Modules.PolicyFramework.Application.Queries;
using Masterdom.Modules.PolicyFramework.Application.Support;
using Masterdom.Modules.PolicyFramework.Domain.Entities.PolicyFramework;

namespace Masterdom.Host.Api;

internal static class PolicyFrameworkEndpoints
{
    public static IEndpointRouteBuilder MapPolicyFrameworkEndpoints(this IEndpointRouteBuilder app)
    {
        ArgumentNullException.ThrowIfNull(app);

        var group = app.MapGroup("/api/policies").WithTags("Policy Framework").RequireAuthorization();

        group.MapPost("/", CreatePolicy);
        group.MapPost("/{policyId:guid}/versions", CreatePolicyVersion);
        group.MapPut("/{policyId:guid}/versions/{versionNumber:int}/activate", ActivatePolicyVersion);
        group.MapPut("/{policyId:guid}/expire", ExpirePolicy);
        group.MapPut("/{policyId:guid}/archive", ArchivePolicy);
        group.MapPost("/{policyId:guid}/assignments", AssignPolicy);
        group.MapGet("/{policyId:guid}", GetPolicyById);
        group.MapGet("/applicable", GetApplicablePolicy);

        return app;
    }

    internal static IResult CreatePolicy(
        CreatePolicyRequest request,
        ICommandHandler<CreatePolicyCommand, ExecutionResult<Policy>> handler)
    {
        var result = handler.Handle(new CreatePolicyCommand(
            PolicyType.Create(request.PolicyType),
            PolicyCategory.Create(request.PolicyCategory),
            PolicyReference.Create(request.PolicyCode, request.PolicyDisplayName),
            PolicyScope.Create(PolicyScopeKind.Create(request.ScopeKind), request.ScopeKey),
            PolicyCondition.Create(request.SelectorKey, request.SelectorDefinition),
            PolicyMetadata.Create(request.Metadata),
            EffectiveDateRange.Create(request.EffectiveStartDate, request.EffectiveEndDate),
            request.CreatedAtUtc));

        return result.IsSuccess && result.Value is not null
            ? TypedResults.Created($"/api/policies/{result.Value.Id.Value}", PolicyResponse.From(result.Value))
            : ApiExecutionResults.ToErrorResult(result.ErrorCode, result.ErrorMessage);
    }

    internal static IResult CreatePolicyVersion(
        Guid policyId,
        CreatePolicyVersionRequest request,
        ICommandHandler<CreatePolicyVersionCommand, ExecutionResult<Policy>> handler)
    {
        var result = handler.Handle(new CreatePolicyVersionCommand(
            PolicyId.From(policyId),
            PolicyCondition.Create(request.SelectorKey, request.SelectorDefinition),
            PolicyMetadata.Create(request.Metadata),
            EffectiveDateRange.Create(request.EffectiveStartDate, request.EffectiveEndDate),
            request.CreatedAtUtc));

        return result.IsSuccess && result.Value is not null
            ? TypedResults.Ok(PolicyResponse.From(result.Value))
            : ApiExecutionResults.ToErrorResult(result.ErrorCode, result.ErrorMessage);
    }

    internal static IResult ActivatePolicyVersion(
        Guid policyId,
        int versionNumber,
        ActivatePolicyVersionRequest request,
        ICommandHandler<ActivatePolicyVersionCommand, ExecutionResult<Policy>> handler)
    {
        var result = handler.Handle(new ActivatePolicyVersionCommand(
            PolicyId.From(policyId),
            versionNumber,
            request.ActivatedAtUtc));

        return result.IsSuccess && result.Value is not null
            ? TypedResults.Ok(PolicyResponse.From(result.Value))
            : ApiExecutionResults.ToErrorResult(result.ErrorCode, result.ErrorMessage);
    }

    internal static IResult ExpirePolicy(
        Guid policyId,
        ExpirePolicyRequest request,
        ICommandHandler<ExpirePolicyCommand, ExecutionResult<Policy>> handler)
    {
        var result = handler.Handle(new ExpirePolicyCommand(PolicyId.From(policyId), request.ExpiredAtUtc));

        return result.IsSuccess && result.Value is not null
            ? TypedResults.Ok(PolicyResponse.From(result.Value))
            : ApiExecutionResults.ToErrorResult(result.ErrorCode, result.ErrorMessage);
    }

    internal static IResult ArchivePolicy(
        Guid policyId,
        ArchivePolicyRequest request,
        ICommandHandler<ArchivePolicyCommand, ExecutionResult<Policy>> handler)
    {
        var result = handler.Handle(new ArchivePolicyCommand(PolicyId.From(policyId), request.Reason, request.ArchivedAtUtc));

        return result.IsSuccess && result.Value is not null
            ? TypedResults.Ok(PolicyResponse.From(result.Value))
            : ApiExecutionResults.ToErrorResult(result.ErrorCode, result.ErrorMessage);
    }

    internal static IResult AssignPolicy(
        Guid policyId,
        AssignPolicyRequest request,
        ICommandHandler<AssignPolicyCommand, ExecutionResult<Policy>> handler)
    {
        var assignment = PolicyAssignment.Create(
            request.AssignmentId,
            PolicyScope.Create(PolicyScopeKind.Create(request.ScopeKind), request.ScopeKey),
            request.AssignedEntityType,
            request.AssignedEntityId,
            EffectiveDateRange.Create(request.EffectiveStartDate, request.EffectiveEndDate),
            request.AssignedAtUtc);

        var result = handler.Handle(new AssignPolicyCommand(PolicyId.From(policyId), assignment));

        return result.IsSuccess && result.Value is not null
            ? TypedResults.Ok(PolicyResponse.From(result.Value))
            : ApiExecutionResults.ToErrorResult(result.ErrorCode, result.ErrorMessage);
    }

    internal static IResult GetPolicyById(
        Guid policyId,
        IQueryHandler<GetPolicyByIdQuery, ExecutionResult<Policy>> handler)
    {
        var result = handler.Handle(new GetPolicyByIdQuery(PolicyId.From(policyId)));

        return result.IsSuccess && result.Value is not null
            ? TypedResults.Ok(PolicyResponse.From(result.Value))
            : ApiExecutionResults.ToErrorResult(result.ErrorCode, result.ErrorMessage);
    }

    internal static IResult GetApplicablePolicy(
        string policyType,
        string scopeKind,
        string? scopeKey,
        DateOnly asOfDate,
        IQueryHandler<GetApplicablePolicyQuery, ExecutionResult<Policy>> handler)
    {
        var result = handler.Handle(new GetApplicablePolicyQuery(
            PolicyType.Create(policyType),
            PolicyScope.Create(PolicyScopeKind.Create(scopeKind), scopeKey),
            asOfDate));

        return result.IsSuccess && result.Value is not null
            ? TypedResults.Ok(PolicyResponse.From(result.Value))
            : ApiExecutionResults.ToErrorResult(result.ErrorCode, result.ErrorMessage);
    }

    internal sealed record CreatePolicyRequest(
        string PolicyType,
        string PolicyCategory,
        string PolicyCode,
        string PolicyDisplayName,
        string ScopeKind,
        string? ScopeKey,
        string SelectorKey,
        string SelectorDefinition,
        DateOnly EffectiveStartDate,
        DateOnly? EffectiveEndDate,
        DateTime CreatedAtUtc,
        IReadOnlyDictionary<string, string> Metadata);

    internal sealed record CreatePolicyVersionRequest(
        string SelectorKey,
        string SelectorDefinition,
        DateOnly EffectiveStartDate,
        DateOnly? EffectiveEndDate,
        DateTime CreatedAtUtc,
        IReadOnlyDictionary<string, string> Metadata);

    internal sealed record ActivatePolicyVersionRequest(DateTime ActivatedAtUtc);

    internal sealed record ExpirePolicyRequest(DateTime ExpiredAtUtc);

    internal sealed record ArchivePolicyRequest(string Reason, DateTime ArchivedAtUtc);

    internal sealed record AssignPolicyRequest(
        Guid AssignmentId,
        string ScopeKind,
        string? ScopeKey,
        string AssignedEntityType,
        string AssignedEntityId,
        DateOnly EffectiveStartDate,
        DateOnly? EffectiveEndDate,
        DateTime AssignedAtUtc);

    internal sealed record PolicyResponse(
        Guid Id,
        string PolicyType,
        string PolicyCategory,
        string PolicyCode,
        string PolicyDisplayName,
        string ScopeKind,
        string ScopeKey,
        string Status,
        int CurrentVersionNumber,
        string CurrentVersionStatus,
        DateTime CreatedAtUtc,
        DateTime? ActivatedAtUtc,
        DateTime? ExpiredAtUtc,
        DateTime? ArchivedAtUtc,
        string? ArchivedReason,
        int VersionCount,
        int AssignmentCount,
        int SnapshotCount)
    {
        public static PolicyResponse From(Policy policy)
        {
            return new PolicyResponse(
                policy.Id.Value,
                policy.PolicyType.Value,
                policy.PolicyCategory.Value,
                policy.PolicyReference.PolicyCode,
                policy.PolicyReference.DisplayName,
                policy.Scope.Kind.Value,
                policy.Scope.ScopeKey,
                policy.Status.Value,
                policy.CurrentVersion.VersionNumber,
                policy.CurrentVersion.Status.Value,
                policy.CreatedAtUtc,
                policy.ActivatedAtUtc,
                policy.ExpiredAtUtc,
                policy.ArchivedAtUtc,
                policy.ArchivedReason,
                policy.Versions.Count,
                policy.Assignments.Count,
                policy.Snapshots.Count);
        }
    }
}
