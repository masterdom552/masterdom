using Masterdom.Core.Primitives;

namespace Masterdom.Modules.SubsidyOptimization.Domain.Entities.SubsidyOptimization;

public sealed class MeterGroup : ValueObject
{
    private MeterGroup(MeterGroupReference reference, string displayName)
    {
        Reference = reference;
        DisplayName = displayName;
    }

    public MeterGroupReference Reference { get; }

    public string DisplayName { get; }

    public static MeterGroup Create(MeterGroupReference reference, string displayName)
    {
        ArgumentNullException.ThrowIfNull(reference);
        ArgumentException.ThrowIfNullOrWhiteSpace(displayName);

        return new MeterGroup(reference, displayName.Trim());
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Reference;
        yield return DisplayName;
    }
}
