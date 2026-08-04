using Masterdom.Core.Identifiers;
using Masterdom.Modules.People.Domain.Entities.Person;
using Masterdom.Modules.People.Domain.Entities.Person.Events;
using PersonAggregate = Masterdom.Modules.People.Domain.Entities.Person.Person;

namespace Masterdom.Core.Tests.Person;

public sealed class PersonDomainTests
{
    [Fact]
    public void Create_ShouldInitializePersonAndRaiseCreatedEvent()
    {
        var person = PersonAggregate.Create(
            PersonNumber.Create("P-0001"),
            PersonName.Create("Ava", "Stone"),
            Gender.Female);

        Assert.NotNull(person);
        Assert.Equal("P-0001", person.Number.Value);
        Assert.Equal("Ava", person.Name.FirstName);
        Assert.Equal(PersonStatus.Active, person.Status);
        Assert.Contains(person.DomainEvents, x => x is PersonCreatedDomainEvent);
    }

    [Fact]
    public void AddContact_ShouldRaiseContactAddedEvent()
    {
        var person = PersonAggregate.Create(
            PersonNumber.Create("P-0002"),
            PersonName.Create("Noah", "Reed"),
            Gender.Male);

        var contact = Contact.Create("Email", "noah@example.com", isPrimary: true);
        person.AddContact(contact);

        Assert.Single(person.Contacts);
        Assert.Contains(person.DomainEvents, x => x is ContactAddedDomainEvent);
    }

    [Fact]
    public void AddGovernmentDocument_ShouldRaiseIdentityDocumentAddedEvent()
    {
        var person = PersonAggregate.Create(
            PersonNumber.Create("P-0003"),
            PersonName.Create("Mia", "Cole"),
            Gender.Female);

        var document = GovernmentDocument.Create("Passport", "A1234567");
        person.AddGovernmentDocument(document);

        Assert.Single(person.GovernmentDocuments);
        Assert.Contains(person.DomainEvents, x => x is IdentityDocumentAddedDomainEvent);
    }

    [Fact]
    public void AddRelationship_ShouldRaiseRelationshipAddedEvent()
    {
        var person = PersonAggregate.Create(
            PersonNumber.Create("P-0004"),
            PersonName.Create("Leo", "King"),
            Gender.Male);

        var other = PersonId.New();
        person.AddRelationship(PersonRelationship.Create(other, "Business Partner"));

        Assert.Single(person.Relationships);
        Assert.Contains(person.DomainEvents, x => x is RelationshipAddedDomainEvent);
    }
}
