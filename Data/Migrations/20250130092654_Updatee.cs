using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Invoice_Generator.Data.Migrations
{
    /// <inheritdoc />
    public partial class Updatee : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_companies_Users_ApplicationUserId",
                table: "companies");

            migrationBuilder.DropIndex(
                name: "IX_companies_ApplicationUserId",
                table: "companies");

            migrationBuilder.DropColumn(
                name: "ApplicationUserId",
                table: "companies");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ApplicationUserId",
                table: "companies",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_companies_ApplicationUserId",
                table: "companies",
                column: "ApplicationUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_companies_Users_ApplicationUserId",
                table: "companies",
                column: "ApplicationUserId",
                principalSchema: "security",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
