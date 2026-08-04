namespace Masterdom.Modules.Billing.Application.Capabilities.ChargeComposition.Pipeline;

public sealed class ChargeCompositionExecutionTrace
{
    public ChargeCompositionExecutionTrace(IReadOnlyCollection<ExecutedProvider> executedProviders)
    {
        ArgumentNullException.ThrowIfNull(executedProviders);

        ExecutedProviders = executedProviders.ToList().AsReadOnly();
    }

    public IReadOnlyCollection<ExecutedProvider> ExecutedProviders { get; }
}
