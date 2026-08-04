using System;
using Masterdom.Platform.Metadata;

namespace Masterdom.Platform.Tests.Metadata;

public sealed class MetadataRegistryTests
{
    [Fact]
    public void RegisterRange_ShouldExposeMetadataInCatalog()
    {
        var registry = new MetadataRegistry();

        var definition = new MetadataDefinition(
            new MetadataId(Guid.NewGuid()),
            new MetadataKey("module.people"),
            MetadataCategory.Module,
            MetadataScope.Module("people"),
            new MetadataVersion(1),
            new MetadataEffectivePeriod(
                DateTime.SpecifyKind(new DateTime(2026, 1, 1), DateTimeKind.Utc),
                null),
            "People",
            "People module",
            null,
            false,
            null,
            null,
            "tester",
            DateTime.SpecifyKind(new DateTime(2026, 1, 1), DateTimeKind.Utc));

        registry.RegisterRange(new[] { definition });

        var catalog = registry.GetCatalog();

        var item = Assert.Single(catalog.Definitions);

        Assert.Equal("module.people", item.Key.Value);
        Assert.Equal(MetadataCategory.Module, item.Category);
    }
}
