namespace Masterdom.Platform.Rules;

/// <summary>
/// Defines scope kinds used by rules and rule sets.
/// </summary>
public enum RuleScopeKind
{
    Global = 0,
    Module = 1,
    Tenant = 2,
    Aggregate = 3,
    Entity = 4,
    Property = 5,
    Field = 6
}
