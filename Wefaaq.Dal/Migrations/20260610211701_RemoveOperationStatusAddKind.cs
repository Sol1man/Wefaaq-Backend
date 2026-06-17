using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Wefaaq.Dal.Migrations
{
    /// <inheritdoc />
    public partial class RemoveOperationStatusAddKind : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ClientOperations_Status",
                table: "ClientOperations");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "ClientOperations");

            // Type is now optional (payment records have no operation type)
            migrationBuilder.AlterColumn<int>(
                name: "Type",
                table: "ClientOperations",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            // Record kind: 1 = Service (debit), 2 = Payment (credit). Existing rows default to Service.
            migrationBuilder.AddColumn<int>(
                name: "Kind",
                table: "ClientOperations",
                type: "int",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.CreateIndex(
                name: "IX_ClientOperations_Kind",
                table: "ClientOperations",
                column: "Kind");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ClientOperations_Kind",
                table: "ClientOperations");

            migrationBuilder.DropColumn(
                name: "Kind",
                table: "ClientOperations");

            migrationBuilder.AlterColumn<int>(
                name: "Type",
                table: "ClientOperations",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "ClientOperations",
                type: "int",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.CreateIndex(
                name: "IX_ClientOperations_Status",
                table: "ClientOperations",
                column: "Status");
        }
    }
}
