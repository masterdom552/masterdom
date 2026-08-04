using System.Reflection;
using Masterdom.Infrastructure.Persistence;
using Masterdom.Modules.SubsidyOptimization.Application.Commands;
using Masterdom.Modules.SubsidyOptimization.Application.Services;
using Masterdom.Modules.SubsidyOptimization.Contracts.Metering;
using Masterdom.Modules.SubsidyOptimization.Contracts.UtilityRating;
using OptimizationRunAggregate = Masterdom.Modules.SubsidyOptimization.Domain.Entities.SubsidyOptimization.OptimizationRun;

namespace Masterdom.Architecture.Tests;

public sealed class SubsidyOptimizationModuleArchitectureTests
{
    private static readonly string RepositoryRoot = ResolveRepositoryRoot();

    [Fact]
    public void SubsidyOptimizationApplication_ShouldNotReferenceInfrastructureAssembly()
    {
        var references = typeof(ISubsidyOptimizationApplicationService).Assembly
            .GetReferencedAssemblies()
            .Select(x => x.Name)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        Assert.DoesNotContain("Masterdom.Infrastructure", references);
    }

    [Fact]
    public void SubsidyOptimizationModule_ShouldNotReferenceBillingPaymentOrLedgerModules()
    {
        var references = typeof(OptimizationRunAggregate).Assembly
            .GetReferencedAssemblies()
            .Select(x => x.Name ?? string.Empty)
            .ToList();

        Assert.DoesNotContain("Masterdom.Modules.Billing", references);
        Assert.DoesNotContain(references, x => x.Contains("Payment", StringComparison.OrdinalIgnoreCase));
        Assert.DoesNotContain(references, x => x.Contains("Ledger", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void SubsidyOptimizationModule_ShouldNotReferenceMeteringOrUtilityRatingAssemblies_Directly()
    {
        var references = typeof(OptimizationRunAggregate).Assembly
            .GetReferencedAssemblies()
            .Select(x => x.Name)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        Assert.DoesNotContain("Masterdom.Modules.Metering", references);
        Assert.DoesNotContain("Masterdom.Modules.UtilityRating", references);
    }

    [Fact]
    public void SubsidyOptimizationCommands_ShouldUsePublishedContracts_ForMeteringAndUtilityRatingInputs()
    {
        var startCommandType = typeof(StartOptimizationCommand);

        Assert.Equal(
            typeof(IReadOnlyCollection<MeteringConsumptionHistoryContract>),
            startCommandType.GetProperty(nameof(StartOptimizationCommand.ConsumptionHistory))?.PropertyType);

        Assert.Equal(
            typeof(IReadOnlyCollection<RatedConsumptionContract>),
            startCommandType.GetProperty(nameof(StartOptimizationCommand.RatedConsumptions))?.PropertyType);
    }

    [Fact]
    public void SubsidyOptimizationAggregate_ShouldNotExposeExternalAggregateTypes()
    {
        var namespaces = typeof(OptimizationRunAggregate)
            .GetProperties(BindingFlags.Instance | BindingFlags.Public)
            .Select(x => x.PropertyType.Namespace ?? string.Empty)
            .ToList();

        Assert.DoesNotContain(namespaces, x => x.StartsWith("Masterdom.Modules.Metering", StringComparison.Ordinal));
        Assert.DoesNotContain(namespaces, x => x.StartsWith("Masterdom.Modules.UtilityRating", StringComparison.Ordinal));
        Assert.DoesNotContain(namespaces, x => x.StartsWith("Masterdom.Modules.Billing", StringComparison.Ordinal));
    }

    [Fact]
    public void Infrastructure_ShouldReferenceSubsidyOptimizationModule_ForPersistenceAdaptation()
    {
        var references = typeof(MasterdomDbContext).Assembly
            .GetReferencedAssemblies()
            .Select(x => x.Name)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        Assert.Contains("Masterdom.Modules.SubsidyOptimization", references);
    }

    [Fact]
    public void SubsidyMaximizer_ShouldConsumeBusinessContextAndRecommendationPlatformContracts()
    {
        var subsidySourceFiles = EnumerateSubsidyModuleSourceFiles();

        var usesBusinessContext = subsidySourceFiles.Any(path =>
            File.ReadAllText(path).Contains("Masterdom.Platform.BusinessContext", StringComparison.Ordinal));
        var usesRecommendation = subsidySourceFiles.Any(path =>
            File.ReadAllText(path).Contains("Masterdom.Platform.Recommendation", StringComparison.Ordinal));

        Assert.True(usesBusinessContext);
        Assert.True(usesRecommendation);
    }

    [Fact]
    public void SubsidyMaximizer_ShouldNotModifyOrRebuildPlatformAssets()
    {
        var subsidySourceFiles = EnumerateSubsidyModuleSourceFiles();

        var offenders = subsidySourceFiles
            .Where(path =>
            {
                var content = File.ReadAllText(path);
                return content.Contains("new BusinessContext(", StringComparison.Ordinal) ||
                       content.Contains("class BusinessContext", StringComparison.Ordinal) ||
                       content.Contains("class RecommendationPipeline", StringComparison.Ordinal);
            })
            .Select(path => path.Replace('\\', '/'))
            .ToList();

        Assert.Empty(offenders);
    }

    [Fact]
    public void SubsidyMaximizer_ShouldNotExecuteDecisionsOrBusinessTransactions()
    {
        var forbiddenTokens = new[]
        {
            "Decision.Create",
            "IDecisionHandler",
            "CreateDecision(",
            "Approve(",
            "Reject(",
            "BusinessTransaction",
            "PostBillingJournal",
            "ReceivePayment"
        };

        var subsidySourceFiles = EnumerateSubsidyModuleSourceFiles();
        var offenders = subsidySourceFiles
            .Where(path => forbiddenTokens.Any(token => File.ReadAllText(path).Contains(token, StringComparison.Ordinal)))
            .Select(path => path.Replace('\\', '/'))
            .ToList();

        Assert.Empty(offenders);
    }

    private static IEnumerable<string> EnumerateSubsidyModuleSourceFiles()
    {
        return Directory.EnumerateFiles(
                Path.Combine(RepositoryRoot, "src", "Masterdom.Modules.SubsidyOptimization"),
                "*.cs",
                SearchOption.AllDirectories)
            .Select(path => path.Replace('\\', '/'));
    }

    private static string ResolveRepositoryRoot()
    {
        var current = new DirectoryInfo(AppContext.BaseDirectory);

        while (current is not null)
        {
            if (File.Exists(Path.Combine(current.FullName, "Masterdom.slnx")))
            {
                return current.FullName.Replace('\\', '/');
            }

            current = current.Parent;
        }

        throw new DirectoryNotFoundException("Could not resolve repository root from test execution path.");
    }
}
