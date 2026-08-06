namespace Masterdom.Architecture.Tests;

public sealed class PropertyCapabilitySecurityArchitectureTests
{
    private static readonly string RepositoryRoot = ResolveRepositoryRoot();

    [Fact]
    public void PropertyCapabilityEndpoints_ShouldRequireAuthorization()
    {
        AssertContains(RepositoryPath("src/Masterdom.Host/Api/PropertyEndpoints.cs"), ".RequireAuthorization()");
        AssertContains(RepositoryPath("src/Masterdom.Host/Api/PeopleEndpoints.cs"), ".RequireAuthorization()");
        AssertContains(RepositoryPath("src/Masterdom.Host/Api/LeaseEndpoints.cs"), ".RequireAuthorization()");
        AssertContains(RepositoryPath("src/Masterdom.Host/Api/TenancyEndpoints.cs"), ".RequireAuthorization()");
    }

    [Fact]
    public void HostProgram_ShouldUseAuthenticationAndAuthorizationMiddleware()
    {
        AssertContains(RepositoryPath("src/Masterdom.Host/Program.cs"), "app.UseAuthentication();");
        AssertContains(RepositoryPath("src/Masterdom.Host/Program.cs"), "app.UseAuthorization();");
        AssertContains(RepositoryPath("src/Masterdom.Host/Program.cs"), "AddSecurityModule(builder.Configuration)");
    }

    [Fact]
    public void PropertyCapabilityRuntime_ShouldRegisterAuthorizationDecorators()
    {
        AssertContains(RepositoryPath("src/Masterdom.Infrastructure/PropertyFoundationDependencyInjection.cs"), "AuthorizationDecorator");
        AssertContains(RepositoryPath("src/Masterdom.Infrastructure/PropertyFoundationDependencyInjection.cs"), "AddSecurityInfrastructureRuntime");
    }

    [Fact]
    public void SecurityModule_ShouldOwnSecurityCompositionEntryPoint()
    {
        AssertContains(RepositoryPath("src/Masterdom.Modules.Security/SecurityModuleServiceCollectionExtensions.cs"), "AddSecurityModule");
        AssertContains(RepositoryPath("src/Masterdom.Modules.Security/SecurityModuleServiceCollectionExtensions.cs"), "AddSecurityInfrastructureRuntime");
    }

    private static void AssertContains(string path, string expectedText)
    {
        var content = File.ReadAllText(path);
        Assert.Contains(expectedText, content, StringComparison.Ordinal);
    }

    private static string RepositoryPath(string relativePath)
    {
        return Path.Combine(RepositoryRoot, relativePath.Replace('/', Path.DirectorySeparatorChar));
    }

    private static string ResolveRepositoryRoot()
    {
        var current = new DirectoryInfo(AppContext.BaseDirectory);

        while (current is not null)
        {
            if (File.Exists(Path.Combine(current.FullName, "Masterdom.slnx")))
            {
                return current.FullName;
            }

            current = current.Parent;
        }

        throw new DirectoryNotFoundException("Could not resolve repository root from test execution path.");
    }
}
