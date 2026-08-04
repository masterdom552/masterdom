namespace Masterdom.Platform.CalculationEngine.Metadata;

/// <summary>
/// Identifies how a calculation operation descriptor was produced.
/// </summary>
public enum CalculationOperationDescriptorSourceType
{
    /// <summary>
    /// Descriptor was discovered through reflection.
    /// </summary>
    Reflection = 0,

    /// <summary>
    /// Descriptor was generated ahead of time.
    /// </summary>
    Generated = 1,

    /// <summary>
    /// Descriptor was supplied by a plugin.
    /// </summary>
    Plugin = 2,

    /// <summary>
    /// Descriptor is used for tests.
    /// </summary>
    Test = 3,

    /// <summary>
    /// Descriptor was authored manually.
    /// </summary>
    Manual = 4
}
