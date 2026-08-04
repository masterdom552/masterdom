using System;
using System.Collections.Generic;
using System.Linq;
using Masterdom.Platform.Metadata;
using Microsoft.EntityFrameworkCore;

namespace Masterdom.Infrastructure.Persistence.Metadata;

/// <summary>
/// EF Core-backed metadata repository implementation.
/// </summary>
public sealed class PlatformMetadataRepository : IMetadataRepository
{
    private readonly MasterdomDbContext _dbContext;

    public PlatformMetadataRepository(MasterdomDbContext dbContext)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
    }

    public IReadOnlyList<MetadataDefinition> GetAll()
    {
        var entities = _dbContext
            .Set<PlatformMetadataDefinitionEntity>()
            .AsNoTracking()
            .OrderBy(x => x.Key)
            .ThenBy(x => x.ScopeKind)
            .ThenBy(x => x.ScopeIdentifier)
            .ThenByDescending(x => x.EffectiveFromUtc)
            .ThenByDescending(x => x.Version)
            .ToList();

        return entities
            .Select(entity => new MetadataDefinition(
                new MetadataId(entity.Id),
                new MetadataKey(entity.Key),
                (MetadataCategory)entity.Category,
                MetadataScope.Create(
                    (MetadataScopeKind)entity.ScopeKind,
                    entity.ScopeIdentifier),
                new MetadataVersion(entity.Version),
                new MetadataEffectivePeriod(
                    entity.EffectiveFromUtc,
                    entity.EffectiveToUtc),
                entity.Name,
                entity.Description,
                entity.ParentId.HasValue ? new MetadataId(entity.ParentId.Value) : null,
                entity.IsDeprecated,
                string.IsNullOrWhiteSpace(entity.ReplacedByKey)
                    ? null
                    : new MetadataKey(entity.ReplacedByKey),
                entity.Compatibility,
                entity.ChangedBy,
                entity.ChangedAtUtc))
            .ToList();
    }
}
