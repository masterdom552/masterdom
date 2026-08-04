using System;
using System.Collections.Generic;
using System.Linq;

namespace Masterdom.Platform.Metadata;

/// <summary>
/// Resolves effective metadata definitions by scope, version, and effective period.
/// </summary>
public sealed class MetadataResolver : IMetadataResolver
{
    private readonly IMetadataRepository _repository;

    public MetadataResolver(IMetadataRepository repository)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
    }

    public MetadataDefinition Resolve(
        MetadataKey key,
        MetadataScope scope,
        DateTime asOfUtc)
    {
        ArgumentNullException.ThrowIfNull(key);
        ArgumentNullException.ThrowIfNull(scope);

        if (asOfUtc.Kind != DateTimeKind.Utc)
        {
            throw new MetadataValidationException(
                "Metadata resolution timestamp must be UTC.");
        }

        var candidates = _repository.GetAll()
            .Where(definition => definition.Key.Equals(key))
            .Where(definition => definition.Scope.Equals(scope))
            .Where(definition => definition.Period.IsEffectiveAt(asOfUtc))
            .ToList();

        if (candidates.Count == 0)
        {
            throw new MetadataValidationException(
                $"Metadata definition not found for key '{key.Value}' in scope '{scope}'.");
        }

        return candidates
            .OrderByDescending(definition => definition.Period.EffectiveFromUtc)
            .ThenByDescending(definition => definition.Version.Value)
            .First();
    }

    public IReadOnlyList<MetadataDefinition> ResolveInheritanceChain(
        MetadataId metadataId,
        DateTime asOfUtc)
    {
        if (asOfUtc.Kind != DateTimeKind.Utc)
        {
            throw new MetadataValidationException(
                "Metadata resolution timestamp must be UTC.");
        }

        var all = _repository.GetAll()
            .Where(definition => definition.Period.IsEffectiveAt(asOfUtc))
            .ToList();

        var byId = all.ToDictionary(definition => definition.Id.Value);

        if (!byId.TryGetValue(metadataId.Value, out var current))
        {
            throw new MetadataValidationException(
                $"Metadata definition '{metadataId.Value}' is not active at '{asOfUtc:O}'.");
        }

        var chain = new List<MetadataDefinition>();
        var visited = new HashSet<Guid>();

        while (true)
        {
            if (!visited.Add(current.Id.Value))
            {
                throw new MetadataValidationException(
                    $"Circular metadata inheritance detected for '{current.Id.Value}'.");
            }

            chain.Add(current);

            if (!current.ParentId.HasValue)
            {
                break;
            }

            if (!byId.TryGetValue(current.ParentId.Value.Value, out var parent))
            {
                throw new MetadataValidationException(
                    $"Metadata parent '{current.ParentId.Value.Value}' was not found for '{current.Id.Value}'.");
            }

            current = parent;
        }

        return chain;
    }
}
