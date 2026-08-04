using System.Globalization;

namespace Masterdom.Platform.BusinessContext;

/// <summary>
/// Identifies the immutable Business Context schema version.
/// </summary>
public sealed record BusinessContextVersion(int Value)
{
    public static BusinessContextVersion BaselineV1 { get; } = new(1);

    public override string ToString()
    {
        return Value.ToString(CultureInfo.InvariantCulture);
    }
}
