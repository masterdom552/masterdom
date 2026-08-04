using System;
using Masterdom.Infrastructure.Persistence;
using Masterdom.Infrastructure.Persistence.Metadata;
using Masterdom.Platform.Metadata;
using Microsoft.EntityFrameworkCore;

namespace Masterdom.Platform.Tests.Metadata;

public sealed class PlatformMetadataRepositoryTests
{
    [Fact]
    public void GetAll_ShouldMapPersistedEntitiesToMetadataDefinitions()
    {
        var options = new DbContextOptionsBuilder<MasterdomDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        using var dbContext = new MasterdomDbContext(options);

        var changedAt = DateTime.SpecifyKind(new DateTime(2026, 2, 1), DateTimeKind.Utc);
        var id = Guid.NewGuid();

        dbContext.PlatformMetadataDefinitions.Add(new PlatformMetadataDefinitionEntity
        {
            Id = id,
            Key = "entity.person",
            Category = (int)MetadataCategory.Entity,
            ScopeKind = (int)MetadataScopeKind.Entity,
            ScopeIdentifier = "people.person",
            Version = 3,
            EffectiveFromUtc = changedAt,
            EffectiveToUtc = null,
            Name = "Person",
            Description = "Person entity metadata",
            ParentId = null,
            IsDeprecated = false,
            ReplacedByKey = null,
            Compatibility = "Compatible",
            ChangedBy = "tester",
            ChangedAtUtc = changedAt
        });

        dbContext.SaveChanges();

        var repository = new PlatformMetadataRepository(dbContext);

        var definitions = repository.GetAll();

        var definition = Assert.Single(definitions);

        Assert.Equal(id, definition.Id.Value);
        Assert.Equal("entity.person", definition.Key.Value);
        Assert.Equal(MetadataCategory.Entity, definition.Category);
        Assert.Equal(MetadataScopeKind.Entity, definition.Scope.Kind);
        Assert.Equal("people.person", definition.Scope.Identifier);
        Assert.Equal(3, definition.Version.Value);
        Assert.Equal("Person", definition.Name);
        Assert.Equal("tester", definition.ChangedBy);
    }
}
