using Masterdom.Core.Primitives;

namespace Masterdom.Modules.People.Domain.Entities.Person;

/// <summary>
/// Represents an emergency contact for a person.
/// </summary>
public sealed class EmergencyContact : ValueObject
{
    private EmergencyContact(
        FullName fullName,
        string relationship,
        string mobileNumber,
        string? alternateMobileNumber,
        string? emailAddress,
        Address? address,
        bool isPrimary,
        string? remarks,
        string? other)
    {
        FullName = fullName;
        Relationship = relationship;
        MobileNumber = mobileNumber;
        AlternateMobileNumber = alternateMobileNumber;
        EmailAddress = emailAddress;
        Address = address;
        IsPrimary = isPrimary;
        Remarks = remarks;
        Other = other;
    }

    /// <summary>
    /// Gets the contact's name.
    /// </summary>
    public FullName FullName { get; }

    /// <summary>
    /// Gets the relationship with the person.
    /// </summary>
    public string Relationship { get; }

    /// <summary>
    /// Gets the primary mobile number.
    /// </summary>
    public string MobileNumber { get; }

    /// <summary>
    /// Gets the alternate mobile number.
    /// </summary>
    public string? AlternateMobileNumber { get; }

    /// <summary>
    /// Gets the email address.
    /// </summary>
    public string? EmailAddress { get; }

    /// <summary>
    /// Gets the address.
    /// </summary>
    public Address? Address { get; }

    /// <summary>
    /// Gets whether this is the primary emergency contact.
    /// </summary>
    public bool IsPrimary { get; }

    /// <summary>
    /// Gets internal remarks.
    /// </summary>
    public string? Remarks { get; }

    /// <summary>
    /// Gets configurable additional information.
    /// </summary>
    public string? Other { get; }

    /// <summary>
    /// Creates a new emergency contact.
    /// </summary>
    public static EmergencyContact Create(
        FullName fullName,
        string relationship,
        string mobileNumber,
        string? alternateMobileNumber = null,
        string? emailAddress = null,
        Address? address = null,
        bool isPrimary = false,
        string? remarks = null,
        string? other = null)
    {
        ArgumentNullException.ThrowIfNull(fullName);
        ArgumentException.ThrowIfNullOrWhiteSpace(relationship);
        ArgumentException.ThrowIfNullOrWhiteSpace(mobileNumber);

        return new EmergencyContact(
            fullName,
            relationship.Trim(),
            mobileNumber.Trim(),
            string.IsNullOrWhiteSpace(alternateMobileNumber)
                ? null
                : alternateMobileNumber.Trim(),
            string.IsNullOrWhiteSpace(emailAddress)
                ? null
                : emailAddress.Trim(),
            address,
            isPrimary,
            string.IsNullOrWhiteSpace(remarks)
                ? null
                : remarks.Trim(),
            string.IsNullOrWhiteSpace(other)
                ? null
                : other.Trim());
    }

    /// <summary>
    /// Marks this as the primary emergency contact.
    /// </summary>
    public EmergencyContact MakePrimary()
    {
        if (IsPrimary)
            return this;

        return new EmergencyContact(
            FullName,
            Relationship,
            MobileNumber,
            AlternateMobileNumber,
            EmailAddress,
            Address,
            true,
            Remarks,
            Other);
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return FullName;
        yield return Relationship.ToUpperInvariant();
        yield return MobileNumber.ToUpperInvariant();
    }

    public override string ToString()
    {
        return $"{FullName} ({Relationship})";
    }
}
