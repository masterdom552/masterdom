using Masterdom.Core.Primitives;

namespace Masterdom.Core.Financial.ValueObjects;

public sealed class Currency : ValueObject
{
    public string Code { get; }

    public string? Symbol { get; }

    private Currency(string code, string? symbol)
    {
        Code = code;
        Symbol = symbol;
    }

    public static Currency Create(string code, string? symbol = null)
    {
        if (string.IsNullOrWhiteSpace(code))
            throw new ArgumentException("Currency code is required.", nameof(code));

        string normalizedCode = code.Trim().ToUpperInvariant();

        if (normalizedCode.Length != 3)
            throw new ArgumentException("Currency code must be a 3-letter ISO code.", nameof(code));

        string? normalizedSymbol = string.IsNullOrWhiteSpace(symbol)
            ? null
            : symbol.Trim();

        return new Currency(normalizedCode, normalizedSymbol);
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Code;
        yield return Symbol;
    }
}
