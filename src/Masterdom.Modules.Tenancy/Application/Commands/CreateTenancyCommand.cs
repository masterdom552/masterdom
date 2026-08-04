using Masterdom.Modules.Tenancy.Domain.Entities.Tenancy;
using Masterdom.Core.Identifiers;

namespace Masterdom.Modules.Tenancy.Application.Commands;

/// <summary>
/// Command entry point for tenancy creation.
/// </summary>
public sealed record CreateTenancyCommand(
    TenancyNumber Number,
    PropertyReference Property,
    UnitReference Unit,
    MoveInDate MoveInDate,
    PersonId PrimaryOccupantPersonId,
    Notes? Notes);
