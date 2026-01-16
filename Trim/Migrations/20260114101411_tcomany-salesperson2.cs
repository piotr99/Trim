using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Trim.Migrations
{
    /// <inheritdoc />
    public partial class tcomanysalesperson2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TransportCompanies_AspNetUsers_SalespersonId",
                table: "TransportCompanies");

            migrationBuilder.AddForeignKey(
                name: "FK_TransportCompanies_AspNetUsers_SalespersonId",
                table: "TransportCompanies",
                column: "SalespersonId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TransportCompanies_AspNetUsers_SalespersonId",
                table: "TransportCompanies");

            migrationBuilder.AddForeignKey(
                name: "FK_TransportCompanies_AspNetUsers_SalespersonId",
                table: "TransportCompanies",
                column: "SalespersonId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }
    }
}
