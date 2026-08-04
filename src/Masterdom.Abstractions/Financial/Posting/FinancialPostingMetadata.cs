using System.Collections.Immutable;

namespace Masterdom.Abstractions.Financial.Posting;

public sealed record FinancialPostingMetadata
{
    public ImmutableDictionary<string, string> Extensions { get; init; } = ImmutableDictionary<string, string>.Empty;
}
