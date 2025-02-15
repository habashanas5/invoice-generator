using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Invoice_Generator.Data.Migrations
{
    /// <inheritdoc />
    public partial class Updateee : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Email",
                table: "companies");

            migrationBuilder.DropColumn(
                name: "Email",
                table: "clients");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Email",
                table: "companies",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Email",
                table: "clients",
                type: "nvarchar(max)",
                nullable: true);
        }
    }
}
