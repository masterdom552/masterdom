using Masterdom.Core.Primitives;

namespace Masterdom.Modules.PolicyFramework.Domain.Entities.PolicyFramework;

public sealed class PolicyMetadata : ValueObject
{
    private readonly IReadOnlyDictionary<string, string> _attributes;

    private PolicyMetadata(IReadOnlyDictionary<string, string> attributes)
    {
        _attributes = attributes;
    }

    public IReadOnlyDictionary<string, string> Attributes => _attributes;

    public static PolicyMetadata Create(IReadOnlyDictionary<string, string> attributes)
    {
        ArgumentNullException.ThrowIfNull(attributes);

        var normalized = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        foreach (var kvp in attributes)
        {
            if (string.IsNullOrWhiteSpace(kvp.Key))
            {
                throw new InvalidOperationException("Policy metadata keys cannot be empty.");
            }

            normalized[kvp.Key.Trim()] = kvp.Value?.Trim() ?? string.Empty;
        }

        return new PolicyMetadata(normalized);
    }

    public static PolicyMetadata Empty()
    {
        return new PolicyMetadata(new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase));
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        foreach (var kvp in _attributes.OrderBy(x => x.Key, StringComparer.OrdinalIgnoreCase))
        {
            yield return kvp.Key.ToUpperInvariant();
            yield return kvp.Value;
        }
    }
}
