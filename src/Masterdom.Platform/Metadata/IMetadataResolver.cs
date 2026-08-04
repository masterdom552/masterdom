using System;
using System.Collections.Generic;

namespace Masterdom.Platform.Metadata;

/// <summary>
/// Resolves effective metadata definitions.
/// </summary>
public interface IMetadataResolver
{
    MetadataDefinition Resolve(
        MetadataKey key,
        MetadataScope scope,
        DateTime asOfUtc);

    IReadOnlyList<MetadataDefinition> ResolveInheritanceChain(
        MetadataId metadataId,
        DateTime asOfUtc);
}
