using Masterdom.Modules.Reporting.Application.Queries;
using Masterdom.Modules.Reporting.Application.Services;

namespace Masterdom.Architecture.Tests;

public sealed class ReportingModuleArchitectureTests
{
    [Fact]
    public void ReportingApplication_ShouldNotReferenceInfrastructureAssembly()
    {
        var references = typeof(IReportApplicationService).Assembly
            .GetReferencedAssemblies()
            .Select(x => x.Name)
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .ToArray();

        Assert.DoesNotContain("Masterdom.Infrastructure", references);
    }

    [Fact]
    public void ReportingQuery_ShouldRemainReadOnlyShape()
    {
        var queryType = typeof(GenerateReportQuery);
        var propertyNames = queryType.GetProperties().Select(x => x.Name).ToArray();

        Assert.DoesNotContain("Command", propertyNames);
        Assert.Contains("ReportCode", propertyNames);
        Assert.Contains("Filters", propertyNames);
    }

    [Fact]
    public void ReportingModule_ShouldNotReferenceBillingOrPaymentDomainTypesDirectly()
    {
        var references = typeof(IReportApplicationService).Assembly
            .GetReferencedAssemblies()
            .Select(x => x.Name)
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .ToArray();

        Assert.DoesNotContain("Masterdom.Modules.Billing", references);
        Assert.DoesNotContain("Masterdom.Modules.Payment", references);
    }
}
