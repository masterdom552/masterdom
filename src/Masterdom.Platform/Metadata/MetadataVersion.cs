namespace Masterdom.Platform.Metadata;

/// <summary>
/// Represents a metadata version number.
/// </summary>
public readonly struct MetadataVersion
{
    public MetadataVersion(int value)
    {
        if (value <= 0)
        {
            throw new MetadataValidationException(
                "Metadata version must be greater than zero.");
        }

        Value = value;
    }

    public int Value { get; }
}
