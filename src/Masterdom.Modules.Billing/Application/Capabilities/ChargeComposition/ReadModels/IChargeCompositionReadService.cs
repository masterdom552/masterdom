namespace Masterdom.Modules.Billing.Application.Capabilities.ChargeComposition.ReadModels;

public interface IChargeCompositionReadService
{
    RentChargeReadModel? GetRentChargeReadModel(
        Guid tenancyId,
        Guid leaseId,
        Guid propertyId,
        Guid unitId);
}
