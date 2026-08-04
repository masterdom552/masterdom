namespace Masterdom.Platform.Configuration;

/// <summary>
/// Defines configuration scopes in precedence order.
/// </summary>
public enum ConfigurationScopeKind
{
    Global = 0,
    Module = 1,
    Tenant = 2,
    Property = 3
}
