using System;
using System.Collections.Generic;
using System.Linq;

namespace Masterdom.Platform.Metadata;

/// <summary>
/// In-memory metadata repository implementation.
/// </summary>
public sealed class InMemoryMetadataRepository : IMetadataRepository
{
    private List<MetadataDefinition> _definitions;

    public InMemoryMetadataRepository(IReadOnlyList<MetadataDefinition>? definitions = null)
    {
        _definitions = definitions?.ToList() ?? new List<MetadataDefinition>();
    }

    public IReadOnlyList<MetadataDefinition> GetAll()
    {
        return _definitions;
    }

    public void ReplaceAll(IReadOnlyList<MetadataDefinition> definitions)
    {
        _definitions = definitions?.ToList()
            ?? throw new ArgumentNullException(nameof(definitions));
    }
}
