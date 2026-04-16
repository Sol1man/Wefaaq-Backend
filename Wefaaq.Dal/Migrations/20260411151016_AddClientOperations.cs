using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Wefaaq.Dal.Migrations
{
    /// <inheritdoc />
    public partial class AddClientOperations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ClientOperations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Type = table.Column<int>(type: "int", nullable: false),
                    TargetType = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    Price = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    ClientId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ClientBranchId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    OrganizationId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ExternalPersonName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    ExternalPersonIdNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    PerformedByUserId = table.Column<int>(type: "int", nullable: false),
                    CompletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClientOperations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ClientOperations_ClientBranches_ClientBranchId",
                        column: x => x.ClientBranchId,
                        principalTable: "ClientBranches",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ClientOperations_Clients_ClientId",
                        column: x => x.ClientId,
                        principalTable: "Clients",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ClientOperations_Organizations_OrganizationId",
                        column: x => x.OrganizationId,
                        principalTable: "Organizations",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ClientOperations_Users_PerformedByUserId",
                        column: x => x.PerformedByUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ClientOperations_ClientBranchId",
                table: "ClientOperations",
                column: "ClientBranchId");

            migrationBuilder.CreateIndex(
                name: "IX_ClientOperations_ClientId",
                table: "ClientOperations",
                column: "ClientId");

            migrationBuilder.CreateIndex(
                name: "IX_ClientOperations_CreatedAt",
                table: "ClientOperations",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_ClientOperations_OrganizationId",
                table: "ClientOperations",
                column: "OrganizationId");

            migrationBuilder.CreateIndex(
                name: "IX_ClientOperations_PerformedByUserId",
                table: "ClientOperations",
                column: "PerformedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ClientOperations_Status",
                table: "ClientOperations",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_ClientOperations_Type",
                table: "ClientOperations",
                column: "Type");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ClientOperations");
        }
    }
}
