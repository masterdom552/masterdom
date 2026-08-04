using Masterdom.Core.Primitives;

namespace Masterdom.Core.Identity.Entities.Organization;

/// <summary>
/// Represents a registration or statutory document belonging to an organization.
/// </summary>
public sealed class RegistrationDocument : ValueObject
{
    private RegistrationDocument(
        string type,
        string documentNumber,
        string? issuingAuthority,
        DateOnly? issueDate,
        DateOnly? expiryDate,
        bool isPrimary,
        bool isVerified,
        string? remarks,
        string? other)
    {
        Type = type;
        DocumentNumber = documentNumber;
        IssuingAuthority = issuingAuthority;
        IssueDate = issueDate;
        ExpiryDate = expiryDate;
        IsPrimary = isPrimary;
        IsVerified = isVerified;
        Remarks = remarks;
        Other = other;
    }

    /// <summary>
    /// Gets the document type.
    /// </summary>
    public string Type { get; }

    /// <summary>
    /// Gets the document number.
    /// </summary>
    public string DocumentNumber { get; }

    /// <summary>
    /// Gets the issuing authority.
    /// </summary>
    public string? IssuingAuthority { get; }

    /// <summary>
    /// Gets the issue date.
    /// </summary>
    public DateOnly? IssueDate { get; }

    /// <summary>
    /// Gets the expiry date.
    /// </summary>
    public DateOnly? ExpiryDate { get; }

    /// <summary>
    /// Gets whether this is the primary document.
    /// </summary>
    public bool IsPrimary { get; }

    /// <summary>
    /// Gets whether this document has been verified.
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
    /// Creates a registration document.
    /// </summary>
    public static RegistrationDocument Create(
        string type,
        string documentNumber,
        string? issuingAuthority = null,
        DateOnly? issueDate = null,
        DateOnly? expiryDate = null,
        bool isPrimary = false,
        bool isVerified = false,
        string? remarks = null,
        string? other = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(type);
        ArgumentException.ThrowIfNullOrWhiteSpace(documentNumber);

        if (issueDate.HasValue &&
            expiryDate.HasValue &&
            issueDate.Value > expiryDate.Value)
        {
            throw new InvalidOperationException(
                "Issue date cannot be after expiry date.");
        }

        return new RegistrationDocument(
            type.Trim(),
            documentNumber.Trim(),
            string.IsNullOrWhiteSpace(issuingAuthority)
                ? null
                : issuingAuthority.Trim(),
            issueDate,
            expiryDate,
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
    /// Marks the document as verified.
    /// </summary>
    public RegistrationDocument Verify()
    {
        if (IsVerified)
            return this;

        return new RegistrationDocument(
            Type,
            DocumentNumber,
            IssuingAuthority,
            IssueDate,
            ExpiryDate,
            IsPrimary,
            true,
            Remarks,
            Other);
    }

    /// <summary>
    /// Marks the document as primary.
    /// </summary>
    public RegistrationDocument MakePrimary()
    {
        if (IsPrimary)
            return this;

        return new RegistrationDocument(
            Type,
            DocumentNumber,
            IssuingAuthority,
            IssueDate,
            ExpiryDate,
            true,
            IsVerified,
            Remarks,
            Other);
    }

    /// <summary>
    /// Gets whether the document has expired.
    /// </summary>
    public bool IsExpired =>
        ExpiryDate.HasValue &&
        ExpiryDate.Value < DateOnly.FromDateTime(DateTime.UtcNow);

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Type.ToUpperInvariant();
        yield return DocumentNumber.ToUpperInvariant();
    }

    public override string ToString()
    {
        return $"{Type}: {DocumentNumber}";
    }
}
