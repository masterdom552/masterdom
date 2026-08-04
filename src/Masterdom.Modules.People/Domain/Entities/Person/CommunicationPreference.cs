using Masterdom.Core.Primitives;

namespace Masterdom.Modules.People.Domain.Entities.Person;

/// <summary>
/// Represents a communication preference entry for a person.
/// </summary>
public sealed class CommunicationPreference : ValueObject
{
    private CommunicationPreference(string channel, bool isAllowed, string? remarks)
    {
        Channel = channel;
        IsAllowed = isAllowed;
        Remarks = remarks;
    }

    public string Channel { get; }

    public bool IsAllowed { get; }

    public string? Remarks { get; }

    public static CommunicationPreference Create(string channel, bool isAllowed, string? remarks = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(channel);

        return new CommunicationPreference(
            channel.Trim(),
            isAllowed,
            string.IsNullOrWhiteSpace(remarks) ? null : remarks.Trim());
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Channel.ToUpperInvariant();
    }
}
