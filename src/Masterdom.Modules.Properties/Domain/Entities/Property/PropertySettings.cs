using Masterdom.Core.Primitives;

namespace Masterdom.Modules.Properties.Domain.Entities.Property;

/// <summary>
/// Represents stable operational settings of a property.
/// </summary>
public sealed class PropertySettings : ValueObject
{
    public static PropertySettings Default { get; } = new("UTC", "USD", false);

    public PropertySettings(
        string timeZoneId,
        string currencyCode,
        bool allowNegativeOccupancy)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(timeZoneId);
        ArgumentException.ThrowIfNullOrWhiteSpace(currencyCode);

        TimeZoneId = timeZoneId.Trim();
        CurrencyCode = currencyCode.Trim().ToUpperInvariant();
        AllowNegativeOccupancy = allowNegativeOccupancy;

        if (CurrencyCode.Length != 3)
        {
            throw new ArgumentException("Currency code must use ISO-4217 alpha-3 format.", nameof(currencyCode));
        }
    }

    public string TimeZoneId { get; }

    public string CurrencyCode { get; }

    public bool AllowNegativeOccupancy { get; }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return TimeZoneId;
        yield return CurrencyCode;
        yield return AllowNegativeOccupancy;
    }
}
