using Masterdom.Core.Identity.Entities.IdentityProfile;
using Masterdom.Core.Primitives;

namespace Masterdom.Core.Identity.Entities.Relationship;

/// <summary>
/// Represents a relationship between two identity profiles.
/// </summary>
public sealed class Relationship : AggregateRoot<RelationshipId>
{
    private Relationship(
        RelationshipId id,
        RelationshipCode code,
        IdentityProfileId fromIdentityProfileId,
        IdentityProfileId toIdentityProfileId,
        RelationshipType type)
        : base(id)
    {
        Code = code;
        FromIdentityProfileId = fromIdentityProfileId;
        ToIdentityProfileId = toIdentityProfileId;
        Type = type;

        Status = RelationshipStatus.Active;

        Description = null;
        Remarks = null;
        Other = null;

        EffectiveFromUtc = null;
        EffectiveToUtc = null;

        DisplayOrder = 0;
        IsHidden = false;
    }

    /// <summary>
    /// Creates a new relationship.
    /// </summary>
    public static Relationship Create(
        RelationshipCode code,
        IdentityProfileId fromIdentityProfileId,
        IdentityProfileId toIdentityProfileId,
        RelationshipType type)
    {
        ArgumentNullException.ThrowIfNull(code);
        ArgumentNullException.ThrowIfNull(fromIdentityProfileId);
        ArgumentNullException.ThrowIfNull(toIdentityProfileId);
        ArgumentNullException.ThrowIfNull(type);

        if (fromIdentityProfileId == toIdentityProfileId)
        {
            throw new InvalidOperationException(
                "A relationship cannot reference the same identity profile.");
        }

        return new Relationship(
            RelationshipId.New(),
            code,
            fromIdentityProfileId,
            toIdentityProfileId,
            type);
    }

    /// <summary>
    /// Gets the business code.
    /// </summary>
    public RelationshipCode Code { get; }

    /// <summary>
    /// Gets the source identity profile.
    /// </summary>
    public IdentityProfileId FromIdentityProfileId { get; }

    /// <summary>
    /// Gets the destination identity profile.
    /// </summary>
    public IdentityProfileId ToIdentityProfileId { get; }

    /// <summary>
    /// Gets the relationship type.
    /// </summary>
    public RelationshipType Type { get; private set; }

    /// <summary>
    /// Gets the lifecycle status.
    /// </summary>
    public RelationshipStatus Status { get; private set; }

    /// <summary>
    /// Gets the description.
    /// </summary>
    public string? Description { get; private set; }

    /// <summary>
    /// Gets internal remarks.
    /// </summary>
    public string? Remarks { get; private set; }

    /// <summary>
    /// Gets configurable additional information.
    /// </summary>
    public string? Other { get; private set; }

    /// <summary>
    /// Gets the effective start date.
    /// </summary>
    public DateTime? EffectiveFromUtc { get; private set; }

    /// <summary>
    /// Gets the effective end date.
    /// </summary>
    public DateTime? EffectiveToUtc { get; private set; }

    /// <summary>
    /// Gets the display order.
    /// </summary>
    public int DisplayOrder { get; private set; }

    /// <summary>
    /// Gets whether the relationship is hidden.
    /// </summary>
    public bool IsHidden { get; private set; }

    /// <summary>
    /// Changes the relationship type.
    /// </summary>
    public void ChangeType(RelationshipType type)
    {
        ArgumentNullException.ThrowIfNull(type);

        if (Type == type)
            return;

        Type = type;
    }

    /// <summary>
    /// Changes the description.
    /// </summary>
    public void ChangeDescription(string? description)
    {
        Description = string.IsNullOrWhiteSpace(description)
            ? null
            : description.Trim();
    }

    /// <summary>
    /// Changes internal remarks.
    /// </summary>
    public void ChangeRemarks(string? remarks)
    {
        Remarks = string.IsNullOrWhiteSpace(remarks)
            ? null
            : remarks.Trim();
    }

    /// <summary>
    /// Changes the configurable other field.
    /// </summary>
    public void ChangeOther(string? other)
    {
        Other = string.IsNullOrWhiteSpace(other)
            ? null
            : other.Trim();
    }

    /// <summary>
    /// Sets the effective period.
    /// </summary>
    public void SetEffectivePeriod(DateTime? fromUtc, DateTime? toUtc)
    {
        if (fromUtc.HasValue &&
            toUtc.HasValue &&
            fromUtc > toUtc)
        {
            throw new InvalidOperationException(
                "EffectiveFromUtc cannot be after EffectiveToUtc.");
        }

        EffectiveFromUtc = fromUtc;
        EffectiveToUtc = toUtc;
    }

    /// <summary>
    /// Sets the display order.
    /// </summary>
    public void SetDisplayOrder(int displayOrder)
    {
        if (displayOrder < 0)
            throw new ArgumentOutOfRangeException(nameof(displayOrder));

        DisplayOrder = displayOrder;
    }

    /// <summary>
    /// Hides the relationship.
    /// </summary>
    public void Hide()
    {
        IsHidden = true;
    }

    /// <summary>
    /// Shows the relationship.
    /// </summary>
    public void Show()
    {
        IsHidden = false;
    }

    /// <summary>
    /// Activates the relationship.
    /// </summary>
    public void Activate()
    {
        if (Status == RelationshipStatus.Active)
            return;

        Status = RelationshipStatus.Active;
    }

    /// <summary>
    /// Deactivates the relationship.
    /// </summary>
    public void Deactivate()
    {
        if (Status == RelationshipStatus.Inactive)
            return;

        if (Status == RelationshipStatus.Archived)
            throw new InvalidOperationException(
                "An archived relationship cannot be deactivated.");

        Status = RelationshipStatus.Inactive;
    }

    /// <summary>
    /// Archives the relationship.
    /// </summary>
    public void Archive()
    {
        if (Status == RelationshipStatus.Archived)
            return;

        Status = RelationshipStatus.Archived;
    }
}
