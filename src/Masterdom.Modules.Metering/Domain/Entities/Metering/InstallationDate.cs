using Masterdom.Core.Primitives;

namespace Masterdom.Modules.Metering.Domain.Entities.Metering;

public sealed class InstallationDate : ValueObject
{
    private InstallationDate(DateOnly value)
    {
        Value = value;
    }

    public DateOnly Value { get; }

    public static InstallationDate Create(DateOnly value)
    {
        return new InstallationDate(value);
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }
}
