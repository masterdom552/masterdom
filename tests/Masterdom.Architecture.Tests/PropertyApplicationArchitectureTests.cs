using System.Reflection;
using Masterdom.Infrastructure.Persistence;
using Masterdom.Modules.Properties.Application.Services;
using Masterdom.Modules.Properties.Application.Support;

namespace Masterdom.Architecture.Tests;

public sealed class PropertyApplicationArchitectureTests
{
    [Fact]
    public void PropertyApplication_ShouldNotReferenceInfrastructureAssembly()
    {
        var referencedAssemblyNames = typeof(IPropertyApplicationService).Assembly
            .GetReferencedAssemblies()
            .Select(x => x.Name)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        Assert.DoesNotContain("Masterdom.Infrastructure", referencedAssemblyNames);
    }

    [Fact]
    public void PropertyApplication_ShouldNotUseEfCoreTypesInApplicationSurface()
    {
        var applicationAssembly = typeof(IPropertyApplicationService).Assembly;

        var applicationTypes = applicationAssembly.GetTypes()
            .Where(type => type.Namespace is not null)
            .Where(type => type.Namespace.StartsWith("Masterdom.Modules.Properties.Application", StringComparison.Ordinal))
            .ToList();

        Assert.NotEmpty(applicationTypes);

        foreach (var type in applicationTypes)
        {
            var memberTypes = new List<Type>();

            memberTypes.AddRange(type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static)
                .Select(field => field.FieldType));

            memberTypes.AddRange(type.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static)
                .Select(property => property.PropertyType));

            memberTypes.AddRange(type.GetConstructors(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                .SelectMany(ctor => ctor.GetParameters())
                .Select(parameter => parameter.ParameterType));

            memberTypes.AddRange(type.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static)
                .Select(method => method.ReturnType));

            memberTypes.AddRange(type.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static)
                .SelectMany(method => method.GetParameters())
                .Select(parameter => parameter.ParameterType));

            Assert.DoesNotContain(memberTypes, memberType =>
                (memberType.Namespace ?? string.Empty)
                    .StartsWith("Microsoft.EntityFrameworkCore", StringComparison.Ordinal));
        }
    }

    [Fact]
    public void PropertyApplication_ShouldNotUseDbContextDirectly()
    {
        var applicationAssembly = typeof(IPropertyApplicationService).Assembly;

        var applicationTypes = applicationAssembly.GetTypes()
            .Where(type => type.Namespace is not null)
            .Where(type => type.Namespace.StartsWith("Masterdom.Modules.Properties.Application", StringComparison.Ordinal))
            .ToList();

        Assert.NotEmpty(applicationTypes);

        foreach (var type in applicationTypes)
        {
            var constructorParameterTypes = type
                .GetConstructors(BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic)
                .SelectMany(ctor => ctor.GetParameters())
                .Select(parameter => parameter.ParameterType)
                .ToList();

            Assert.DoesNotContain(typeof(MasterdomDbContext), constructorParameterTypes);
        }
    }

    [Fact]
    public void PropertyCommandHandlers_ShouldDependOnApplicationServiceBoundary()
    {
        var handlerTypes = typeof(IPropertyApplicationService).Assembly.GetTypes()
            .Where(type => type.IsClass && !type.IsAbstract)
            .Where(type => type.Name.EndsWith("CommandHandler", StringComparison.Ordinal))
            .ToList();

        Assert.NotEmpty(handlerTypes);

        foreach (var handlerType in handlerTypes)
        {
            var constructors = handlerType.GetConstructors(BindingFlags.Public | BindingFlags.Instance);
            Assert.NotEmpty(constructors);

            var parameterTypes = constructors
                .SelectMany(x => x.GetParameters())
                .Select(x => x.ParameterType)
                .ToList();

            Assert.Contains(typeof(IPropertyApplicationService), parameterTypes);
        }
    }

    [Fact]
    public void PropertyApplication_ShouldNotDependOnUiAssemblies()
    {
        var referencedAssemblyNames = typeof(ICommandHandler<,>).Assembly
            .GetReferencedAssemblies()
            .Select(x => x.Name)
            .Where(x => x is not null)
            .Cast<string>()
            .ToList();

        Assert.DoesNotContain(referencedAssemblyNames, x =>
            x.StartsWith("Microsoft.AspNetCore.Mvc", StringComparison.OrdinalIgnoreCase) ||
            x.StartsWith("System.Windows", StringComparison.OrdinalIgnoreCase));
    }
}
