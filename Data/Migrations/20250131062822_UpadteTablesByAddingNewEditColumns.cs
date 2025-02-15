using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Invoice_Generator.Data.Migrations
{
    /// <inheritdoc />
    public partial class UpadteTablesByAddingNewEditColumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_invoices_clients_ClientId",
                table: "invoices");

            migrationBuilder.DropForeignKey(
                name: "FK_invoices_companies_CompanyId",
                table: "invoices");

            migrationBuilder.AlterColumn<int>(
                name: "CompanyId",
                table: "invoices",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "ClientId",
                table: "invoices",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Email",
                table: "clients",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddForeignKey(
                name: "FK_invoices_clients_ClientId",
                table: "invoices",
                column: "ClientId",
                principalTable: "clients",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_invoices_companies_CompanyId",
                table: "invoices",
                column: "CompanyId",
                principalTable: "companies",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_invoices_clients_ClientId",
                table: "invoices");

            migrationBuilder.DropForeignKey(
                name: "FK_invoices_companies_CompanyId",
                table: "invoices");

            migrationBuilder.DropColumn(
                name: "Email",
                table: "clients");

            migrationBuilder.AlterColumn<int>(
                name: "CompanyId",
                table: "invoices",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

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

            migrationBuilder.AddForeignKey(
                name: "FK_invoices_companies_CompanyId",
                table: "invoices",
                column: "CompanyId",
                principalTable: "companies",
                principalColumn: "Id");
        }
    }
}
