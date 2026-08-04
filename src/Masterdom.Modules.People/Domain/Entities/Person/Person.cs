using Masterdom.Core.Common.Events;
using Masterdom.Core.Common.Interfaces;
using Masterdom.Modules.People.Domain.Entities.Person.Events;
using Masterdom.Core.Primitives;

namespace Masterdom.Modules.People.Domain.Entities.Person;

/// <summary>
/// Represents a person within the Masterdom platform.
/// </summary>
public sealed class Person : AggregateRoot<PersonId>, IHasDomainEvents
{
    private readonly List<Contact> _contacts = [];
    private readonly List<Address> _addresses = [];
    private readonly List<EmergencyContact> _emergencyContacts = [];
    private readonly List<GovernmentDocument> _governmentDocuments = [];
    private readonly List<CommunicationPreference> _communicationPreferences = [];
    private readonly List<PersonRelationship> _relationships = [];
    private readonly List<IDomainEvent> _domainEvents = [];

    private Person(
        PersonId id,
        PersonNumber number,
        PersonName name,
        Gender gender)
        : base(id)
    {
        Number = number;
        Name = name;
        Gender = gender;

        Status = PersonStatus.Active;

        DateOfBirth = null;
        MaritalStatus = null;
        Nationality = null;
        Occupation = null;
        PreferredLanguage = null;
        Notes = null;
        PreferredContact = null;

        Description = null;
        Remarks = null;
        Other = null;

        EffectiveFromUtc = null;
        EffectiveToUtc = null;

        DisplayOrder = 0;
        IsHidden = false;
    }

    /// <summary>
    /// Creates a new person.
    /// </summary>
    public static Person Create(
        PersonNumber number,
        PersonName name,
        Gender gender)
    {
        ArgumentNullException.ThrowIfNull(number);
        ArgumentNullException.ThrowIfNull(name);
        ArgumentNullException.ThrowIfNull(gender);

        var person = new Person(
            PersonId.New(),
            number,
            name,
            gender);

        person.Raise(new PersonCreatedDomainEvent(person.Id, person.Number));

        return person;
    }

    /// <summary>
    /// Gets the business code.
    /// </summary>
    public PersonNumber Number { get; }

    /// <summary>
    /// Gets the person's full name.
    /// </summary>
    public PersonName Name { get; private set; }

    /// <summary>
    /// Gets the gender.
    /// </summary>
    public Gender Gender { get; private set; }

    /// <summary>
    /// Gets the current status.
    /// </summary>
    public PersonStatus Status { get; private set; }

    /// <summary>
    /// Gets the date of birth.
    /// </summary>
    public DateOfBirth? DateOfBirth { get; private set; }

    /// <summary>
    /// Gets the marital status.
    /// </summary>
    public MaritalStatus? MaritalStatus { get; private set; }

    /// <summary>
    /// Gets the nationality.
    /// </summary>
    public Nationality? Nationality { get; private set; }

    /// <summary>
    /// Gets the occupation.
    /// </summary>
    public Occupation? Occupation { get; private set; }

    /// <summary>
    /// Gets the preferred language.
    /// </summary>
    public PreferredLanguage? PreferredLanguage { get; private set; }

    /// <summary>
    /// Gets the notes.
    /// </summary>
    public Notes? Notes { get; private set; }

    /// <summary>
    /// Gets the optional description.
    /// </summary>
    public string? Description { get; private set; }

    /// <summary>
    /// Gets internal remarks.
    /// </summary>
    public string? Remarks { get; private set; }

    /// <summary>
    /// Gets the configurable other information.
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
    /// Gets whether the record is hidden.
    /// </summary>
    public bool IsHidden { get; private set; }

    /// <summary>
    /// Gets the person's contacts.
    /// </summary>
    public IReadOnlyCollection<Contact> Contacts => _contacts;

    /// <summary>
    /// Gets the person's addresses.
    /// </summary>
    public IReadOnlyCollection<Address> Addresses => _addresses;

    /// <summary>
    /// Gets the person's emergency contacts.
    /// </summary>
    public IReadOnlyCollection<EmergencyContact> EmergencyContacts => _emergencyContacts;

    /// <summary>
    /// Gets the person's government documents.
    /// </summary>
    public IReadOnlyCollection<GovernmentDocument> GovernmentDocuments => _governmentDocuments;

    /// <summary>
    /// Gets communication preferences.
    /// </summary>
    public IReadOnlyCollection<CommunicationPreference> CommunicationPreferences => _communicationPreferences;

    /// <summary>
    /// Gets person relationships.
    /// </summary>
    public IReadOnlyCollection<PersonRelationship> Relationships => _relationships;

    /// <summary>
    /// Gets the preferred contact.
    /// </summary>
    public PreferredContact? PreferredContact { get; private set; }

    /// <inheritdoc />
    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    /// <summary>
    /// Renames the person.
    /// </summary>
    public void Rename(PersonName name)
    {
        ArgumentNullException.ThrowIfNull(name);

        if (Name == name)
            return;

        Name = name;
        Raise(new PersonUpdatedDomainEvent(Id));
    }

    /// <summary>
    /// Changes the gender.
    /// </summary>
    public void ChangeGender(Gender gender)
    {
        ArgumentNullException.ThrowIfNull(gender);

        if (Gender == gender)
            return;

        Gender = gender;
    }

    /// <summary>
    /// Sets the date of birth.
    /// </summary>
    public void SetDateOfBirth(DateOfBirth? dateOfBirth)
    {
        DateOfBirth = dateOfBirth;
        Raise(new PersonUpdatedDomainEvent(Id));
    }

    /// <summary>
    /// Sets the marital status.
    /// </summary>
    public void SetMaritalStatus(MaritalStatus? maritalStatus)
    {
        MaritalStatus = maritalStatus;
        Raise(new PersonUpdatedDomainEvent(Id));
    }

    /// <summary>
    /// Sets the nationality.
    /// </summary>
    public void SetNationality(Nationality? nationality)
    {
        Nationality = nationality;
        Raise(new PersonUpdatedDomainEvent(Id));
    }

    /// <summary>
    /// Sets the occupation.
    /// </summary>
    public void SetOccupation(Occupation? occupation)
    {
        Occupation = occupation;
        Raise(new PersonUpdatedDomainEvent(Id));
    }

    /// <summary>
    /// Sets the preferred language.
    /// </summary>
    public void SetPreferredLanguage(PreferredLanguage? preferredLanguage)
    {
        PreferredLanguage = preferredLanguage;
        Raise(new PersonUpdatedDomainEvent(Id));
    }

    /// <summary>
    /// Sets notes.
    /// </summary>
    public void SetNotes(Notes? notes)
    {
        Notes = notes;
        Raise(new PersonUpdatedDomainEvent(Id));
    }

    /// <summary>
    /// Updates the description.
    /// </summary>
    public void ChangeDescription(string? description)
    {
        Description = string.IsNullOrWhiteSpace(description)
            ? null
            : description.Trim();

        Raise(new PersonUpdatedDomainEvent(Id));
    }

    /// <summary>
    /// Updates internal remarks.
    /// </summary>
    public void ChangeRemarks(string? remarks)
    {
        Remarks = string.IsNullOrWhiteSpace(remarks)
            ? null
            : remarks.Trim();

        Raise(new PersonUpdatedDomainEvent(Id));
    }

    /// <summary>
    /// Updates the other information.
    /// </summary>
    public void ChangeOther(string? other)
    {
        Other = string.IsNullOrWhiteSpace(other)
            ? null
            : other.Trim();

        Raise(new PersonUpdatedDomainEvent(Id));
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
        Raise(new PersonUpdatedDomainEvent(Id));
    }

    /// <summary>
    /// Sets the display order.
    /// </summary>
    public void SetDisplayOrder(int displayOrder)
    {
        if (displayOrder < 0)
            throw new ArgumentOutOfRangeException(nameof(displayOrder));

        DisplayOrder = displayOrder;
        Raise(new PersonUpdatedDomainEvent(Id));
    }

    /// <summary>
    /// Hides the person.
    /// </summary>
    public void Hide()
    {
        IsHidden = true;
        Raise(new PersonUpdatedDomainEvent(Id));
    }

    /// <summary>
    /// Shows the person.
    /// </summary>
    public void Show()
    {
        IsHidden = false;
        Raise(new PersonUpdatedDomainEvent(Id));
    }

    /// <summary>
    /// Activates the person.
    /// </summary>
    public void Activate()
    {
        if (Status == PersonStatus.Active)
            return;

        Status = PersonStatus.Active;
        Raise(new PersonUpdatedDomainEvent(Id));
    }

    /// <summary>
    /// Deactivates the person.
    /// </summary>
    public void Deactivate()
    {
        if (Status == PersonStatus.Inactive)
            return;

        if (Status == PersonStatus.Archived)
        {
            throw new InvalidOperationException(
                "An archived person cannot be deactivated.");
        }

        Status = PersonStatus.Inactive;
        Raise(new PersonUpdatedDomainEvent(Id));
    }

    /// <summary>
    /// Archives the person.
    /// </summary>
    public void Archive()
    {
        if (Status == PersonStatus.Archived)
            return;

        Status = PersonStatus.Archived;
        Raise(new PersonUpdatedDomainEvent(Id));
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

        _contacts.Add(contact);
        Raise(new ContactAddedDomainEvent(Id, contact.Type, contact.Value));
    }

    /// <summary>
    /// Removes a contact.
    /// </summary>
    public bool RemoveContact(Contact contact)
    {
        ArgumentNullException.ThrowIfNull(contact);

        var removed = _contacts.Remove(contact);
        if (removed)
        {
            Raise(new ContactRemovedDomainEvent(Id, contact.Type, contact.Value));
        }

        return removed;
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

        _addresses.Add(address);
        Raise(new PersonUpdatedDomainEvent(Id));
    }

    /// <summary>
    /// Removes an address.
    /// </summary>
    public bool RemoveAddress(Address address)
    {
        ArgumentNullException.ThrowIfNull(address);

        var removed = _addresses.Remove(address);
        if (removed)
        {
            Raise(new PersonUpdatedDomainEvent(Id));
        }

        return removed;
    }

    /// <summary>
    /// Adds an emergency contact.
    /// </summary>
    public void AddEmergencyContact(EmergencyContact emergencyContact)
    {
        ArgumentNullException.ThrowIfNull(emergencyContact);

        if (_emergencyContacts.Contains(emergencyContact))
            throw new InvalidOperationException(
                "The emergency contact already exists.");

        _emergencyContacts.Add(emergencyContact);
        Raise(new PersonUpdatedDomainEvent(Id));
    }

    /// <summary>
    /// Removes an emergency contact.
    /// </summary>
    public bool RemoveEmergencyContact(EmergencyContact emergencyContact)
    {
        ArgumentNullException.ThrowIfNull(emergencyContact);

        var removed = _emergencyContacts.Remove(emergencyContact);
        if (removed)
        {
            Raise(new PersonUpdatedDomainEvent(Id));
        }

        return removed;
    }

    /// <summary>
    /// Adds a government document.
    /// </summary>
    public void AddGovernmentDocument(GovernmentDocument document)
    {
        ArgumentNullException.ThrowIfNull(document);

        if (_governmentDocuments.Any(x => x.Type == document.Type))
        {
            throw new InvalidOperationException(
                $"A document of type '{document.Type}' already exists.");
        }

        _governmentDocuments.Add(document);
        Raise(new IdentityDocumentAddedDomainEvent(Id, document.Type, document.DocumentNumber));
    }

    /// <summary>
    /// Removes a government document.
    /// </summary>
    public bool RemoveGovernmentDocument(GovernmentDocument document)
    {
        ArgumentNullException.ThrowIfNull(document);

        var removed = _governmentDocuments.Remove(document);
        if (removed)
        {
            Raise(new PersonUpdatedDomainEvent(Id));
        }

        return removed;
    }

    /// <summary>
    /// Sets the preferred contact.
    /// </summary>
    public void SetPreferredContact(PreferredContact preferredContact)
    {
        ArgumentNullException.ThrowIfNull(preferredContact);

        PreferredContact = preferredContact;
        Raise(new PersonUpdatedDomainEvent(Id));
    }

    /// <summary>
    /// Adds a communication preference.
    /// </summary>
    public void AddCommunicationPreference(CommunicationPreference preference)
    {
        ArgumentNullException.ThrowIfNull(preference);

        if (_communicationPreferences.Contains(preference))
        {
            return;
        }

        _communicationPreferences.Add(preference);
        Raise(new PersonUpdatedDomainEvent(Id));
    }

    /// <summary>
    /// Adds a relationship.
    /// </summary>
    public void AddRelationship(PersonRelationship relationship)
    {
        ArgumentNullException.ThrowIfNull(relationship);

        if (relationship.RelatedPersonId == Id)
        {
            throw new InvalidOperationException("A person cannot be related to itself.");
        }

        if (_relationships.Contains(relationship))
        {
            return;
        }

        _relationships.Add(relationship);
        Raise(new RelationshipAddedDomainEvent(Id, relationship.RelatedPersonId, relationship.Type));
    }

    /// <summary>
    /// Removes a relationship.
    /// </summary>
    public bool RemoveRelationship(PersonRelationship relationship)
    {
        ArgumentNullException.ThrowIfNull(relationship);

        var removed = _relationships.Remove(relationship);
        if (removed)
        {
            Raise(new PersonUpdatedDomainEvent(Id));
        }

        return removed;
    }

    /// <inheritdoc />
    public void ClearDomainEvents()
    {
        _domainEvents.Clear();
    }

    private void Raise(IDomainEvent domainEvent)
    {
        ArgumentNullException.ThrowIfNull(domainEvent);
        _domainEvents.Add(domainEvent);
    }
}
