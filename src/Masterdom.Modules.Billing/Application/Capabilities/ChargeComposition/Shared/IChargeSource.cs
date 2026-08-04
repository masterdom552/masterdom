using Masterdom.Modules.Billing.Application.Capabilities.ChargeComposition.Contracts;

namespace Masterdom.Modules.Billing.Application.Capabilities.ChargeComposition.Shared;

public interface IChargeSource
{
    string ProviderId { get; }

    IReadOnlyCollection<ChargeCandidate> Compose(ChargeCompositionRequest request);
}
