using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Invoice_Generator.Data.Migrations
{
    /// <inheritdoc />
    public partial class UpdateInvoiceTablee : Migration
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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
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

            migrationBuilder.AddForeignKey(
                name: "FK_invoices_clients_ClientId",
                table: "invoices",
                column: "ClientId",
                principalTable: "clients",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
