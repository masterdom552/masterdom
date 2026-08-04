using Masterdom.Core.Primitives;

namespace Masterdom.Core.Identity.Entities.Organization;

/// <summary>
/// Represents an organization's contact information.
/// </summary>
public sealed class Contact : ValueObject
{
    private Contact(
        string type,
        string value,
        bool isPrimary,
        bool isVerified,
        string? remarks,
        string? other)
    {
        Type = type;
        Value = value;
        IsPrimary = isPrimary;
        IsVerified = isVerified;
        Remarks = remarks;
        Other = other;
    }

    /// <summary>
    /// Gets the contact type.
    /// </summary>
    public string Type { get; }

    /// <summary>
    /// Gets the contact value.
    /// </summary>
    public string Value { get; }

    /// <summary>
    /// Gets whether this is the primary contact.
    /// </summary>
    public bool IsPrimary { get; }

    /// <summary>
    /// Gets whether this contact has been verified.
    /// </summary>
    public bool IsVerified { get; }

    /// <summary>
    /// Gets internal remarks.
    /// </summary>
    public string? Remarks { get; }

    /// <summary>
    /// Gets configurable additional information.
    /// </summary>
    public string? Other { get; }

    /// <summary>
    /// Creates a contact.
    /// </summary>
    public static Contact Create(
        string type,
        string value,
        bool isPrimary = false,
        bool isVerified = false,
        string? remarks = null,
        string? other = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(type);
        ArgumentException.ThrowIfNullOrWhiteSpace(value);

        return new Contact(
            type.Trim(),
            value.Trim(),
            isPrimary,
            isVerified,
            string.IsNullOrWhiteSpace(remarks)
                ? null
                : remarks.Trim(),
            string.IsNullOrWhiteSpace(other)
                ? null
                : other.Trim());
    }

    /// <summary>
    /// Marks the contact as primary.
    /// </summary>
    public Contact MakePrimary()
    {
        if (IsPrimary)
            return this;

        return new Contact(
            Type,
            Value,
            true,
            IsVerified,
            Remarks,
            Other);
    }

    /// <summary>
    /// Marks the contact as verified.
    /// </summary>
    public Contact Verify()
    {
        if (IsVerified)
            return this;

        return new Contact(
            Type,
            Value,
            IsPrimary,
            true,
            Remarks,
            Other);
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Type.ToUpperInvariant();
        yield return Value.ToUpperInvariant();
    }

    public override string ToString()
    {
        return $"{Type}: {Value}";
    }
}
