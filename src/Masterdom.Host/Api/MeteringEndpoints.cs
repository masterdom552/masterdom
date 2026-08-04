using Masterdom.Modules.Metering.Application.Commands;
using Masterdom.Modules.Metering.Application.Queries;
using Masterdom.Modules.Metering.Application.Support;
using Masterdom.Modules.Metering.Domain.Entities.Metering;
using MeterAggregate = Masterdom.Modules.Metering.Domain.Entities.Metering.Meter;

namespace Masterdom.Host.Api;

internal static class MeteringEndpoints
{
    public static IEndpointRouteBuilder MapMeteringEndpoints(this IEndpointRouteBuilder app)
    {
        ArgumentNullException.ThrowIfNull(app);

        var group = app.MapGroup("/api/meters").WithTags("Metering").RequireAuthorization();

        group.MapPost("/", InstallMeter);
        group.MapPut("/{meterId:guid}/readings", SubmitReading);
        group.MapPut("/{meterId:guid}/readings/{readingId:guid}/approve", ApproveReading);
        group.MapPut("/{meterId:guid}/readings/{readingId:guid}/correct", CorrectReading);
        group.MapPut("/{meterId:guid}/retire", RetireMeter);
        group.MapGet("/{meterId:guid}", GetMeterById);
        group.MapGet("/by-number/{meterNumber}", GetMeterByNumber);

        return app;
    }

    internal static IResult InstallMeter(
        InstallMeterRequest request,
        ICommandHandler<InstallMeterCommand, ExecutionResult<MeterAggregate>> handler)
    {
        var command = new InstallMeterCommand(
            MeterNumber.Create(request.MeterNumber),
            MeterCategory.Create(request.MeterCategory),
            MeterType.Create(request.MeterType),
            MeterLocationReference.Create(request.PropertyId, request.UnitId),
            InstallationDate.Create(request.InstallationDate));

        var result = handler.Handle(command);
        if (!result.IsSuccess || result.Value is null)
        {
            return ApiExecutionResults.ToErrorResult(result.ErrorCode, result.ErrorMessage);
        }

        var response = MeterResponse.From(result.Value);
        return TypedResults.Created($"/api/meters/{response.Id}", response);
    }

    internal static IResult SubmitReading(
        Guid meterId,
        SubmitReadingRequest request,
        ICommandHandler<SubmitReadingCommand, ExecutionResult<MeterAggregate>> handler)
    {
        var command = new SubmitReadingCommand(
            MeterId.From(meterId),
            ReadingDate.Create(request.ReadingDate),
            ReadingValue.Create(request.ReadingValue),
            ReadingSource.Create(request.ReadingSource),
            SubmittedBy.Create(request.SubmittedBy),
            request.SubmittedAtUtc,
            request.AllowFutureReadings,
            request.IsRollover,
            request.ReadingNotes is null ? null : ReadingNotes.Create(request.ReadingNotes));

        var result = handler.Handle(command);
        return result.IsSuccess && result.Value is not null
            ? TypedResults.Ok(MeterResponse.From(result.Value))
            : ApiExecutionResults.ToErrorResult(result.ErrorCode, result.ErrorMessage);
    }

    internal static IResult ApproveReading(
        Guid meterId,
        Guid readingId,
        ApproveReadingRequest request,
        ICommandHandler<ApproveReadingCommand, ExecutionResult<MeterAggregate>> handler)
    {
        var command = new ApproveReadingCommand(
            MeterId.From(meterId),
            readingId,
            ReviewedBy.Create(request.ReviewedBy),
            ReviewDate.Create(request.ReviewDate.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc)));

        var result = handler.Handle(command);
        return result.IsSuccess && result.Value is not null
            ? TypedResults.Ok(MeterResponse.From(result.Value))
            : ApiExecutionResults.ToErrorResult(result.ErrorCode, result.ErrorMessage);
    }

    internal static IResult CorrectReading(
        Guid meterId,
        Guid readingId,
        CorrectReadingRequest request,
        ICommandHandler<CorrectReadingCommand, ExecutionResult<MeterAggregate>> handler)
    {
        var command = new CorrectReadingCommand(
            MeterId.From(meterId),
            readingId,
            ReadingValue.Create(request.CorrectedValue),
            request.Reason,
            SubmittedBy.Create(request.CorrectedBy),
            request.CorrectedAtUtc);

        var result = handler.Handle(command);
        return result.IsSuccess && result.Value is not null
            ? TypedResults.Ok(MeterResponse.From(result.Value))
            : ApiExecutionResults.ToErrorResult(result.ErrorCode, result.ErrorMessage);
    }

    internal static IResult RetireMeter(
        Guid meterId,
        RetireMeterRequest request,
        ICommandHandler<RetireMeterCommand, ExecutionResult<MeterAggregate>> handler)
    {
        var command = new RetireMeterCommand(
            MeterId.From(meterId),
            RemovalDate.Create(request.RemovalDate));

        var result = handler.Handle(command);
        return result.IsSuccess && result.Value is not null
            ? TypedResults.Ok(MeterResponse.From(result.Value))
            : ApiExecutionResults.ToErrorResult(result.ErrorCode, result.ErrorMessage);
    }

    internal static IResult GetMeterById(
        Guid meterId,
        IQueryHandler<GetMeterByIdQuery, ExecutionResult<MeterAggregate>> handler)
    {
        var result = handler.Handle(new GetMeterByIdQuery(MeterId.From(meterId)));
        return result.IsSuccess && result.Value is not null
            ? TypedResults.Ok(MeterResponse.From(result.Value))
            : ApiExecutionResults.ToErrorResult(result.ErrorCode, result.ErrorMessage);
    }

    internal static IResult GetMeterByNumber(
        string meterNumber,
        IQueryHandler<GetMeterByNumberQuery, ExecutionResult<MeterAggregate>> handler)
    {
        var result = handler.Handle(new GetMeterByNumberQuery(MeterNumber.Create(meterNumber)));
        return result.IsSuccess && result.Value is not null
            ? TypedResults.Ok(MeterResponse.From(result.Value))
            : ApiExecutionResults.ToErrorResult(result.ErrorCode, result.ErrorMessage);
    }

    internal sealed record InstallMeterRequest(
        string MeterNumber,
        string MeterCategory,
        string MeterType,
        Guid PropertyId,
        Guid UnitId,
        DateOnly InstallationDate);

    internal sealed record SubmitReadingRequest(
        DateOnly ReadingDate,
        decimal ReadingValue,
        string ReadingSource,
        string SubmittedBy,
        DateTime SubmittedAtUtc,
        bool AllowFutureReadings,
        bool IsRollover,
        string? ReadingNotes);

    internal sealed record ApproveReadingRequest(string ReviewedBy, DateOnly ReviewDate);

    internal sealed record CorrectReadingRequest(decimal CorrectedValue, string Reason, string CorrectedBy, DateTime CorrectedAtUtc);

    internal sealed record RetireMeterRequest(DateOnly RemovalDate);

    internal sealed record MeterResponse(
        Guid Id,
        string MeterNumber,
        string MeterCategory,
        string MeterType,
        string MeterStatus,
        Guid PropertyId,
        Guid UnitId,
        DateOnly InstallationDate,
        DateOnly? RemovalDate,
        Guid? CurrentReadingId,
        DateOnly? CurrentReadingDate,
        decimal? CurrentReadingValue,
        decimal? CurrentConsumption,
        int ReadingCount)
    {
        public static MeterResponse From(MeterAggregate meter)
        {
            return new MeterResponse(
                meter.Id.Value,
                meter.MeterNumber.Value,
                meter.MeterCategory.Value,
                meter.MeterType.Value,
                meter.MeterStatus.Value,
                meter.MeterLocationReference.PropertyId,
                meter.MeterLocationReference.UnitId,
                meter.InstallationDate.Value,
                meter.RemovalDate?.Value,
                meter.CurrentReading?.ReadingId,
                meter.CurrentReading?.ReadingDate.Value,
                meter.CurrentReading?.ReadingValue.Value,
                meter.CurrentReading?.Consumption?.Value,
                meter.HistoricalReadings.Count);
        }
    }
}
