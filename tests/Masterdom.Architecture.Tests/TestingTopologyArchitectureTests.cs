using System.Xml.Linq;

namespace Masterdom.Architecture.Tests;

public sealed class TestingTopologyArchitectureTests
{
    private static readonly string[] DuplicatePlatformBoundaryContractNames =
    [
        "IModule.cs",
        "IModuleCatalog.cs",
        "IModuleLoader.cs",
        "IPlatformContext.cs"
    ];

    private static readonly HashSet<string> ExcludedPureTestProjectNames =
    [
        "Masterdom.Architecture.Tests",
        "Masterdom.Core.Tests"
    ];

    private static readonly HashSet<string> AllowedPureTestReferences =
    [
        "Masterdom.Core",
        "Masterdom.Abstractions",
        "Masterdom.TestKit"
    ];

    private static readonly HashSet<string> CommonTestPackages =
    [
        "coverlet.collector",
        "Microsoft.NET.Test.Sdk",
        "xunit",
        "xunit.runner.visualstudio"
    ];

    [Fact]
    public void PureTestProjects_ShouldNotReferenceForbiddenProjects()
    {
        var violations = new List<string>();

        foreach (var project in DiscoverTestProjects().Where(IsPureTestProject))
        {
            var projectName = Path.GetFileNameWithoutExtension(project);
            var owningProjectName = projectName[..^".Tests".Length];

            var allowedReferences = new HashSet<string>(AllowedPureTestReferences, StringComparer.OrdinalIgnoreCase)
            {
                owningProjectName
            };

            foreach (var projectReference in GetProjectReferences(project))
            {
                if (allowedReferences.Contains(projectReference))
                {
                    continue;
                }

                violations.Add(
                    $"Offending project '{projectName}'. Forbidden reference '{projectReference}'. " +
                    "Expected rule: pure test projects may reference only owning module, Masterdom.Core, Masterdom.Abstractions, and Masterdom.TestKit.");
            }
        }

        Assert.True(violations.Count == 0, string.Join(Environment.NewLine, violations));
    }

    [Fact]
    public void TestProjects_ShouldFollowNamingConventions()
    {
        var violations = new List<string>();

        foreach (var project in DiscoverTestProjects())
        {
            var projectName = Path.GetFileNameWithoutExtension(project);

            if (projectName.Equals("Masterdom.TestKit", StringComparison.Ordinal))
            {
                continue;
            }

            var hasValidSuffix = projectName.EndsWith(".Tests", StringComparison.Ordinal) ||
                                 projectName.EndsWith(".Infrastructure.Tests", StringComparison.Ordinal) ||
                                 projectName.EndsWith(".BusinessIntegration.Tests", StringComparison.Ordinal);

            if (!hasValidSuffix)
            {
                violations.Add(
                    $"Offending project '{projectName}'. Invalid naming convention. " +
                    "Expected rule: test projects must end with '.Tests', '.Infrastructure.Tests', or '.BusinessIntegration.Tests'.");
            }
        }

        Assert.True(violations.Count == 0, string.Join(Environment.NewLine, violations));
    }

    [Fact]
    public void CommonTestPackages_ShouldBeCentralized()
    {
        var violations = new List<string>();

        foreach (var project in DiscoverTestProjects())
        {
            var projectName = Path.GetFileNameWithoutExtension(project);

            if (projectName.Equals("Masterdom.TestKit", StringComparison.Ordinal))
            {
                continue;
            }

            var packageReferences = GetPackageReferences(project);

            foreach (var packageReference in packageReferences)
            {
                if (!CommonTestPackages.Contains(packageReference))
                {
                    continue;
                }

                violations.Add(
                    $"Offending project '{projectName}'. Duplicated package reference '{packageReference}'. " +
                    "Expected rule: common test packages must be declared once in tests/Directory.Build.props.");
            }
        }

        Assert.True(violations.Count == 0, string.Join(Environment.NewLine, violations));
    }

    [Fact]
    public void TestsDirectoryBuildProps_ShouldContainCommonTestPackages()
    {
        var repositoryRoot = ResolveRepositoryRoot();
        var buildPropsPath = Path.Combine(repositoryRoot, "tests", "Directory.Build.props");

        Assert.True(File.Exists(buildPropsPath), "Missing required file 'tests/Directory.Build.props'.");

        var propsDocument = XDocument.Load(buildPropsPath);
        var packageReferences = propsDocument
            .Descendants()
            .Where(element => element.Name.LocalName.Equals("PackageReference", StringComparison.Ordinal))
            .Select(element => element.Attribute("Include")?.Value)
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .Select(value => value!)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        var missingPackages = CommonTestPackages
            .Where(packageName => !packageReferences.Contains(packageName))
            .ToList();

        Assert.True(
            missingPackages.Count == 0,
            $"Missing common test packages in tests/Directory.Build.props: {string.Join(", ", missingPackages)}.");
    }

    [Fact]
    public void ModuleProjects_ShouldReferenceAbstractionsOnlyWhenTheyConsumeSharedContracts()
    {
        var repositoryRoot = ResolveRepositoryRoot();
        var moduleProjects = Directory.EnumerateFiles(
                Path.Combine(repositoryRoot, "src"),
                "Masterdom.Modules.*.csproj",
                SearchOption.AllDirectories)
            .Where(path => path.Contains($"{Path.DirectorySeparatorChar}src{Path.DirectorySeparatorChar}Masterdom.Modules.", StringComparison.Ordinal))
            .OrderBy(path => path, StringComparer.OrdinalIgnoreCase)
            .ToList();

        var violations = new List<string>();

        foreach (var projectPath in moduleProjects)
        {
            var referencesAbstractions = GetProjectReferences(projectPath)
                .Contains("Masterdom.Abstractions");
            var usesAbstractions = ProjectSourceUsesNamespace(
                Path.GetDirectoryName(projectPath)!,
                "Masterdom.Abstractions");

            if (referencesAbstractions == usesAbstractions)
            {
                continue;
            }

            var projectName = Path.GetFileNameWithoutExtension(projectPath);
            violations.Add(
                referencesAbstractions
                    ? $"Project '{projectName}' references Masterdom.Abstractions but does not consume any shared abstraction types."
                    : $"Project '{projectName}' consumes Masterdom.Abstractions types but does not reference Masterdom.Abstractions.");
        }

        Assert.True(violations.Count == 0, string.Join(Environment.NewLine, violations));
    }

    [Fact]
    public void PlatformRuntimeBoundaryContracts_ShouldNotBeDuplicatedInAbstractionsModules()
    {
        var repositoryRoot = ResolveRepositoryRoot();
        var platformModulesPath = Path.Combine(repositoryRoot, "src", "Masterdom.Platform", "Modules");
        var platformCorePath = Path.Combine(repositoryRoot, "src", "Masterdom.Platform", "Core");
        var abstractionsModulesPath = Path.Combine(repositoryRoot, "src", "Masterdom.Abstractions", "Modules");

        var violations = new List<string>();

        foreach (var contractName in DuplicatePlatformBoundaryContractNames)
        {
            var platformPath = contractName.Equals("IPlatformContext.cs", StringComparison.Ordinal)
                ? Path.Combine(platformCorePath, contractName)
                : Path.Combine(platformModulesPath, contractName);
            var abstractionsPath = Path.Combine(abstractionsModulesPath, contractName);

            if (!File.Exists(platformPath))
            {
                violations.Add($"Missing expected Platform runtime contract '{platformPath}'.");
            }

            if (File.Exists(abstractionsPath))
            {
                violations.Add(
                    $"Duplicate runtime boundary contract detected: '{abstractionsPath}' duplicates active Platform contract '{platformPath}'.");
            }
        }

        Assert.True(violations.Count == 0, string.Join(Environment.NewLine, violations));
    }

    [Fact]
    public void PlatformProject_ShouldNotReferenceAbstractionsForModuleBoundaryContracts()
    {
        var repositoryRoot = ResolveRepositoryRoot();
        var platformProjectPath = Path.Combine(repositoryRoot, "src", "Masterdom.Platform", "Masterdom.Platform.csproj");
        var projectDocument = XDocument.Load(platformProjectPath);

        var projectReferences = projectDocument
            .Descendants()
            .Where(element => element.Name.LocalName.Equals("ProjectReference", StringComparison.Ordinal))
            .Select(element => element.Attribute("Include")?.Value)
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .Select(value => value!.Replace('\\', Path.DirectorySeparatorChar))
            .Select(Path.GetFileNameWithoutExtension)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        Assert.DoesNotContain("Masterdom.Abstractions", projectReferences);
    }

    [Fact]
    public void ArchitectureReadme_ShouldReference_BusinessModuleMigrationPolicy()
    {
        var repositoryRoot = ResolveRepositoryRoot();
        var architectureReadmePath = Path.Combine(repositoryRoot, "docs", "architecture", "README.md");
        var content = File.ReadAllText(architectureReadmePath);

        Assert.Contains(
            "docs/architecture/BUSINESS_MODULE_MIGRATION_POLICY.md",
            content,
            StringComparison.Ordinal);
    }

    private static IEnumerable<string> DiscoverTestProjects()
    {
        var testsDirectory = Path.Combine(ResolveRepositoryRoot(), "tests");

        return Directory.EnumerateFiles(testsDirectory, "*.csproj", SearchOption.AllDirectories)
            .Where(path => !path.Contains("/bin/", StringComparison.Ordinal) &&
                           !path.Contains("\\bin\\", StringComparison.Ordinal))
            .Where(path => !path.Contains("/obj/", StringComparison.Ordinal) &&
                           !path.Contains("\\obj\\", StringComparison.Ordinal))
            .OrderBy(path => path, StringComparer.OrdinalIgnoreCase);
    }

    private static bool IsPureTestProject(string projectPath)
    {
        var projectName = Path.GetFileNameWithoutExtension(projectPath);

        if (!projectName.EndsWith(".Tests", StringComparison.Ordinal))
        {
            return false;
        }

        if (projectName.EndsWith(".Infrastructure.Tests", StringComparison.Ordinal) ||
            projectName.EndsWith(".BusinessIntegration.Tests", StringComparison.Ordinal))
        {
            return false;
        }

        return !ExcludedPureTestProjectNames.Contains(projectName);
    }

    private static IReadOnlyCollection<string> GetProjectReferences(string projectPath)
    {
        var projectDirectory = Path.GetDirectoryName(projectPath)!;
        var projectDocument = XDocument.Load(projectPath);

        return projectDocument
            .Descendants()
            .Where(element => element.Name.LocalName.Equals("ProjectReference", StringComparison.Ordinal))
            .Select(element => element.Attribute("Include")?.Value)
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .Select(value => value!.Replace('\\', Path.DirectorySeparatorChar))
            .Select(value => Path.GetFullPath(Path.Combine(projectDirectory, value)))
            .Select(Path.GetFileNameWithoutExtension)
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .Select(value => value!)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
    }

    private static IReadOnlyCollection<string> GetPackageReferences(string projectPath)
    {
        var projectDocument = XDocument.Load(projectPath);

        return projectDocument
            .Descendants()
            .Where(element => element.Name.LocalName.Equals("PackageReference", StringComparison.Ordinal))
            .Select(element => element.Attribute("Include")?.Value)
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .Select(value => value!)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
    }

    private static bool ProjectSourceUsesNamespace(string projectDirectory, string namespacePrefix)
    {
        return Directory.EnumerateFiles(projectDirectory, "*.cs", SearchOption.AllDirectories)
            .Where(path => !path.Contains($"{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}", StringComparison.Ordinal))
            .Where(path => !path.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}", StringComparison.Ordinal))
            .Select(File.ReadAllText)
            .Any(content => content.Contains(namespacePrefix, StringComparison.Ordinal));
    }

    private static string ResolveRepositoryRoot()
    {
        var current = new DirectoryInfo(AppContext.BaseDirectory);

        while (current is not null)
        {
            var slnxPath = Path.Combine(current.FullName, "Masterdom.slnx");
            var testsPath = Path.Combine(current.FullName, "tests");

            if (File.Exists(slnxPath) && Directory.Exists(testsPath))
            {
                return current.FullName;
            }

            current = current.Parent;
        }

        throw new DirectoryNotFoundException("Could not resolve repository root from test execution path.");
    }
}
