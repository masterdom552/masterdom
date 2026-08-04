using System.Collections.Generic;

namespace Masterdom.Platform.Rules;

/// <summary>
/// Represents seeded rules data produced from module catalog entries.
/// </summary>
public sealed class RuleCatalogSeed
{
    public required IReadOnlyList<RuleSetDefinition> RuleSets { get; init; }

    public required IReadOnlyList<RuleDefinition> Rules { get; init; }
}
