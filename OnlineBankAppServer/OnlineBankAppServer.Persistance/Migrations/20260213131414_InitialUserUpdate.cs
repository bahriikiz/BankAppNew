using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OnlineBankAppServer.Persistance.Migrations
{
    /// <inheritdoc />
    public partial class InitialUserUpdate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "CreaatedAt",
                table: "Users",
                newName: "CreatedAt");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "Users",
                newName: "CreaatedAt");
        }
    }
}
