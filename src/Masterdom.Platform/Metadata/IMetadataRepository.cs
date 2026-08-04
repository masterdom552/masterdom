using System.Collections.Generic;

namespace Masterdom.Platform.Metadata;

/// <summary>
/// Provides read access to metadata definitions.
/// </summary>
public interface IMetadataRepository
{
    IReadOnlyList<MetadataDefinition> GetAll();
}
