using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Wefaaq.Dal.Migrations
{
    /// <inheritdoc />
    public partial class AddPaymentTypeAndAccountAmounts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "CurrentAccountAmount",
                table: "Users",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "InitialAccountAmount",
                table: "Users",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<Guid>(
                name: "RelatedPaymentId",
                table: "UserPayments",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Type",
                table: "UserPayments",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_UserPayments_RelatedPaymentId",
                table: "UserPayments",
                column: "RelatedPaymentId");

            migrationBuilder.CreateIndex(
                name: "IX_UserPayments_Type",
                table: "UserPayments",
                column: "Type");

            migrationBuilder.AddForeignKey(
                name: "FK_UserPayments_UserPayments_RelatedPaymentId",
                table: "UserPayments",
                column: "RelatedPaymentId",
                principalTable: "UserPayments",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserPayments_UserPayments_RelatedPaymentId",
                table: "UserPayments");

            migrationBuilder.DropIndex(
                name: "IX_UserPayments_RelatedPaymentId",
                table: "UserPayments");

            migrationBuilder.DropIndex(
                name: "IX_UserPayments_Type",
                table: "UserPayments");

            migrationBuilder.DropColumn(
                name: "CurrentAccountAmount",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "InitialAccountAmount",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "RelatedPaymentId",
                table: "UserPayments");

            migrationBuilder.DropColumn(
                name: "Type",
                table: "UserPayments");
        }
    }
}
