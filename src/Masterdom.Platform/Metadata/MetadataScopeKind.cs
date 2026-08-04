namespace Masterdom.Platform.Metadata;

/// <summary>
/// Defines scope kinds used to resolve metadata definitions.
/// </summary>
public enum MetadataScopeKind
{
    Global = 0,
    Module = 1,
    Aggregate = 2,
    Entity = 3,
    Property = 4,
    Field = 5,
    Enumeration = 6
}
