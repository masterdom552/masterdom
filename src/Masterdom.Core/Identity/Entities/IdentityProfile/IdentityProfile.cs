using Masterdom.Core.Identity.Entities.Organization;
using Masterdom.Core.Identifiers;
using Masterdom.Core.Primitives;

namespace Masterdom.Core.Identity.Entities.IdentityProfile;

/// <summary>
/// Represents the canonical identity within the Masterdom platform.
/// </summary>
public sealed class IdentityProfile : AggregateRoot<IdentityProfileId>
{
    private IdentityProfile(
        IdentityProfileId id,
        IdentityProfileCode code,
        IdentityProfileType type)
        : base(id)
    {
        Code = code;
        Type = type;

        Status = IdentityProfileStatus.Active;

        PersonId = null;
        OrganizationId = null;

        DisplayName = null;
        Description = null;
        Remarks = null;
        Other = null;

        EffectiveFromUtc = null;
        EffectiveToUtc = null;

        DisplayOrder = 0;
        IsHidden = false;
    }

    /// <summary>
    /// Creates an identity profile.
    /// </summary>
    public static IdentityProfile Create(
        IdentityProfileCode code,
        IdentityProfileType type)
    {
        ArgumentNullException.ThrowIfNull(code);
        ArgumentNullException.ThrowIfNull(type);

        return new IdentityProfile(
            IdentityProfileId.New(),
            code,
            type);
    }

    /// <summary>
    /// Gets the business code.
    /// </summary>
    public IdentityProfileCode Code { get; }

    /// <summary>
    /// Gets the profile type.
    /// </summary>
    public IdentityProfileType Type { get; private set; }

    /// <summary>
    /// Gets the lifecycle status.
    /// </summary>
    public IdentityProfileStatus Status { get; private set; }

    /// <summary>
    /// Gets the linked person identifier.
    /// </summary>
    public PersonId? PersonId { get; private set; }

    /// <summary>
    /// Gets the linked organization identifier.
    /// </summary>
    public OrganizationId? OrganizationId { get; private set; }

    /// <summary>
    /// Gets the display name.
    /// </summary>
    public string? DisplayName { get; private set; }

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
    /// Gets whether the profile is hidden.
    /// </summary>
    public bool IsHidden { get; private set; }

    /// <summary>
    /// Links the profile to a person.
    /// </summary>
    public void LinkPerson(PersonId personId)
    {
        ArgumentNullException.ThrowIfNull(personId);

        PersonId = personId;
        OrganizationId = null;
    }

    /// <summary>
    /// Links the profile to an organization.
    /// </summary>
    public void LinkOrganization(OrganizationId organizationId)
    {
        ArgumentNullException.ThrowIfNull(organizationId);

        OrganizationId = organizationId;
        PersonId = null;
    }

    /// <summary>
    /// Sets the display name.
    /// </summary>
    public void SetDisplayName(string? displayName)
    {
        DisplayName = string.IsNullOrWhiteSpace(displayName)
            ? null
            : displayName.Trim();
    }

    /// <summary>
    /// Changes the profile type.
    /// </summary>
    public void ChangeType(IdentityProfileType type)
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
    /// Hides the profile.
    /// </summary>
    public void Hide()
    {
        IsHidden = true;
    }

    /// <summary>
    /// Shows the profile.
    /// </summary>
    public void Show()
    {
        IsHidden = false;
    }

    /// <summary>
    /// Activates the profile.
    /// </summary>
    public void Activate()
    {
        if (Status == IdentityProfileStatus.Active)
            return;

        Status = IdentityProfileStatus.Active;
    }

    /// <summary>
    /// Deactivates the profile.
    /// </summary>
    public void Deactivate()
    {
        if (Status == IdentityProfileStatus.Inactive)
            return;

        if (Status == IdentityProfileStatus.Archived)
            throw new InvalidOperationException(
                "An archived profile cannot be deactivated.");

        Status = IdentityProfileStatus.Inactive;
    }

    /// <summary>
    /// Archives the profile.
    /// </summary>
    public void Archive()
    {
        if (Status == IdentityProfileStatus.Archived)
            return;

        Status = IdentityProfileStatus.Archived;
    }
}
