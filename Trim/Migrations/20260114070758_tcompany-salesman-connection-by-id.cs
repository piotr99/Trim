using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Trim.Migrations
{
    /// <inheritdoc />
    public partial class tcompanysalesmanconnectionbyid : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "TransportCompanyId",
                table: "Vehicles",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ResponsibleSalesmanId",
                table: "TransportCompanies",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Vehicles_TransportCompanyId",
                table: "Vehicles",
                column: "TransportCompanyId");

            migrationBuilder.AddForeignKey(
                name: "FK_Vehicles_TransportCompanies_TransportCompanyId",
                table: "Vehicles",
                column: "TransportCompanyId",
                principalTable: "TransportCompanies",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Vehicles_TransportCompanies_TransportCompanyId",
                table: "Vehicles");

            migrationBuilder.DropIndex(
                name: "IX_Vehicles_TransportCompanyId",
                table: "Vehicles");

            migrationBuilder.DropColumn(
                name: "TransportCompanyId",
                table: "Vehicles");

            migrationBuilder.DropColumn(
                name: "ResponsibleSalesmanId",
                table: "TransportCompanies");
        }
    }
}
