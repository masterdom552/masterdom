using Masterdom.Abstractions.Policies;
using Masterdom.Infrastructure.Persistence.PolicyFramework;
using Masterdom.Infrastructure.PolicyFramework;
using Masterdom.Modules.PolicyFramework.Application.Commands;
using Masterdom.Modules.PolicyFramework.Application.Handlers.Commands;
using Masterdom.Modules.PolicyFramework.Application.Handlers.Queries;
using Masterdom.Modules.PolicyFramework.Application.Queries;
using Masterdom.Modules.PolicyFramework.Application.Services;
using Masterdom.Modules.PolicyFramework.Application.Support;
using Masterdom.Modules.PolicyFramework.Domain.Entities.PolicyFramework;
using Masterdom.Modules.PolicyFramework.Domain.Repositories;
using Microsoft.Extensions.DependencyInjection;

namespace Masterdom.Infrastructure;

/// <summary>
/// Registers Policy Framework runtime dependencies.
/// </summary>
public static class PolicyFrameworkFoundationDependencyInjection
{
    /// <summary>
    /// Adds the Policy Framework runtime composition.
    /// </summary>
    public static IServiceCollection AddPolicyFrameworkRuntime(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddScoped<IPolicyRepository, PolicyRepository>();
        services.AddScoped<IPolicyFrameworkUnitOfWork, PolicyFrameworkUnitOfWork>();
        services.AddScoped<IPolicyFrameworkPlatformOrchestrator, PolicyFrameworkPlatformOrchestrator>();
        services.AddScoped<IPolicyFrameworkApplicationService, PolicyFrameworkApplicationService>();
        services.AddScoped<IApplicablePolicyResolver, ApplicablePolicyResolver>();

        services.AddScoped<ICommandHandler<CreatePolicyCommand, ExecutionResult<Policy>>, CreatePolicyCommandHandler>();
        services.AddScoped<ICommandHandler<CreatePolicyVersionCommand, ExecutionResult<Policy>>, CreatePolicyVersionCommandHandler>();
        services.AddScoped<ICommandHandler<ActivatePolicyVersionCommand, ExecutionResult<Policy>>, ActivatePolicyVersionCommandHandler>();
        services.AddScoped<ICommandHandler<ExpirePolicyCommand, ExecutionResult<Policy>>, ExpirePolicyCommandHandler>();
        services.AddScoped<ICommandHandler<ArchivePolicyCommand, ExecutionResult<Policy>>, ArchivePolicyCommandHandler>();
        services.AddScoped<ICommandHandler<AssignPolicyCommand, ExecutionResult<Policy>>, AssignPolicyCommandHandler>();

        services.AddScoped<IQueryHandler<GetPolicyByIdQuery, ExecutionResult<Policy>>, GetPolicyByIdQueryHandler>();
        services.AddScoped<IQueryHandler<GetApplicablePolicyQuery, ExecutionResult<Policy>>, GetApplicablePolicyQueryHandler>();

        return services;
    }
}
