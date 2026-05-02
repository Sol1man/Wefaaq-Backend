using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Wefaaq.Dal.Migrations
{
    /// <inheritdoc />
    public partial class AddClientAssignment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "AssignedUserId",
                table: "Clients",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Clients_AssignedUserId",
                table: "Clients",
                column: "AssignedUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Clients_Users_AssignedUserId",
                table: "Clients",
                column: "AssignedUserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Clients_Users_AssignedUserId",
                table: "Clients");

            migrationBuilder.DropIndex(
                name: "IX_Clients_AssignedUserId",
                table: "Clients");

            migrationBuilder.DropColumn(
                name: "AssignedUserId",
                table: "Clients");
        }
    }
}
