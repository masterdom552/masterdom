using System.Collections.Generic;

namespace Masterdom.Platform.Rules;

/// <summary>
/// Represents an immutable runtime view of registered rules.
/// </summary>
public interface IRuleCatalog
{
    IReadOnlyList<RuleSetDefinition> RuleSets { get; }

    IReadOnlyList<RuleDefinition> Rules { get; }
}
