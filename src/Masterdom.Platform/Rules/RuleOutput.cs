using System.Collections.Generic;
using System.Linq;

namespace Masterdom.Platform.Rules;

/// <summary>
/// Represents aggregated output of rule evaluation.
/// </summary>
public sealed class RuleOutput
{
    public required RuleSetId RuleSetId { get; init; }

    public required RuleSetKey RuleSetKey { get; init; }

    public required IReadOnlyList<RuleResult> Results { get; init; }

    public bool Passed => Results.All(result => result.Status == RuleResultStatus.Passed);
}
