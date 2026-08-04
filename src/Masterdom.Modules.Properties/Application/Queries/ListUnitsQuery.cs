using Masterdom.Modules.Properties.Domain.Entities.Property;

namespace Masterdom.Modules.Properties.Application.Queries;

/// <summary>
/// Query entry point for listing units in a property aggregate.
/// </summary>
public sealed record ListUnitsQuery(PropertyId PropertyId);
