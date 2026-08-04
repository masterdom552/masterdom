using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Masterdom.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class PersonDomainFoundation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "FullName",
                table: "Persons",
                newName: "Name");

            migrationBuilder.RenameColumn(
                name: "Code",
                table: "Persons",
                newName: "Number");

            migrationBuilder.RenameIndex(
                name: "IX_Persons_Code",
                table: "Persons",
                newName: "IX_Persons_Number");

            migrationBuilder.AddColumn<string>(
                name: "Nationality",
                table: "Persons",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Notes",
                table: "Persons",
                type: "character varying(4000)",
                maxLength: 4000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Occupation",
                table: "Persons",
                type: "character varying(150)",
                maxLength: 150,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PreferredLanguage",
                table: "Persons",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "preferred_contact_type",
                table: "Persons",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "preferred_contact_value",
                table: "Persons",
                type: "character varying(320)",
                maxLength: 320,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "person_communication_preferences",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Channel = table.Column<string>(type: "text", nullable: false),
                    IsAllowed = table.Column<bool>(type: "boolean", nullable: false),
                    Remarks = table.Column<string>(type: "text", nullable: true),
                    PersonId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_person_communication_preferences", x => x.Id);
                    table.ForeignKey(
                        name: "FK_person_communication_preferences_Persons_PersonId",
                        column: x => x.PersonId,
                        principalTable: "Persons",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "person_emergency_contacts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    FullName = table.Column<string>(type: "jsonb", nullable: false),
                    Relationship = table.Column<string>(type: "text", nullable: false),
                    MobileNumber = table.Column<string>(type: "text", nullable: false),
                    AlternateMobileNumber = table.Column<string>(type: "text", nullable: true),
                    EmailAddress = table.Column<string>(type: "text", nullable: true),
                    Address = table.Column<string>(type: "jsonb", nullable: true),
                    IsPrimary = table.Column<bool>(type: "boolean", nullable: false),
                    Remarks = table.Column<string>(type: "text", nullable: true),
                    Other = table.Column<string>(type: "text", nullable: true),
                    PersonId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_person_emergency_contacts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_person_emergency_contacts_Persons_PersonId",
                        column: x => x.PersonId,
                        principalTable: "Persons",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "person_relationships",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    related_person_id = table.Column<Guid>(type: "uuid", nullable: false),
                    Type = table.Column<string>(type: "text", nullable: false),
                    Remarks = table.Column<string>(type: "text", nullable: true),
                    PersonId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_person_relationships", x => x.Id);
                    table.ForeignKey(
                        name: "FK_person_relationships_Persons_PersonId",
                        column: x => x.PersonId,
                        principalTable: "Persons",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_person_communication_preferences_PersonId",
                table: "person_communication_preferences",
                column: "PersonId");

            migrationBuilder.CreateIndex(
                name: "IX_person_emergency_contacts_PersonId",
                table: "person_emergency_contacts",
                column: "PersonId");

            migrationBuilder.CreateIndex(
                name: "IX_person_relationships_PersonId",
                table: "person_relationships",
                column: "PersonId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "person_communication_preferences");

            migrationBuilder.DropTable(
                name: "person_emergency_contacts");

            migrationBuilder.DropTable(
                name: "person_relationships");

            migrationBuilder.DropColumn(
                name: "Nationality",
                table: "Persons");

            migrationBuilder.DropColumn(
                name: "Notes",
                table: "Persons");

            migrationBuilder.DropColumn(
                name: "Occupation",
                table: "Persons");

            migrationBuilder.DropColumn(
                name: "PreferredLanguage",
                table: "Persons");

            migrationBuilder.DropColumn(
                name: "preferred_contact_type",
                table: "Persons");

            migrationBuilder.DropColumn(
                name: "preferred_contact_value",
                table: "Persons");

            migrationBuilder.RenameColumn(
                name: "Number",
                table: "Persons",
                newName: "Code");

            migrationBuilder.RenameColumn(
                name: "Name",
                table: "Persons",
                newName: "FullName");

            migrationBuilder.RenameIndex(
                name: "IX_Persons_Number",
                table: "Persons",
                newName: "IX_Persons_Code");
        }
    }
}
