using Masterdom.Modules.Billing.Application.Capabilities.ChargeComposition.Contracts;
using Masterdom.Modules.Billing.Application.Capabilities.ChargeComposition.Additional;
using Masterdom.Modules.Billing.Application.Capabilities.ChargeComposition.Arrears;
using Masterdom.Modules.Billing.Application.Capabilities.ChargeComposition.Penalties;
using Masterdom.Modules.Billing.Application.Capabilities.ChargeComposition.ReadModels;
using Masterdom.Modules.Billing.Application.Capabilities.ChargeComposition.Rent;
using Masterdom.Modules.Billing.Application.Capabilities.ChargeComposition.Shared;
using Masterdom.Modules.Billing.Application.Capabilities.ChargeComposition.Utility;

namespace Masterdom.Modules.Billing.Application.Capabilities.ChargeComposition.Pipeline;

public sealed class ChargeCompositionPipeline
{
    private readonly IReadOnlyList<IChargeSource> _chargeSources;

    public ChargeCompositionPipeline(IChargeCompositionReadService readService)
    {
        ArgumentNullException.ThrowIfNull(readService);

        _chargeSources =
        [
            new RentChargeSource(readService),
            new UtilityChargeSource(),
            new AdditionalChargeSource(),
            new CarryForwardChargeSource(),
            new PenaltyChargeSource()
        ];
    }

    public ChargeCompositionPipeline(
        IEnumerable<IChargeSource> chargeSources)
    {
        ArgumentNullException.ThrowIfNull(chargeSources);

        _chargeSources = chargeSources.ToList().AsReadOnly();
    }

    public ChargeCompositionResult Compose(ChargeCompositionRequest request)
    {
        return Compose(request, out var _);
    }

    public ChargeCompositionResult Compose(
        ChargeCompositionRequest request,
        out ChargeCompositionExecutionTrace executionTrace)
    {
        ArgumentNullException.ThrowIfNull(request);

        var candidates = new List<ChargeCandidate>();
        var executedProviders = new List<ExecutedProvider>();

        foreach (var source in _chargeSources)
        {
            candidates.AddRange(source.Compose(request));

            executedProviders.Add(new ExecutedProvider(source.ProviderId, executedProviders.Count));
        }

        executionTrace = new ChargeCompositionExecutionTrace(executedProviders);
        return new ChargeCompositionResult(candidates);
    }
}
