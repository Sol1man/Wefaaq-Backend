using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Wefaaq.Dal.Migrations
{
    /// <inheritdoc />
    public partial class UpdateClientOrganizationRelationship : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ClientOrganizations");

            migrationBuilder.AddColumn<Guid>(
                name: "ClientId",
                table: "Organizations",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_Organizations_ClientId",
                table: "Organizations",
                column: "ClientId");

            migrationBuilder.AddForeignKey(
                name: "FK_Organizations_Clients_ClientId",
                table: "Organizations",
                column: "ClientId",
                principalTable: "Clients",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Organizations_Clients_ClientId",
                table: "Organizations");

            migrationBuilder.DropIndex(
                name: "IX_Organizations_ClientId",
                table: "Organizations");

            migrationBuilder.DropColumn(
                name: "ClientId",
                table: "Organizations");

            migrationBuilder.CreateTable(
                name: "ClientOrganizations",
                columns: table => new
                {
                    ClientId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OrganizationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClientOrganizations", x => new { x.ClientId, x.OrganizationId });
                    table.ForeignKey(
                        name: "FK_ClientOrganizations_Clients_ClientId",
                        column: x => x.ClientId,
                        principalTable: "Clients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ClientOrganizations_Organizations_OrganizationId",
                        column: x => x.OrganizationId,
                        principalTable: "Organizations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ClientOrganizations_OrganizationId",
                table: "ClientOrganizations",
                column: "OrganizationId");
        }
    }
}
