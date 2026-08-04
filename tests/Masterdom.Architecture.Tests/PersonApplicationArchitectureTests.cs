using System.Reflection;
using Masterdom.Infrastructure.Persistence;
using Masterdom.Modules.People.Application.Services;
using Masterdom.Modules.People.Application.Support;

namespace Masterdom.Architecture.Tests;

public sealed class PersonApplicationArchitectureTests
{
    [Fact]
    public void PersonApplication_ShouldNotUseDbContextDirectly()
    {
        var assembly = typeof(IPersonApplicationService).Assembly;

        var applicationTypes = assembly.GetTypes()
            .Where(type => type.Namespace is not null)
            .Where(type => type.Namespace.StartsWith("Masterdom.Modules.People.Application", StringComparison.Ordinal))
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
    public void PersonCommandHandlers_ShouldDependOnApplicationServiceBoundary()
    {
        var handlerTypes = typeof(IPersonApplicationService).Assembly.GetTypes()
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

            Assert.Contains(typeof(IPersonApplicationService), parameterTypes);
        }
    }

    [Fact]
    public void PersonApplication_ShouldNotDependOnUiAssemblies()
    {
        var references = typeof(ICommandHandler<,>).Assembly
            .GetReferencedAssemblies()
            .Select(x => x.Name)
            .Where(x => x is not null)
            .Cast<string>()
            .ToList();

        Assert.DoesNotContain(references, x =>
            x.StartsWith("Microsoft.AspNetCore.Mvc", StringComparison.OrdinalIgnoreCase) ||
            x.StartsWith("System.Windows", StringComparison.OrdinalIgnoreCase));
    }
}
