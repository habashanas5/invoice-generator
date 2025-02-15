using Microsoft.EntityFrameworkCore.Migrations;
using System.Collections.Generic;

#nullable disable

namespace Invoice_Generator.Data.Migrations
{
    /// <inheritdoc />
    public partial class AssignAdminUserToAllRoles : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            //     migrationBuilder.Sql(" INSERT INTO[security].[UserRoles] (UserId, RoleId) SELECT '8af9badd-be87-4f52-93dd-ae091b4bafef', Id FROM[security].[Roles] ;");

        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            //       migrationBuilder.Sql("DELETE FROM [security].[UserRoles] WHERE UserId = '8af9badd-be87-4f52-93dd-ae091b4bafef'");
        }
    }
}
