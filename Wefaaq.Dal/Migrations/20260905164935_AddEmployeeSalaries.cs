using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Wefaaq.Dal.Migrations
{
    /// <inheritdoc />
    public partial class AddEmployeeSalaries : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "Salary",
                table: "Users",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.CreateTable(
                name: "ExternalEmployees",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Salary = table.Column<decimal>(type: "decimal(18,2)", nullable: false, defaultValue: 0m),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExternalEmployees", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "EmployeeDeductions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    DeductionDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UserId = table.Column<int>(type: "int", nullable: true),
                    ExternalEmployeeId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmployeeDeductions", x => x.Id);
                    table.CheckConstraint("CK_EmployeeDeduction_User_XOR_External", "([UserId] IS NOT NULL AND [ExternalEmployeeId] IS NULL) OR ([UserId] IS NULL AND [ExternalEmployeeId] IS NOT NULL)");
                    table.ForeignKey(
                        name: "FK_EmployeeDeductions_ExternalEmployees_ExternalEmployeeId",
                        column: x => x.ExternalEmployeeId,
                        principalTable: "ExternalEmployees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_EmployeeDeductions_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeDeductions_DeductionDate",
                table: "EmployeeDeductions",
                column: "DeductionDate");

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeDeductions_ExternalEmployeeId",
                table: "EmployeeDeductions",
                column: "ExternalEmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeDeductions_UserId",
                table: "EmployeeDeductions",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_ExternalEmployees_Name",
                table: "ExternalEmployees",
                column: "Name");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EmployeeDeductions");

            migrationBuilder.DropTable(
                name: "ExternalEmployees");

            migrationBuilder.DropColumn(
                name: "Salary",
                table: "Users");
        }
    }
}
