using Masterdom.Core.Identity.Entities.Organization;

namespace Masterdom.Core.Tests.Identity;

public sealed class OrganizationTests
{
    [Fact]
    public void AddContact_WhenPrimaryAlreadyExists_ShouldThrow()
    {
        var organization = CreateOrganization();

        organization.AddContact(Contact.Create("Email", "primary@example.com", isPrimary: true));

        var candidate = Contact.Create("Phone", "+15551234567", isPrimary: true);

        var action = () => organization.AddContact(candidate);

        Assert.Throws<InvalidOperationException>(action);
    }

    [Fact]
    public void AddAddress_WhenPrimaryAlreadyExists_ShouldThrow()
    {
        var organization = CreateOrganization();

        organization.AddAddress(Address.Create(
            type: "HeadOffice",
            line1: "Line 1",
            city: "City",
            district: "District",
            state: "State",
            country: "Country",
            postalCode: "10001",
            isPrimary: true));

        var candidate = Address.Create(
            type: "Branch",
            line1: "Line 2",
            city: "City",
            district: "District",
            state: "State",
            country: "Country",
            postalCode: "10002",
            isPrimary: true);

        var action = () => organization.AddAddress(candidate);

        Assert.Throws<InvalidOperationException>(action);
    }

    [Fact]
    public void AddRegistrationDocument_WhenPrimaryAlreadyExists_ShouldThrow()
    {
        var organization = CreateOrganization();

        organization.AddRegistrationDocument(
            RegistrationDocument.Create(
                type: "GST",
                documentNumber: "GST-001",
                isPrimary: true));

        var candidate = RegistrationDocument.Create(
            type: "TIN",
            documentNumber: "TIN-001",
            isPrimary: true);

        var action = () => organization.AddRegistrationDocument(candidate);

        Assert.Throws<InvalidOperationException>(action);
    }

    [Fact]
    public void AddContact_WhenPrimaryDoesNotExist_ShouldAddPrimary()
    {
        var organization = CreateOrganization();

        var contact = Contact.Create("Email", "primary@example.com", isPrimary: true);

        organization.AddContact(contact);

        Assert.Single(organization.Contacts);
        Assert.True(organization.Contacts.Single().IsPrimary);
    }

    private static Organization CreateOrganization()
    {
        return Organization.Create(
            OrganizationCode.Create("ORG-001"),
            OrganizationName.Create("Acme Properties"),
            OrganizationType.Enterprise);
    }
}
