using System.Collections.Generic;

namespace Masterdom.Platform.Metadata;

/// <summary>
/// Represents an immutable metadata catalog view.
/// </summary>
public interface IMetadataCatalog
{
    IReadOnlyList<MetadataDefinition> Definitions { get; }
}
