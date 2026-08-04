using Masterdom.Core.Primitives;

namespace Masterdom.Modules.Lease.Domain.Entities.Lease;

/// <summary>
/// Represents an individual clause in a lease version.
/// </summary>
public sealed class LeaseClause : ValueObject
{
    private LeaseClause(string code, string text)
    {
        Code = code;
        Text = text;
    }

    public string Code { get; }

    public string Text { get; }

    public static LeaseClause Create(string code, string text)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(code);
        ArgumentException.ThrowIfNullOrWhiteSpace(text);

        var normalizedCode = code.Trim().ToUpperInvariant();
        var normalizedText = text.Trim();

        if (normalizedCode.Length > 50)
        {
            throw new InvalidOperationException("Clause code cannot exceed 50 characters.");
        }

        if (normalizedText.Length > 4000)
        {
            throw new InvalidOperationException("Clause text cannot exceed 4000 characters.");
        }

        return new LeaseClause(normalizedCode, normalizedText);
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Code;
        yield return Text;
    }
}
