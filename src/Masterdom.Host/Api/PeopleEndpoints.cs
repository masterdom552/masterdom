using Masterdom.Core.Identifiers;
using Masterdom.Modules.People.Application.Commands;
using Masterdom.Modules.People.Application.Queries;
using Masterdom.Modules.People.Domain.Entities.Person;
using PeopleSupport = Masterdom.Modules.People.Application.Support;

namespace Masterdom.Host.Api;

internal static class PeopleEndpoints
{
    public static IEndpointRouteBuilder MapPeopleEndpoints(this IEndpointRouteBuilder app)
    {
        ArgumentNullException.ThrowIfNull(app);

        var group = app.MapGroup("/api/people").WithTags("People").RequireAuthorization();

        group.MapPost("/", CreatePerson);
        group.MapPut("/{personId:guid}/name", RenamePerson);
        group.MapPut("/{personId:guid}/status", ChangePersonStatus);
        group.MapPost("/{personId:guid}/contacts", AddContact);
        group.MapPost("/{personId:guid}/contacts/remove", RemoveContact);
        group.MapPost("/{personId:guid}/documents", AddIdentityDocument);
        group.MapPost("/{personId:guid}/relationships", AddRelationship);
        group.MapGet("/{personId:guid}", GetPersonById);
        group.MapGet("/by-number/{number}", GetPersonByNumber);
        group.MapGet("/search", SearchPeople);

        return app;
    }

    internal static IResult CreatePerson(
        CreatePersonRequest request,
        PeopleSupport.ICommandHandler<CreatePersonCommand, PeopleSupport.ExecutionResult<Person>> handler)
    {
        var command = new CreatePersonCommand(
            PersonNumber.Create(request.Number),
            PersonName.Create(request.FirstName, request.LastName, request.MiddleName, request.Title, request.Suffix),
            Gender.Create(request.Gender));

        var result = handler.Handle(command);
        if (!result.IsSuccess || result.Value is null)
        {
            return ApiExecutionResults.ToErrorResult(result.ErrorCode, result.ErrorMessage);
        }

        var response = PersonResponse.From(result.Value);
        return TypedResults.Created($"/api/people/{response.Id}", response);
    }

    internal static IResult RenamePerson(
        Guid personId,
        RenamePersonRequest request,
        PeopleSupport.ICommandHandler<RenamePersonCommand, PeopleSupport.ExecutionResult<Person>> handler)
    {
        var command = new RenamePersonCommand(
            PersonId.From(personId),
            PersonName.Create(request.FirstName, request.LastName, request.MiddleName, request.Title, request.Suffix));

        var result = handler.Handle(command);
        return result.IsSuccess && result.Value is not null
            ? TypedResults.Ok(PersonResponse.From(result.Value))
            : ApiExecutionResults.ToErrorResult(result.ErrorCode, result.ErrorMessage);
    }

    internal static IResult ChangePersonStatus(
        Guid personId,
        ChangePersonStatusRequest request,
        PeopleSupport.ICommandHandler<ChangePersonStatusCommand, PeopleSupport.ExecutionResult<Person>> handler)
    {
        var command = new ChangePersonStatusCommand(
            PersonId.From(personId),
            PersonStatus.Create(request.Status));

        var result = handler.Handle(command);
        return result.IsSuccess && result.Value is not null
            ? TypedResults.Ok(PersonResponse.From(result.Value))
            : ApiExecutionResults.ToErrorResult(result.ErrorCode, result.ErrorMessage);
    }

    internal static IResult AddContact(
        Guid personId,
        AddContactRequest request,
        PeopleSupport.ICommandHandler<AddContactCommand, PeopleSupport.ExecutionResult<Person>> handler)
    {
        var command = new AddContactCommand(
            PersonId.From(personId),
            Contact.Create(
                request.Type,
                request.Value,
                request.IsPrimary,
                request.IsVerified,
                request.Remarks,
                request.Other));

        var result = handler.Handle(command);
        return result.IsSuccess && result.Value is not null
            ? TypedResults.Ok(PersonResponse.From(result.Value))
            : ApiExecutionResults.ToErrorResult(result.ErrorCode, result.ErrorMessage);
    }

    internal static IResult RemoveContact(
        Guid personId,
        RemoveContactRequest request,
        PeopleSupport.ICommandHandler<RemoveContactCommand, PeopleSupport.ExecutionResult<bool>> handler)
    {
        var command = new RemoveContactCommand(
            PersonId.From(personId),
            Contact.Create(
                request.Type,
                request.Value,
                request.IsPrimary,
                request.IsVerified,
                request.Remarks,
                request.Other));

        var result = handler.Handle(command);
        return result.IsSuccess
            ? TypedResults.NoContent()
            : ApiExecutionResults.ToErrorResult(result.ErrorCode, result.ErrorMessage);
    }

    internal static IResult AddIdentityDocument(
        Guid personId,
        AddIdentityDocumentRequest request,
        PeopleSupport.ICommandHandler<AddIdentityDocumentCommand, PeopleSupport.ExecutionResult<Person>> handler)
    {
        var command = new AddIdentityDocumentCommand(
            PersonId.From(personId),
            GovernmentDocument.Create(
                request.Type,
                request.DocumentNumber,
                request.IssuingAuthority,
                request.IssueDate,
                request.ExpiryDate,
                request.IsPrimary,
                request.IsVerified,
                request.Remarks,
                request.Other));

        var result = handler.Handle(command);
        return result.IsSuccess && result.Value is not null
            ? TypedResults.Ok(PersonResponse.From(result.Value))
            : ApiExecutionResults.ToErrorResult(result.ErrorCode, result.ErrorMessage);
    }

    internal static IResult AddRelationship(
        Guid personId,
        AddRelationshipRequest request,
        PeopleSupport.ICommandHandler<AddRelationshipCommand, PeopleSupport.ExecutionResult<Person>> handler)
    {
        var command = new AddRelationshipCommand(
            PersonId.From(personId),
            PersonRelationship.Create(PersonId.From(request.RelatedPersonId), request.Type, request.Remarks));

        var result = handler.Handle(command);
        return result.IsSuccess && result.Value is not null
            ? TypedResults.Ok(PersonResponse.From(result.Value))
            : ApiExecutionResults.ToErrorResult(result.ErrorCode, result.ErrorMessage);
    }

    internal static IResult GetPersonById(
        Guid personId,
        PeopleSupport.IQueryHandler<GetPersonByIdQuery, PeopleSupport.ExecutionResult<Person>> handler)
    {
        var result = handler.Handle(new GetPersonByIdQuery(PersonId.From(personId)));
        return result.IsSuccess && result.Value is not null
            ? TypedResults.Ok(PersonResponse.From(result.Value))
            : ApiExecutionResults.ToErrorResult(result.ErrorCode, result.ErrorMessage);
    }

    internal static IResult GetPersonByNumber(
        string number,
        PeopleSupport.IQueryHandler<GetPersonByNumberQuery, PeopleSupport.ExecutionResult<Person>> handler)
    {
        var result = handler.Handle(new GetPersonByNumberQuery(PersonNumber.Create(number)));
        return result.IsSuccess && result.Value is not null
            ? TypedResults.Ok(PersonResponse.From(result.Value))
            : ApiExecutionResults.ToErrorResult(result.ErrorCode, result.ErrorMessage);
    }

    internal static IResult SearchPeople(
        string? numberContains,
        int? take,
        PeopleSupport.IQueryHandler<SearchPeopleQuery, PeopleSupport.ExecutionResult<IReadOnlyCollection<Person>>> handler)
    {
        var result = handler.Handle(new SearchPeopleQuery(numberContains, take ?? 50));
        return result.IsSuccess && result.Value is not null
            ? TypedResults.Ok(result.Value.Select(PersonResponse.From).ToList())
            : ApiExecutionResults.ToErrorResult(result.ErrorCode, result.ErrorMessage);
    }

    internal sealed record CreatePersonRequest(
        string Number,
        string FirstName,
        string LastName,
        string? MiddleName,
        string? Title,
        string? Suffix,
        string Gender);

    internal sealed record RenamePersonRequest(
        string FirstName,
        string LastName,
        string? MiddleName,
        string? Title,
        string? Suffix);

    internal sealed record ChangePersonStatusRequest(string Status);

    internal sealed record AddContactRequest(
        string Type,
        string Value,
        bool IsPrimary,
        bool IsVerified,
        string? Remarks,
        string? Other);

    internal sealed record RemoveContactRequest(
        string Type,
        string Value,
        bool IsPrimary,
        bool IsVerified,
        string? Remarks,
        string? Other);

    internal sealed record AddIdentityDocumentRequest(
        string Type,
        string DocumentNumber,
        string? IssuingAuthority,
        DateOnly? IssueDate,
        DateOnly? ExpiryDate,
        bool IsPrimary,
        bool IsVerified,
        string? Remarks,
        string? Other);

    internal sealed record AddRelationshipRequest(
        Guid RelatedPersonId,
        string Type,
        string? Remarks);

    internal sealed record PersonResponse(
        Guid Id,
        string Number,
        string DisplayName,
        string Status,
        int ContactCount)
    {
        public static PersonResponse From(Person person)
        {
            return new PersonResponse(
                person.Id.Value,
                person.Number.Value,
                person.Name.DisplayName,
                person.Status.Value,
                person.Contacts.Count);
        }
    }
}
