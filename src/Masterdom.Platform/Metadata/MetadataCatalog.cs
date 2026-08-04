using System;
using System.Collections.Generic;
using System.Linq;

namespace Masterdom.Platform.Metadata;

/// <summary>
/// Represents an immutable snapshot of metadata definitions.
/// </summary>
public sealed class MetadataCatalog : IMetadataCatalog
{
    public MetadataCatalog(IEnumerable<MetadataDefinition> definitions)
    {
        ArgumentNullException.ThrowIfNull(definitions);

        Definitions = definitions.ToList();
    }

    public IReadOnlyList<MetadataDefinition> Definitions { get; }
}
