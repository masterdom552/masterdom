using System;
using System.Collections.Generic;
using System.Linq;

namespace Masterdom.Platform.Metadata;

/// <summary>
/// Default metadata registry for runtime metadata lifecycle.
/// </summary>
public sealed class MetadataRegistry : IMetadataRegistry
{
    private readonly InMemoryMetadataRepository _repository;

    public MetadataRegistry(IMetadataRepository? repository = null)
    {
        _repository = repository as InMemoryMetadataRepository
            ?? new InMemoryMetadataRepository(repository?.GetAll());
    }

    public void ReplaceAll(IReadOnlyList<MetadataDefinition> definitions)
    {
        ArgumentNullException.ThrowIfNull(definitions);

        MetadataValidation.ValidateAll(definitions);

        _repository.ReplaceAll(definitions);
    }

    public void RegisterRange(IReadOnlyList<MetadataDefinition> definitions)
    {
        ArgumentNullException.ThrowIfNull(definitions);

        var merged = _repository.GetAll()
            .Concat(definitions)
            .ToList();

        MetadataValidation.ValidateAll(merged);

        _repository.ReplaceAll(merged);
    }

    public IMetadataCatalog GetCatalog()
    {
        return new MetadataCatalog(_repository.GetAll());
    }
}
