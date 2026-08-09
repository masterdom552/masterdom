using Masterdom.Core.Primitives;

namespace Masterdom.Modules.CRM.Domain.Entities.Party;

/// <summary>
/// Captures actor-oriented audit information for a party.
/// </summary>
public sealed class AuditInfo : ValueObject
{
    private AuditInfo(string? createdBy, string? updatedBy)
    {
        CreatedBy = Normalize(createdBy);
        UpdatedBy = Normalize(updatedBy);
    }

    public string? CreatedBy { get; }

    public string? UpdatedBy { get; }

    public static AuditInfo Create(string? createdBy)
    {
        return new AuditInfo(createdBy, createdBy);
    }

    public AuditInfo WithUpdatedBy(string? updatedBy)
    {
        return new AuditInfo(CreatedBy, updatedBy);
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return CreatedBy?.ToUpperInvariant();
        yield return UpdatedBy?.ToUpperInvariant();
    }

    private static string? Normalize(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }
}
