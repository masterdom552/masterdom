namespace Masterdom.Modules.Billing.Application.Capabilities.ChargeComposition.Contracts;

public sealed class ChargeCompositionResult
{
    public ChargeCompositionResult(IReadOnlyCollection<ChargeCandidate> chargeCandidates)
    {
        ArgumentNullException.ThrowIfNull(chargeCandidates);

        ChargeCandidates = chargeCandidates.ToList().AsReadOnly();
    }

    public IReadOnlyCollection<ChargeCandidate> ChargeCandidates { get; }
}
