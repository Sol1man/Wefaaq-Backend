using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Wefaaq.Dal.Migrations
{
    /// <inheritdoc />
    public partial class AddNameToRecordsAndLicenses : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Name",
                table: "OrganizationRecords",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Name",
                table: "OrganizationLicenses",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Name",
                table: "OrganizationRecords");

            migrationBuilder.DropColumn(
                name: "Name",
                table: "OrganizationLicenses");
        }
    }
}
