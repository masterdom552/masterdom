using Masterdom.Modules.Billing.Application.Capabilities.ChargeComposition.ReadModels;
using Masterdom.Modules.Lease.Domain.Entities.Lease;
using Masterdom.Modules.Tenancy.Domain.Entities.Tenancy;
using Microsoft.EntityFrameworkCore;
using LeasePropertyReference = Masterdom.Modules.Lease.Domain.Entities.Lease.PropertyReference;
using LeaseTenancyReference = Masterdom.Modules.Lease.Domain.Entities.Lease.TenancyReference;
using LeaseUnitReference = Masterdom.Modules.Lease.Domain.Entities.Lease.UnitReference;

namespace Masterdom.Infrastructure.Persistence.Billing;

public sealed class BillingChargeCompositionReadService : IChargeCompositionReadService
{
    private readonly MasterdomDbContext _dbContext;

    public BillingChargeCompositionReadService(MasterdomDbContext dbContext)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
    }

    public RentChargeReadModel? GetRentChargeReadModel(
        Guid tenancyId,
        Guid leaseId,
        Guid propertyId,
        Guid unitId)
    {
        var lease = _dbContext.Leases
            .AsNoTracking()
            .Include(x => x.Versions)
            .FirstOrDefault(x =>
                x.Id == LeaseId.From(leaseId) &&
                x.Tenancy == LeaseTenancyReference.Create(tenancyId) &&
                x.Property == LeasePropertyReference.Create(propertyId) &&
                x.Unit == LeaseUnitReference.Create(unitId));

        if (lease is null)
        {
            return null;
        }

        var tenancy = _dbContext.Tenancies
            .AsNoTracking()
            .FirstOrDefault(x => x.Id == TenancyId.From(tenancyId));

        var currentVersion = lease.Versions
            .OrderByDescending(x => x.VersionNumber)
            .FirstOrDefault();

        if (currentVersion is null)
        {
            return null;
        }

        var rentTerms = currentVersion.CommercialTerms.RentTerms;

        return new RentChargeReadModel(
            tenancyId,
            leaseId,
            propertyId,
            unitId,
            IsTenancyActive: tenancy?.Status.Value.Equals("Active", StringComparison.OrdinalIgnoreCase) == true,
            IsLeaseActive: lease.Status.Value.Equals("Active", StringComparison.OrdinalIgnoreCase),
            RentAmount: rentTerms.MonthlyRent,
            Currency: "USD",
            BillingFrequency: rentTerms.BillingFrequency.Value,
            LeaseNumber: lease.Number.Value);
    }
}
