using Masterdom.Core.Primitives;

namespace Masterdom.Modules.UtilityRating.Domain.Entities.UtilityRating;

public sealed class TariffReference : ValueObject
{
    private TariffReference(string tariffCode, int tariffVersion)
    {
        TariffCode = tariffCode;
        TariffVersion = tariffVersion;
    }

    public string TariffCode { get; }

    public int TariffVersion { get; }

    public static TariffReference Create(string tariffCode, int tariffVersion)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(tariffCode);

        var normalizedCode = tariffCode.Trim().ToUpperInvariant();
        if (normalizedCode.Length > 50)
        {
            throw new ArgumentException("Tariff code cannot exceed 50 characters.", nameof(tariffCode));
        }

        if (tariffVersion <= 0)
        {
            throw new InvalidOperationException("Tariff version must be greater than zero.");
        }

        return new TariffReference(normalizedCode, tariffVersion);
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return TariffCode;
        yield return TariffVersion;
    }
}
