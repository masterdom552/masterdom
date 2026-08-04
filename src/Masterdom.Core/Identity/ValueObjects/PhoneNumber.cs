using Masterdom.Core.Common.Primitives;

namespace Masterdom.Core.Identity.ValueObjects;

public sealed class PhoneNumber : ValueObject
{
    public string CountryCode { get; }

    public string Number { get; }

    public PhoneNumber(string countryCode, string number)
    {
        CountryCode = countryCode.Trim();

        Number = number.Trim();
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return CountryCode;
        yield return Number;
    }

    public override string ToString()
        => $"{CountryCode}{Number}";
}
