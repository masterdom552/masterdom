namespace Masterdom.Abstractions.Policies;

/// <summary>
/// Represents the outcome of an applicable-policy request.
/// </summary>
public sealed record ApplicablePolicyResolution
{
    private ApplicablePolicyResolution(bool isApplicable, ApplicablePolicy? policy)
    {
        IsApplicable = isApplicable;
        Policy = policy;
    }

    /// <summary>
    /// Gets a value indicating whether an applicable policy was found.
    /// </summary>
    public bool IsApplicable { get; }

    /// <summary>
    /// Gets the applicable policy, or <see langword="null"/> when none was found.
    /// </summary>
    public ApplicablePolicy? Policy { get; }

    /// <summary>
    /// Creates a successful applicable-policy resolution.
    /// </summary>
    /// <param name="policy">The applicable policy.</param>
    /// <returns>A resolution containing the applicable policy.</returns>
    public static ApplicablePolicyResolution Applicable(ApplicablePolicy policy)
    {
        ArgumentNullException.ThrowIfNull(policy);
        return new ApplicablePolicyResolution(true, policy);
    }

    /// <summary>
    /// Creates a resolution indicating that no policy is applicable.
    /// </summary>
    /// <returns>A resolution with no applicable policy.</returns>
    public static ApplicablePolicyResolution NotApplicable() => new(false, null);
}
