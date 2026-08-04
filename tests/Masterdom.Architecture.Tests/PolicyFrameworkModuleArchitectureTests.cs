using System.Reflection;
using Masterdom.Infrastructure.Persistence;
using Masterdom.Infrastructure.Persistence.PolicyFramework;
using Masterdom.Modules.PolicyFramework.Application.Commands;
using Masterdom.Modules.PolicyFramework.Application.Services;
using Masterdom.Modules.PolicyFramework.Domain.Entities.PolicyFramework;

namespace Masterdom.Architecture.Tests;

public sealed class PolicyFrameworkModuleArchitectureTests
{
    [Fact]
    public void PolicyFrameworkApplication_ShouldNotReferenceInfrastructureAssembly()
    {
        var references = typeof(IPolicyFrameworkApplicationService).Assembly
            .GetReferencedAssemblies()
            .Select(x => x.Name)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        Assert.DoesNotContain("Masterdom.Infrastructure", references);
    }

    [Fact]
    public void PolicyFrameworkModule_ShouldNotReferenceBusinessModuleAssemblies()
    {
        var references = typeof(Policy)
            .Assembly
            .GetReferencedAssemblies()
            .Select(x => x.Name ?? string.Empty)
            .ToList();

        Assert.DoesNotContain(references, x => x.Equals("Masterdom.Modules.Billing", StringComparison.OrdinalIgnoreCase));
        Assert.DoesNotContain(references, x => x.Equals("Masterdom.Modules.Lease", StringComparison.OrdinalIgnoreCase));
        Assert.DoesNotContain(references, x => x.Equals("Masterdom.Modules.Metering", StringComparison.OrdinalIgnoreCase));
        Assert.DoesNotContain(references, x => x.Equals("Masterdom.Modules.UtilityRating", StringComparison.OrdinalIgnoreCase));
        Assert.DoesNotContain(references, x => x.Equals("Masterdom.Modules.SubsidyOptimization", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void PolicyFrameworkCommands_ShouldUsePolicyDomainTypes_Only()
    {
        var commandType = typeof(CreatePolicyCommand);

        Assert.Equal(typeof(PolicyType), commandType.GetProperty(nameof(CreatePolicyCommand.PolicyType))?.PropertyType);
        Assert.Equal(typeof(PolicyScope), commandType.GetProperty(nameof(CreatePolicyCommand.Scope))?.PropertyType);
        Assert.Equal(typeof(PolicyCondition), commandType.GetProperty(nameof(CreatePolicyCommand.Condition))?.PropertyType);
    }

    [Fact]
    public void PolicyFrameworkOrchestrator_ShouldNotDependOnRuleOrWorkflowResolvers()
    {
        var dependencyTypes = typeof(PolicyFrameworkPlatformOrchestrator)
            .GetFields(BindingFlags.Instance | BindingFlags.NonPublic)
            .Select(x => x.FieldType.FullName ?? string.Empty)
            .ToList();

        Assert.DoesNotContain(dependencyTypes, x => x.Contains("IRuleResolver", StringComparison.Ordinal));
        Assert.DoesNotContain(dependencyTypes, x => x.Contains("IWorkflowResolver", StringComparison.Ordinal));
    }

    [Fact]
    public void Infrastructure_ShouldReferencePolicyFrameworkModule_ForPersistenceAdaptation()
    {
        var references = typeof(MasterdomDbContext).Assembly
            .GetReferencedAssemblies()
            .Select(x => x.Name)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        Assert.Contains("Masterdom.Modules.PolicyFramework", references);
    }
}
