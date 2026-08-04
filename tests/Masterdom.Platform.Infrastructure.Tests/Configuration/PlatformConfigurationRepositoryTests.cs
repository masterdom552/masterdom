using System;
using Masterdom.Infrastructure.Persistence;
using Masterdom.Infrastructure.Persistence.Configuration;
using Masterdom.Platform.Configuration;
using Microsoft.EntityFrameworkCore;

namespace Masterdom.Platform.Tests.Configuration;

public sealed class PlatformConfigurationRepositoryTests
{
    [Fact]
    public void GetAll_ShouldMapPersistedEntitiesToConfigurationRecords()
    {
        var options = new DbContextOptionsBuilder<MasterdomDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        using var dbContext = new MasterdomDbContext(options);

        var changedAt = DateTime.SpecifyKind(new DateTime(2026, 1, 5), DateTimeKind.Utc);

        dbContext.PlatformConfigurationRecords.Add(new PlatformConfigurationRecordEntity
        {
            Id = Guid.NewGuid(),
            Key = "billing.currency",
            ScopeKind = (int)ConfigurationScopeKind.Module,
            ScopeIdentifier = "billing",
            Version = 3,
            Value = "USD",
            EffectiveFromUtc = changedAt,
            EffectiveToUtc = null,
            ChangedBy = "tester",
            Reason = "initial setup",
            ChangedAtUtc = changedAt
        });

        dbContext.SaveChanges();

        var repository = new PlatformConfigurationRepository(dbContext);

        var records = repository.GetAll();

        var record = Assert.Single(records);

        Assert.NotEqual(Guid.Empty, record.Id.Value);
        Assert.Equal("billing.currency", record.Key.Value);
        Assert.Equal(ConfigurationScopeKind.Module, record.Scope.Kind);
        Assert.Equal("billing", record.Scope.Identifier);
        Assert.Equal(3, record.Version.Value);
        Assert.Equal("USD", record.Value.Value);
        Assert.Equal("tester", record.ChangedBy);
    }
}
