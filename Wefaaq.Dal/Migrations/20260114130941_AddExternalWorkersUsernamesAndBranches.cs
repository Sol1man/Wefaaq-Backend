using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Wefaaq.Dal.Migrations
{
    /// <inheritdoc />
    public partial class AddExternalWorkersUsernamesAndBranches : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Organizations_Clients_ClientId",
                table: "Organizations");

            migrationBuilder.DropColumn(
                name: "ExternalWorkersCount",
                table: "Clients");

            migrationBuilder.AlterColumn<Guid>(
                name: "ClientId",
                table: "Organizations",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AddColumn<Guid>(
                name: "ClientBranchId",
                table: "Organizations",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ClientBranches",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    PhoneNumber = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Classification = table.Column<int>(type: "int", nullable: false),
                    Balance = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    BranchType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ParentClientId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClientBranches", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ClientBranches_Clients_ParentClientId",
                        column: x => x.ParentClientId,
                        principalTable: "Clients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "OrganizationUsernames",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SiteName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Username = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Password = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    OrganizationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrganizationUsernames", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OrganizationUsernames_Organizations_OrganizationId",
                        column: x => x.OrganizationId,
                        principalTable: "Organizations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ExternalWorkers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    ResidenceNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ResidenceImagePath = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    ExpiryDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    WorkerType = table.Column<int>(type: "int", nullable: false),
                    ClientId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ClientBranchId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExternalWorkers", x => x.Id);
                    table.CheckConstraint("CK_ExternalWorker_Client_XOR_Branch", "([ClientId] IS NOT NULL AND [ClientBranchId] IS NULL) OR ([ClientId] IS NULL AND [ClientBranchId] IS NOT NULL)");
                    table.ForeignKey(
                        name: "FK_ExternalWorkers_ClientBranches_ClientBranchId",
                        column: x => x.ClientBranchId,
                        principalTable: "ClientBranches",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ExternalWorkers_Clients_ClientId",
                        column: x => x.ClientId,
                        principalTable: "Clients",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Organizations_ClientBranchId",
                table: "Organizations",
                column: "ClientBranchId");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Organization_Client_XOR_Branch",
                table: "Organizations",
                sql: "([ClientId] IS NOT NULL AND [ClientBranchId] IS NULL) OR ([ClientId] IS NULL AND [ClientBranchId] IS NOT NULL)");

            migrationBuilder.CreateIndex(
                name: "IX_ClientBranches_Classification",
                table: "ClientBranches",
                column: "Classification");

            migrationBuilder.CreateIndex(
                name: "IX_ClientBranches_Name",
                table: "ClientBranches",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_ClientBranches_ParentClientId",
                table: "ClientBranches",
                column: "ParentClientId");

            migrationBuilder.CreateIndex(
                name: "IX_ExternalWorkers_ClientBranchId",
                table: "ExternalWorkers",
                column: "ClientBranchId");

            migrationBuilder.CreateIndex(
                name: "IX_ExternalWorkers_ClientId",
                table: "ExternalWorkers",
                column: "ClientId");

            migrationBuilder.CreateIndex(
                name: "IX_ExternalWorkers_ExpiryDate",
                table: "ExternalWorkers",
                column: "ExpiryDate");

            migrationBuilder.CreateIndex(
                name: "IX_ExternalWorkers_ResidenceNumber",
                table: "ExternalWorkers",
                column: "ResidenceNumber");

            migrationBuilder.CreateIndex(
                name: "IX_ExternalWorkers_WorkerType",
                table: "ExternalWorkers",
                column: "WorkerType");

            migrationBuilder.CreateIndex(
                name: "IX_OrganizationUsernames_OrganizationId",
                table: "OrganizationUsernames",
                column: "OrganizationId");

            migrationBuilder.CreateIndex(
                name: "IX_OrganizationUsernames_SiteName",
                table: "OrganizationUsernames",
                column: "SiteName");

            migrationBuilder.AddForeignKey(
                name: "FK_Organizations_ClientBranches_ClientBranchId",
                table: "Organizations",
                column: "ClientBranchId",
                principalTable: "ClientBranches",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Organizations_Clients_ClientId",
                table: "Organizations",
                column: "ClientId",
                principalTable: "Clients",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Organizations_ClientBranches_ClientBranchId",
                table: "Organizations");

            migrationBuilder.DropForeignKey(
                name: "FK_Organizations_Clients_ClientId",
                table: "Organizations");

            migrationBuilder.DropTable(
                name: "ExternalWorkers");

            migrationBuilder.DropTable(
                name: "OrganizationUsernames");

            migrationBuilder.DropTable(
                name: "ClientBranches");

            migrationBuilder.DropIndex(
                name: "IX_Organizations_ClientBranchId",
                table: "Organizations");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Organization_Client_XOR_Branch",
                table: "Organizations");

            migrationBuilder.DropColumn(
                name: "ClientBranchId",
                table: "Organizations");

            migrationBuilder.AlterColumn<Guid>(
                name: "ClientId",
                table: "Organizations",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ExternalWorkersCount",
                table: "Clients",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddForeignKey(
                name: "FK_Organizations_Clients_ClientId",
                table: "Organizations",
                column: "ClientId",
                principalTable: "Clients",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
