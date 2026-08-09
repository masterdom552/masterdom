using Masterdom.Modules.UtilityRating.Application.Commands;
using Masterdom.Modules.UtilityRating.Application.Queries;
using Masterdom.Modules.UtilityRating.Application.Support;
using Masterdom.Modules.UtilityRating.Contracts.Metering;
using Masterdom.Modules.UtilityRating.Domain.Entities.UtilityRating;
using UtilityRatingAggregate = Masterdom.Modules.UtilityRating.Domain.Entities.UtilityRating.UtilityRating;

namespace Masterdom.Host.Api;

internal static class UtilityRatingEndpoints
{
    public static IEndpointRouteBuilder MapUtilityRatingEndpoints(this IEndpointRouteBuilder app)
    {
        ArgumentNullException.ThrowIfNull(app);

        var group = app.MapGroup("/api/utility-ratings").WithTags("UtilityRating").RequireAuthorization();

        group.MapPost("/", RateConsumption);
        group.MapGet("/{utilityRatingId:guid}", GetRatingById);

        return app;
    }

    internal static IResult RateConsumption(
        RateConsumptionRequest request,
        ICommandHandler<RateConsumptionCommand, ExecutionResult<UtilityRatingAggregate>> handler)
    {
        var command = new RateConsumptionCommand(
            new MeteringConsumptionOutputContract(
                request.MeterId,
                request.ReadingId,
                request.ConsumptionValue,
                request.PeriodStart,
                request.PeriodEnd,
                request.CapturedAtUtc),
            request.TariffCode);

        var result = handler.Handle(command);
        if (!result.IsSuccess || result.Value is null)
        {
            return ApiExecutionResults.ToErrorResult(result.ErrorCode, result.ErrorMessage);
        }

        var response = UtilityRatingResponse.From(result.Value);
        return TypedResults.Created($"/api/utility-ratings/{response.Id}", response);
    }

    internal static IResult GetRatingById(
        Guid utilityRatingId,
        IQueryHandler<GetRatingByIdQuery, ExecutionResult<UtilityRatingAggregate>> handler)
    {
        var result = handler.Handle(new GetRatingByIdQuery(UtilityRatingId.From(utilityRatingId)));

        return result.IsSuccess && result.Value is not null
            ? TypedResults.Ok(UtilityRatingResponse.From(result.Value))
            : ApiExecutionResults.ToErrorResult(result.ErrorCode, result.ErrorMessage);
    }

    internal sealed record RateConsumptionRequest(
        Guid MeterId,
        Guid ReadingId,
        decimal ConsumptionValue,
        DateOnly PeriodStart,
        DateOnly PeriodEnd,
        DateTime CapturedAtUtc,
        string TariffCode);

    internal sealed record UtilityRatingResponse(
        Guid Id,
        Guid MeterId,
        Guid ReadingId,
        decimal ConsumptionValue,
        DateOnly RatingPeriodStartDate,
        DateOnly RatingPeriodEndDate,
        string TariffCode,
        int TariffVersion,
        decimal RatedUnits,
        decimal RatedAmount,
        string RatingStatus,
        int RatingVersion,
        DateTime RatedAtUtc)
    {
        public static UtilityRatingResponse From(UtilityRatingAggregate rating)
        {
            return new UtilityRatingResponse(
                rating.Id.Value,
                rating.MeterReference.MeterId,
                rating.ConsumptionReference.ReadingId,
                rating.ConsumptionReference.ConsumptionValue,
                rating.RatingPeriod.StartDate,
                rating.RatingPeriod.EndDate,
                rating.TariffReference.TariffCode,
                rating.TariffReference.TariffVersion,
                rating.RatedUnits.Value,
                rating.RatedAmount.Value,
                rating.RatingStatus.Value,
                rating.RatingVersion.Value,
                rating.RatedAtUtc);
        }
    }
}
