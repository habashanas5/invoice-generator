using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Invoice_Generator.Data.Migrations
{
    /// <inheritdoc />
    public partial class UpdateInvoiceTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_invoices_clients_ClientId",
                table: "invoices");

            migrationBuilder.AlterColumn<int>(
                name: "ClientId",
                table: "invoices",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

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

            migrationBuilder.AddForeignKey(
                name: "FK_invoices_clients_ClientId",
                table: "invoices",
                column: "ClientId",
                principalTable: "clients",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_invoices_clients_BillToClientId",
                table: "invoices");

            migrationBuilder.DropForeignKey(
                name: "FK_invoices_clients_ClientId",
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

            migrationBuilder.AlterColumn<int>(
                name: "ClientId",
                table: "invoices",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddForeignKey(
                name: "FK_invoices_clients_ClientId",
                table: "invoices",
                column: "ClientId",
                principalTable: "clients",
                principalColumn: "Id");
        }
    }
}
