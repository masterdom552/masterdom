using System.Collections.ObjectModel;
using System.Text.Json.Serialization;

namespace Masterdom.Core.Identity.ValueObjects;

/// <summary>
/// Represents the scope constraints of a delegated authority.
/// </summary>
public sealed record DelegationScope
{
    /// <summary>
    /// Property IDs within which the delegation applies.
    /// Null means no property restriction.
    /// </summary>
    public IReadOnlyCollection<Guid>? PropertyIds { get; }

    /// <summary>
    /// Maximum authority level the delegation can exercise.
    /// Null means no additional level cap (uses delegator's effective level).
    /// </summary>
    public int? EffectiveLevel { get; }

    [JsonConstructor]
    private DelegationScope(IReadOnlyCollection<Guid>? propertyIds, int? effectiveLevel)
    {
        PropertyIds = propertyIds;
        EffectiveLevel = effectiveLevel;
    }

    /// <summary>
    /// Creates a delegation scope with no restrictions.
    /// </summary>
    public static DelegationScope Unrestricted() => new(null, null);

    /// <summary>
    /// Creates a delegation scope with property restrictions.
    /// </summary>
    public static DelegationScope WithProperties(IEnumerable<Guid> propertyIds)
    {
        ArgumentNullException.ThrowIfNull(propertyIds);

        var properties = propertyIds.ToList();
        if (properties.Count == 0)
            throw new ArgumentException("At least one property must be specified.", nameof(propertyIds));

        return new DelegationScope(new ReadOnlyCollection<Guid>(properties), null);
    }

    /// <summary>
    /// Creates a delegation scope with an effective authority level cap.
    /// </summary>
    public static DelegationScope WithEffectiveLevel(int level)
    {
        if (level < 1)
            throw new ArgumentException("Effective level must be at least 1.", nameof(level));

        return new DelegationScope(null, level);
    }

    /// <summary>
    /// Creates a delegation scope with both property and level restrictions.
    /// </summary>
    public static DelegationScope WithPropertiesAndLevel(IEnumerable<Guid> propertyIds, int level)
    {
        ArgumentNullException.ThrowIfNull(propertyIds);

        if (level < 1)
            throw new ArgumentException("Effective level must be at least 1.", nameof(level));

        var properties = propertyIds.ToList();
        if (properties.Count == 0)
            throw new ArgumentException("At least one property must be specified.", nameof(propertyIds));

        return new DelegationScope(new ReadOnlyCollection<Guid>(properties), level);
    }

    /// <summary>
    /// Determines whether a property is within this scope.
    /// </summary>
    public bool ContainsProperty(Guid propertyId)
    {
        if (PropertyIds == null)
            return true;

        return PropertyIds.Contains(propertyId);
    }

    /// <summary>
    /// Determines whether an authority level is within this scope's cap.
    /// </summary>
    public bool IsLevelWithinScope(int level)
    {
        if (EffectiveLevel == null)
            return true;

        return level <= EffectiveLevel.Value;
    }
}
