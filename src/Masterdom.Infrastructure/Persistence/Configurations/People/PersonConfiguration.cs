using Masterdom.Core.Identifiers;
using Masterdom.Modules.People.Domain.Entities.Person;
using Masterdom.Infrastructure.Persistence.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.Text.Json;

namespace Masterdom.Infrastructure.Persistence.Configurations.People;

/// <summary>
/// EF Core configuration for <see cref="Person"/>.
/// </summary>
public sealed class PersonConfiguration : IEntityTypeConfiguration<Person>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<Person> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.ToTable("Persons");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasEntityIdConversion(PersonId.From)
            .ValueGeneratedNever();

        builder.Property(x => x.Number)
            .HasValueObjectConversion(PersonNumber.Create)
            .HasMaxLength(50)
            .IsRequired();

        builder.HasIndex(x => x.Number)
            .IsUnique();

        builder.Property(x => x.Name)
            .HasConversion(
                personName => JsonSerializer.Serialize(
                    new PersonNamePersistenceModel(
                        personName.FirstName,
                        personName.MiddleName,
                        personName.LastName,
                        personName.Title,
                        personName.Suffix),
                    JsonSerializerOptions.Web),
                json => DeserializePersonName(json))
            .HasColumnType("jsonb")
            .IsRequired();

        builder.OwnsMany(x => x.Addresses, addressBuilder =>
        {
            addressBuilder.ToTable("person_addresses");

            addressBuilder.WithOwner()
                .HasForeignKey("PersonId");

            addressBuilder.Property<int>("Id");
            addressBuilder.HasKey("Id");

            addressBuilder.Property(x => x.Type)
                .IsRequired();

            addressBuilder.Property(x => x.Line1)
                .IsRequired();

            addressBuilder.Property(x => x.Line2);

            addressBuilder.Property(x => x.Landmark);

            addressBuilder.Property(x => x.City)
                .IsRequired();

            addressBuilder.Property(x => x.District)
                .IsRequired();

            addressBuilder.Property(x => x.State)
                .IsRequired();

            addressBuilder.Property(x => x.Country)
                .IsRequired();

            addressBuilder.Property(x => x.PostalCode)
                .IsRequired();

            addressBuilder.Property(x => x.IsPrimary)
                .IsRequired();

            addressBuilder.Property(x => x.Remarks);

            addressBuilder.Property(x => x.Other);
        });

        builder.Navigation(x => x.Addresses)
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.OwnsMany(x => x.Contacts, contactBuilder =>
        {
            contactBuilder.ToTable("person_contacts");

            contactBuilder.WithOwner()
                .HasForeignKey("PersonId");

            contactBuilder.Property<int>("Id");
            contactBuilder.HasKey("Id");

            contactBuilder.Property(x => x.Type)
                .IsRequired();

            contactBuilder.Property(x => x.Value)
                .IsRequired();

            contactBuilder.Property(x => x.IsPrimary)
                .IsRequired();

            contactBuilder.Property(x => x.IsVerified)
                .IsRequired();

            contactBuilder.Property(x => x.Remarks);

            contactBuilder.Property(x => x.Other);
        });

        builder.Navigation(x => x.Contacts)
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.OwnsMany(x => x.EmergencyContacts, emergencyContactBuilder =>
        {
            emergencyContactBuilder.ToTable("person_emergency_contacts");

            emergencyContactBuilder.WithOwner()
                .HasForeignKey("PersonId");

            emergencyContactBuilder.Property<int>("Id");
            emergencyContactBuilder.HasKey("Id");

            emergencyContactBuilder.Property(x => x.FullName)
                .HasConversion(
                    fullName => JsonSerializer.Serialize(
                        new FullNamePersistenceModel(
                            fullName.Title,
                            fullName.FirstName,
                            fullName.MiddleName,
                            fullName.LastName,
                            fullName.Suffix),
                        JsonSerializerOptions.Web),
                    json => DeserializeFullName(json))
                .HasColumnType("jsonb")
                .IsRequired();

            emergencyContactBuilder.Property(x => x.Relationship)
                .IsRequired();

            emergencyContactBuilder.Property(x => x.MobileNumber)
                .IsRequired();

            emergencyContactBuilder.Property(x => x.AlternateMobileNumber);

            emergencyContactBuilder.Property(x => x.EmailAddress);

            emergencyContactBuilder.Property(x => x.Address)
                .HasConversion(
                    address => SerializeAddress(address),
                    json => DeserializeAddress(json))
                .HasColumnType("jsonb");

            emergencyContactBuilder.Property(x => x.IsPrimary)
                .IsRequired();

            emergencyContactBuilder.Property(x => x.Remarks);

            emergencyContactBuilder.Property(x => x.Other);
        });

        builder.Navigation(x => x.EmergencyContacts)
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.OwnsMany(x => x.GovernmentDocuments, governmentDocumentBuilder =>
        {
            governmentDocumentBuilder.ToTable("person_government_documents");

            governmentDocumentBuilder.WithOwner()
                .HasForeignKey("PersonId");

            governmentDocumentBuilder.Property<int>("Id");
            governmentDocumentBuilder.HasKey("Id");

            governmentDocumentBuilder.Property(x => x.Type)
                .IsRequired();

            governmentDocumentBuilder.Property(x => x.DocumentNumber)
                .IsRequired();

            governmentDocumentBuilder.Property(x => x.IssuingAuthority);

            governmentDocumentBuilder.Property(x => x.IssueDate);

            governmentDocumentBuilder.Property(x => x.ExpiryDate);

            governmentDocumentBuilder.Property(x => x.IsPrimary)
                .IsRequired();

            governmentDocumentBuilder.Property(x => x.IsVerified)
                .IsRequired();

            governmentDocumentBuilder.Property(x => x.Remarks);

            governmentDocumentBuilder.Property(x => x.Other);
        });

        builder.Navigation(x => x.GovernmentDocuments)
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.Property(x => x.DateOfBirth)
            .HasConversion(
                dateOfBirth => dateOfBirth == null ? (DateOnly?)null : dateOfBirth.Value,
                value => value.HasValue ? DateOfBirth.Create(value.Value) : null);

        builder.Property(x => x.Gender)
            .HasValueObjectConversion(Gender.Create)
            .HasMaxLength(50);

        builder.Property(x => x.MaritalStatus)
            .HasConversion(
                value => value == null ? null : value.Value,
                value => string.IsNullOrWhiteSpace(value) ? null : MaritalStatus.Create(value))
            .HasMaxLength(50);

        builder.Property(x => x.Nationality)
            .HasConversion(
                value => value == null ? null : value.Value,
                value => string.IsNullOrWhiteSpace(value) ? null : Nationality.Create(value))
            .HasMaxLength(100);

        builder.Property(x => x.Occupation)
            .HasConversion(
                value => value == null ? null : value.Value,
                value => string.IsNullOrWhiteSpace(value) ? null : Occupation.Create(value))
            .HasMaxLength(150);

        builder.Property(x => x.PreferredLanguage)
            .HasConversion(
                value => value == null ? null : value.Value,
                value => string.IsNullOrWhiteSpace(value) ? null : PreferredLanguage.Create(value))
            .HasMaxLength(50);

        builder.Property(x => x.Notes)
            .HasConversion(
                value => value == null ? null : value.Value,
                value => string.IsNullOrWhiteSpace(value) ? null : Notes.Create(value))
            .HasMaxLength(4000);

        builder.Property(x => x.Status)
            .HasValueObjectConversion(PersonStatus.Create)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(x => x.Description)
            .HasMaxLength(1000);

        builder.Property(x => x.Remarks)
            .HasMaxLength(2000);

        builder.Property(x => x.Other)
            .HasMaxLength(2000);

        builder.OwnsOne(x => x.PreferredContact, preferredContactBuilder =>
        {
            preferredContactBuilder.Property(x => x.Type)
                .HasColumnName("preferred_contact_type")
                .HasMaxLength(100);

            preferredContactBuilder.Property(x => x.Value)
                .HasColumnName("preferred_contact_value")
                .HasMaxLength(320);
        });

        builder.OwnsMany(x => x.CommunicationPreferences, preferenceBuilder =>
        {
            preferenceBuilder.ToTable("person_communication_preferences");

            preferenceBuilder.WithOwner()
                .HasForeignKey("PersonId");

            preferenceBuilder.Property<int>("Id");
            preferenceBuilder.HasKey("Id");

            preferenceBuilder.Property(x => x.Channel)
                .IsRequired();

            preferenceBuilder.Property(x => x.IsAllowed)
                .IsRequired();

            preferenceBuilder.Property(x => x.Remarks);
        });

        builder.Navigation(x => x.CommunicationPreferences)
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.OwnsMany(x => x.Relationships, relationshipBuilder =>
        {
            relationshipBuilder.ToTable("person_relationships");

            relationshipBuilder.WithOwner()
                .HasForeignKey("PersonId");

            relationshipBuilder.Property<int>("Id");
            relationshipBuilder.HasKey("Id");

            relationshipBuilder.Property(x => x.RelatedPersonId)
                .HasEntityIdConversion(PersonId.From)
                .HasColumnName("related_person_id")
                .IsRequired();

            relationshipBuilder.Property(x => x.Type)
                .IsRequired();

            relationshipBuilder.Property(x => x.Remarks);
        });

        builder.Navigation(x => x.Relationships)
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.Property(x => x.DisplayOrder)
            .IsRequired();

        builder.Property(x => x.IsHidden)
            .IsRequired();

        builder.Property(x => x.EffectiveFromUtc);

        builder.Property(x => x.EffectiveToUtc);

        builder.Ignore("DomainEvents");
    }

    private static PersonName DeserializePersonName(string json)
    {
        var model = JsonSerializer.Deserialize<PersonNamePersistenceModel>(json, JsonSerializerOptions.Web)
            ?? throw new InvalidOperationException("Unable to deserialize PersonName value object.");

        return PersonName.Create(
            model.FirstName,
            model.LastName,
            model.MiddleName,
            model.Title,
            model.Suffix);
    }

    private static FullName DeserializeFullName(string json)
    {
        var model = JsonSerializer.Deserialize<FullNamePersistenceModel>(json, JsonSerializerOptions.Web)
            ?? throw new InvalidOperationException("Unable to deserialize FullName value object.");

        return FullName.Create(
            model.FirstName,
            model.LastName,
            model.MiddleName,
            model.Title,
            model.Suffix);
    }

    private static string? SerializeAddress(Address? address)
    {
        if (address is null)
            return null;

        return JsonSerializer.Serialize(
            new AddressPersistenceModel(
                address.Type,
                address.Line1,
                address.Line2,
                address.Landmark,
                address.City,
                address.District,
                address.State,
                address.Country,
                address.PostalCode,
                address.IsPrimary,
                address.Remarks,
                address.Other),
            JsonSerializerOptions.Web);
    }

    private static Address? DeserializeAddress(string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
            return null;

        var model = JsonSerializer.Deserialize<AddressPersistenceModel>(json, JsonSerializerOptions.Web)
            ?? throw new InvalidOperationException("Unable to deserialize Address value object.");

        return Address.Create(
            model.Type,
            model.Line1,
            model.City,
            model.District,
            model.State,
            model.Country,
            model.PostalCode,
            model.Line2,
            model.Landmark,
            model.IsPrimary,
            model.Remarks,
            model.Other);
    }

    private sealed record FullNamePersistenceModel(
        string Title,
        string FirstName,
        string? MiddleName,
        string LastName,
        string? Suffix);

    private sealed record PersonNamePersistenceModel(
        string FirstName,
        string? MiddleName,
        string LastName,
        string? Title,
        string? Suffix);

    private sealed record AddressPersistenceModel(
        string Type,
        string Line1,
        string? Line2,
        string? Landmark,
        string City,
        string District,
        string State,
        string Country,
        string PostalCode,
        bool IsPrimary,
        string? Remarks,
        string? Other);
}
