using Masterdom.Platform.CalculationEngine.Contracts;
using Masterdom.Platform.CalculationEngine.Execution;
using Microsoft.Extensions.DependencyInjection;

namespace Masterdom.Platform.CalculationEngine;

public static class CalculationEngineServiceCollectionExtensions
{
    public static IServiceCollection AddCalculationEngine(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddSingleton<ICalculationRuntime>(_ => new CalculationRuntime());

        return services;
    }
}
