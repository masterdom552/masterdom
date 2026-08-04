using Masterdom.Modules.Properties.Domain.Entities.Property;

namespace Masterdom.Modules.Properties.Application.Queries;

/// <summary>
/// Query entry point for retrieving a property aggregate.
/// </summary>
public sealed record GetPropertyByIdQuery(PropertyId PropertyId);
