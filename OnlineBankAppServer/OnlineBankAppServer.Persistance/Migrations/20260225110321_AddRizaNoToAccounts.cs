using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OnlineBankAppServer.Persistance.Migrations
{
    /// <inheritdoc />
    public partial class AddRizaNoToAccounts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "RizaNo",
                table: "Accounts",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RizaNo",
                table: "Accounts");
        }
    }
}
