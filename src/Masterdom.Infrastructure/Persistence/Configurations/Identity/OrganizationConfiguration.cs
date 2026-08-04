using Masterdom.Core.Identity.Entities.Organization;
using Masterdom.Infrastructure.Persistence.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Masterdom.Infrastructure.Persistence.Configurations.Identity;

/// <summary>
/// EF Core configuration for <see cref="Organization"/>.
/// </summary>
public sealed class OrganizationConfiguration : IEntityTypeConfiguration<Organization>
{
    public void Configure(EntityTypeBuilder<Organization> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.ToTable("Organizations");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasEntityIdConversion(OrganizationId.From)
            .ValueGeneratedNever();

        builder.Property(x => x.Code)
            .HasValueObjectConversion(OrganizationCode.Create)
            .HasMaxLength(50)
            .IsRequired();

        builder.HasIndex(x => x.Code)
            .IsUnique();

        builder.Property(x => x.Name)
            .HasValueObjectConversion(OrganizationName.Create)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(x => x.Type)
            .HasValueObjectConversion(OrganizationType.Create)
            .HasMaxLength(50)
            .IsRequired();

        builder.OwnsMany(x => x.Addresses, addressBuilder =>
        {
            addressBuilder.ToTable("organization_addresses");

            addressBuilder.WithOwner()
                .HasForeignKey("OrganizationId");

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
            contactBuilder.ToTable("organization_contacts");

            contactBuilder.WithOwner()
                .HasForeignKey("OrganizationId");

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

        builder.OwnsMany(x => x.RegistrationDocuments, registrationDocumentBuilder =>
        {
            registrationDocumentBuilder.ToTable("organization_registration_documents");

            registrationDocumentBuilder.WithOwner()
                .HasForeignKey("OrganizationId");

            registrationDocumentBuilder.Property<int>("Id");
            registrationDocumentBuilder.HasKey("Id");

            registrationDocumentBuilder.Property(x => x.Type)
                .IsRequired();

            registrationDocumentBuilder.Property(x => x.DocumentNumber)
                .IsRequired();

            registrationDocumentBuilder.Property(x => x.IssuingAuthority);

            registrationDocumentBuilder.Property(x => x.IssueDate);

            registrationDocumentBuilder.Property(x => x.ExpiryDate);

            registrationDocumentBuilder.Property(x => x.IsPrimary)
                .IsRequired();

            registrationDocumentBuilder.Property(x => x.IsVerified)
                .IsRequired();

            registrationDocumentBuilder.Property(x => x.Remarks);

            registrationDocumentBuilder.Property(x => x.Other);
        });

        builder.Navigation(x => x.RegistrationDocuments)
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.Property(x => x.Status)
            .HasValueObjectConversion(OrganizationStatus.Create)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(x => x.Description)
            .HasMaxLength(1000);

        builder.Property(x => x.Remarks)
            .HasMaxLength(2000);

        builder.Property(x => x.Other)
            .HasMaxLength(2000);

        builder.Property(x => x.DisplayOrder)
            .IsRequired();

        builder.Property(x => x.IsHidden)
            .IsRequired();

        builder.Property(x => x.EffectiveFromUtc);

        builder.Property(x => x.EffectiveToUtc);

        builder.Ignore("DomainEvents");
    }
}
