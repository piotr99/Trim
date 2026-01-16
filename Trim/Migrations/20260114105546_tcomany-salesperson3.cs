using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Trim.Migrations
{
    /// <inheritdoc />
    public partial class tcomanysalesperson3 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TransportCompanies_AspNetUsers_SalespersonId",
                table: "TransportCompanies");

            migrationBuilder.DropIndex(
                name: "IX_TransportCompanies_SalespersonId",
                table: "TransportCompanies");

            migrationBuilder.DropColumn(
                name: "SalespersonId",
                table: "TransportCompanies");

            migrationBuilder.AddColumn<int>(
                name: "SalespersonId",
                table: "Customers",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Customers_SalespersonId",
                table: "Customers",
                column: "SalespersonId");

            migrationBuilder.AddForeignKey(
                name: "FK_Customers_AspNetUsers_SalespersonId",
                table: "Customers",
                column: "SalespersonId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Customers_AspNetUsers_SalespersonId",
                table: "Customers");

            migrationBuilder.DropIndex(
                name: "IX_Customers_SalespersonId",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "SalespersonId",
                table: "Customers");

            migrationBuilder.AddColumn<int>(
                name: "SalespersonId",
                table: "TransportCompanies",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_TransportCompanies_SalespersonId",
                table: "TransportCompanies",
                column: "SalespersonId");

            migrationBuilder.AddForeignKey(
                name: "FK_TransportCompanies_AspNetUsers_SalespersonId",
                table: "TransportCompanies",
                column: "SalespersonId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }
    }
}
