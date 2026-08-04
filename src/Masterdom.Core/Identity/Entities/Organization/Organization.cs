using Masterdom.Core.Primitives;

namespace Masterdom.Core.Identity.Entities.Organization;

/// <summary>
/// Represents an organization within the Masterdom platform.
/// </summary>
public sealed class Organization : AggregateRoot<OrganizationId>
{
    private readonly List<Contact> _contacts = [];
    private readonly List<Address> _addresses = [];
    private readonly List<RegistrationDocument> _registrationDocuments = [];

    private Organization(
        OrganizationId id,
        OrganizationCode code,
        OrganizationName name,
        OrganizationType type)
        : base(id)
    {
        Code = code;
        Name = name;
        Type = type;

        Status = OrganizationStatus.Active;

        Description = null;
        Remarks = null;
        Other = null;

        EffectiveFromUtc = null;
        EffectiveToUtc = null;

        DisplayOrder = 0;
        IsHidden = false;
    }

    /// <summary>
    /// Creates a new organization.
    /// </summary>
    public static Organization Create(
        OrganizationCode code,
        OrganizationName name,
        OrganizationType type)
    {
        ArgumentNullException.ThrowIfNull(code);
        ArgumentNullException.ThrowIfNull(name);
        ArgumentNullException.ThrowIfNull(type);

        return new Organization(
            OrganizationId.New(),
            code,
            name,
            type);
    }

    /// <summary>
    /// Gets the business code.
    /// </summary>
    public OrganizationCode Code { get; }

    /// <summary>
    /// Gets the organization name.
    /// </summary>
    public OrganizationName Name { get; private set; }

    /// <summary>
    /// Gets the organization type.
    /// </summary>
    public OrganizationType Type { get; private set; }

    /// <summary>
    /// Gets the current status.
    /// </summary>
    public OrganizationStatus Status { get; private set; }

    /// <summary>
    /// Gets the optional description.
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
    /// Gets whether the organization is hidden.
    /// </summary>
    public bool IsHidden { get; private set; }

    /// <summary>
    /// Gets the contacts.
    /// </summary>
    public IReadOnlyCollection<Contact> Contacts => _contacts;

    /// <summary>
    /// Gets the addresses.
    /// </summary>
    public IReadOnlyCollection<Address> Addresses => _addresses;

    /// <summary>
    /// Gets the registration documents.
    /// </summary>
    public IReadOnlyCollection<RegistrationDocument> RegistrationDocuments => _registrationDocuments;

    /// <summary>
    /// Renames the organization.
    /// </summary>
    public void Rename(OrganizationName name)
    {
        ArgumentNullException.ThrowIfNull(name);

        if (Name == name)
            return;

        Name = name;
    }

    /// <summary>
    /// Changes the organization type.
    /// </summary>
    public void ChangeType(OrganizationType type)
    {
        ArgumentNullException.ThrowIfNull(type);

        if (Type == type)
            return;

        Type = type;
    }

    /// <summary>
    /// Updates the description.
    /// </summary>
    public void ChangeDescription(string? description)
    {
        Description = string.IsNullOrWhiteSpace(description)
            ? null
            : description.Trim();
    }

    /// <summary>
    /// Updates internal remarks.
    /// </summary>
    public void ChangeRemarks(string? remarks)
    {
        Remarks = string.IsNullOrWhiteSpace(remarks)
            ? null
            : remarks.Trim();
    }

    /// <summary>
    /// Updates configurable additional information.
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
    /// Hides the organization.
    /// </summary>
    public void Hide()
    {
        IsHidden = true;
    }

    /// <summary>
    /// Makes the organization visible.
    /// </summary>
    public void Show()
    {
        IsHidden = false;
    }

    /// <summary>
    /// Activates the organization.
    /// </summary>
    public void Activate()
    {
        if (Status == OrganizationStatus.Active)
            return;

        Status = OrganizationStatus.Active;
    }

    /// <summary>
    /// Deactivates the organization.
    /// </summary>
    public void Deactivate()
    {
        if (Status == OrganizationStatus.Inactive)
            return;

        if (Status == OrganizationStatus.Archived)
            throw new InvalidOperationException(
                "An archived organization cannot be deactivated.");

        Status = OrganizationStatus.Inactive;
    }

    /// <summary>
    /// Archives the organization.
    /// </summary>
    public void Archive()
    {
        if (Status == OrganizationStatus.Archived)
            return;

        Status = OrganizationStatus.Archived;
    }

    /// <summary>
    /// Adds a contact.
    /// </summary>
    public void AddContact(Contact contact)
    {
        ArgumentNullException.ThrowIfNull(contact);

        if (_contacts.Contains(contact))
            throw new InvalidOperationException(
                "The contact already exists.");

        if (contact.IsPrimary &&
            _contacts.Any(x => x.IsPrimary))
        {
            throw new InvalidOperationException(
                "Only one primary contact is allowed.");
        }

        _contacts.Add(contact);
    }

    /// <summary>
    /// Removes a contact.
    /// </summary>
    public bool RemoveContact(Contact contact)
    {
        ArgumentNullException.ThrowIfNull(contact);

        return _contacts.Remove(contact);
    }

    /// <summary>
    /// Adds an address.
    /// </summary>
    public void AddAddress(Address address)
    {
        ArgumentNullException.ThrowIfNull(address);

        if (_addresses.Contains(address))
            throw new InvalidOperationException(
                "The address already exists.");

        if (address.IsPrimary &&
            _addresses.Any(x => x.IsPrimary))
        {
            throw new InvalidOperationException(
                "Only one primary address is allowed.");
        }

        _addresses.Add(address);
    }

    /// <summary>
    /// Removes an address.
    /// </summary>
    public bool RemoveAddress(Address address)
    {
        ArgumentNullException.ThrowIfNull(address);

        return _addresses.Remove(address);
    }

    /// <summary>
    /// Adds a registration document.
    /// </summary>
    public void AddRegistrationDocument(RegistrationDocument document)
    {
        ArgumentNullException.ThrowIfNull(document);

        if (_registrationDocuments.Any(x =>
            x.Type == document.Type &&
            x.DocumentNumber == document.DocumentNumber))
        {
            throw new InvalidOperationException(
                "The registration document already exists.");
        }

        if (document.IsPrimary &&
            _registrationDocuments.Any(x => x.IsPrimary))
        {
            throw new InvalidOperationException(
                "Only one primary registration document is allowed.");
        }

        _registrationDocuments.Add(document);
    }

    /// <summary>
    /// Removes a registration document.
    /// </summary>
    public bool RemoveRegistrationDocument(RegistrationDocument document)
    {
        ArgumentNullException.ThrowIfNull(document);

        return _registrationDocuments.Remove(document);
    }
}
