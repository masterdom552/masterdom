using System.Collections.Generic;

namespace Masterdom.Platform.Metadata;

/// <summary>
/// Registers metadata definitions into the runtime repository.
/// </summary>
public interface IMetadataRegistry
{
    void ReplaceAll(IReadOnlyList<MetadataDefinition> definitions);

    void RegisterRange(IReadOnlyList<MetadataDefinition> definitions);

    IMetadataCatalog GetCatalog();
}
