using Masterdom.Modules.Billing.Application.Capabilities.ChargeComposition.ReadModels;
using Microsoft.EntityFrameworkCore;

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
                x.Id.Value == leaseId &&
                x.Tenancy.TenancyId == tenancyId &&
                x.Property.PropertyId == propertyId &&
                x.Unit.UnitId == unitId);

        if (lease is null)
        {
            return null;
        }

        var tenancy = _dbContext.Tenancies
            .AsNoTracking()
            .FirstOrDefault(x => x.Id.Value == tenancyId);

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
