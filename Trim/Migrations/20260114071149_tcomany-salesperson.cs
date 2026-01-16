using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Trim.Migrations
{
    /// <inheritdoc />
    public partial class tcomanysalesperson : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ResponsibleSalesmanId",
                table: "TransportCompanies");

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
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
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
                name: "ResponsibleSalesmanId",
                table: "TransportCompanies",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }
    }
}
