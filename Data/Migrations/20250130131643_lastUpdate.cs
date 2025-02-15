using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Invoice_Generator.Data.Migrations
{
    /// <inheritdoc />
    public partial class lastUpdate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_invoices_clients_BillToClientId",
                table: "invoices");

            migrationBuilder.DropIndex(
                name: "IX_invoices_BillToClientId",
                table: "invoices");

            migrationBuilder.DropColumn(
                name: "BillToClientId",
                table: "invoices");

            migrationBuilder.DropColumn(
                name: "IsClientInvoice",
                table: "invoices");

            migrationBuilder.DropColumn(
                name: "ShipToAddress",
                table: "invoices");

            migrationBuilder.RenameColumn(
                name: "TaxPercentage",
                table: "invoices",
                newName: "Shipping");

            migrationBuilder.AddColumn<decimal>(
                name: "Discount",
                table: "invoices",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Discount",
                table: "invoices");

            migrationBuilder.RenameColumn(
                name: "Shipping",
                table: "invoices",
                newName: "TaxPercentage");

            migrationBuilder.AddColumn<int>(
                name: "BillToClientId",
                table: "invoices",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsClientInvoice",
                table: "invoices",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "ShipToAddress",
                table: "invoices",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_invoices_BillToClientId",
                table: "invoices",
                column: "BillToClientId");

            migrationBuilder.AddForeignKey(
                name: "FK_invoices_clients_BillToClientId",
                table: "invoices",
                column: "BillToClientId",
                principalTable: "clients",
                principalColumn: "Id");
        }
    }
}
