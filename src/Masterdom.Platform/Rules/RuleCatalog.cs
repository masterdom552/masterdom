using System;
using System.Collections.Generic;
using System.Linq;

namespace Masterdom.Platform.Rules;

/// <summary>
/// Immutable runtime rules catalog.
/// </summary>
public sealed class RuleCatalog : IRuleCatalog
{
    public RuleCatalog(
        IEnumerable<RuleSetDefinition> ruleSets,
        IEnumerable<RuleDefinition> rules)
    {
        ArgumentNullException.ThrowIfNull(ruleSets);
        ArgumentNullException.ThrowIfNull(rules);

        RuleSets = ruleSets.ToList();
        Rules = rules.ToList();
    }

    public IReadOnlyList<RuleSetDefinition> RuleSets { get; }

    public IReadOnlyList<RuleDefinition> Rules { get; }
}
