using System;
using System.Collections.Generic;
using System.Linq;
using Masterdom.Platform.Configuration;
using Microsoft.EntityFrameworkCore;

namespace Masterdom.Infrastructure.Persistence.Configuration;

/// <summary>
/// EF Core-backed implementation of versioned configuration repository.
/// </summary>
public sealed class PlatformConfigurationRepository : IConfigurationRepository
{
    private readonly MasterdomDbContext _dbContext;

    public PlatformConfigurationRepository(MasterdomDbContext dbContext)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
    }

    public IReadOnlyList<ConfigurationRecord> GetAll()
    {
        var entities = _dbContext
            .Set<PlatformConfigurationRecordEntity>()
            .AsNoTracking()
            .OrderBy(x => x.Key)
            .ThenBy(x => x.ScopeKind)
            .ThenBy(x => x.ScopeIdentifier)
            .ThenByDescending(x => x.EffectiveFromUtc)
            .ThenByDescending(x => x.Version)
            .ToList();

        return entities
            .Select(entity => new ConfigurationRecord(
                new ConfigurationId(entity.Id),
                new ConfigurationKey(entity.Key),
                ConfigurationScope.Create(
                    (ConfigurationScopeKind)entity.ScopeKind,
                    entity.ScopeIdentifier),
                new ConfigurationVersion(entity.Version),
                new ConfigurationValue(entity.Value),
                new EffectivePeriod(entity.EffectiveFromUtc, entity.EffectiveToUtc),
                entity.ChangedBy,
                entity.Reason,
                entity.ChangedAtUtc))
            .ToList();
    }
}
