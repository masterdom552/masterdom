using System.Reflection;

namespace Masterdom.Architecture.Tests;

public sealed class ContractOwnershipArchitectureTests
{
    private static readonly string RepositoryRoot = ResolveRepositoryRoot();

    [Fact]
    public void BillingPublishedApis_ShouldRemainOwnedByBilling_AndConsumedThroughPublishedNamespaces()
    {
        var billSnapshotConsumers = FindTypeConsumers("BillSnapshotModel");
        var billPersistedConsumers = FindTypeConsumers("BillPersistedNotification");

        Assert.Contains(
            billSnapshotConsumers,
            path => path.EndsWith("src/Masterdom.Modules.FinancialLedger/Application/Posting/BillingSnapshotPostingPreparationService.cs", StringComparison.Ordinal));
        Assert.Contains(
            billPersistedConsumers,
            path => path.EndsWith("src/Masterdom.Modules.FinancialLedger/Application/Translation/BillingNotificationTranslator.cs", StringComparison.Ordinal));

        Assert.DoesNotContain(
            billSnapshotConsumers.Concat(billPersistedConsumers),
            path => path.Contains("Masterdom.Modules.Billing/Domain/", StringComparison.Ordinal));
    }

    [Fact]
    public void SharedFinancialPostingAbstractions_ShouldRemainBusinessNeutral_AndHaveMultipleConsumers()
    {
        var consumers = FindNamespaceConsumers("Masterdom.Abstractions.Financial.Posting");

        Assert.Contains(consumers, path => path.Contains("src/Masterdom.Modules.FinancialLedger/", StringComparison.Ordinal));
        Assert.Contains(consumers, path => path.Contains("src/Masterdom.Infrastructure/", StringComparison.Ordinal));
    }

    [Fact]
    public void LocalDtos_ShouldNotBeConsumedCrossModule()
    {
        AssertAllConsumersStayWithinOwner(
            "Masterdom.Modules.Payment.Contracts.Billing",
            "Masterdom.Modules.Payment");
        AssertAllConsumersStayWithinOwner(
            "Masterdom.Modules.UtilityRating.Contracts.Metering",
            "Masterdom.Modules.UtilityRating");
        AssertAllConsumersStayWithinOwner(
            "Masterdom.Modules.SubsidyOptimization.Contracts.Metering",
            "Masterdom.Modules.SubsidyOptimization");
        AssertAllConsumersStayWithinOwner(
            "Masterdom.Modules.SubsidyOptimization.Contracts.UtilityRating",
            "Masterdom.Modules.SubsidyOptimization");
        AssertAllConsumersStayWithinOwner(
            "Masterdom.Modules.FinancialLedger.Contracts.Billing",
            "Masterdom.Modules.FinancialLedger");
        AssertAllConsumersStayWithinOwner(
            "Masterdom.Modules.FinancialLedger.Contracts.Payment",
            "Masterdom.Modules.FinancialLedger");
    }

    [Fact]
    public void UnusedTranslatorAbstraction_ShouldNotGainConsumersWithoutArchitectureReview()
    {
        var consumers = FindNamespaceConsumers("Masterdom.Abstractions.Translation")
            .Where(path => !path.EndsWith("src/Masterdom.Abstractions/Translation/ITranslator.cs", StringComparison.Ordinal))
            .ToList();

        Assert.Empty(consumers);
    }

    private static void AssertAllConsumersStayWithinOwner(string namespacePrefix, string ownerModulePathSegment)
    {
        var consumers = FindNamespaceConsumers(namespacePrefix);

        Assert.DoesNotContain(
            consumers,
            path => !path.Contains($"src/{ownerModulePathSegment}/", StringComparison.Ordinal));
    }

    private static List<string> FindTypeConsumers(string typeName)
    {
        return EnumerateSourceFiles()
            .Where(path => File.ReadAllText(path).Contains(typeName, StringComparison.Ordinal))
            .ToList();
    }

    private static List<string> FindNamespaceConsumers(string namespacePrefix)
    {
        return EnumerateSourceFiles()
            .Where(path => File.ReadAllText(path).Contains(namespacePrefix, StringComparison.Ordinal))
            .ToList();
    }

    private static IEnumerable<string> EnumerateSourceFiles()
    {
        return Directory.EnumerateFiles(Path.Combine(RepositoryRoot, "src"), "*.cs", SearchOption.AllDirectories)
            .Where(path => !path.Contains("/bin/", StringComparison.Ordinal))
            .Where(path => !path.Contains("/obj/", StringComparison.Ordinal))
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
