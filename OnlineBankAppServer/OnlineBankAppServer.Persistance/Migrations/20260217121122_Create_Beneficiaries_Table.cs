using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OnlineBankAppServer.Persistance.Migrations
{
    /// <inheritdoc />
    public partial class Create_Beneficiaries_Table : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Createddate",
                table: "Users",
                newName: "CreatedDate");

            migrationBuilder.RenameColumn(
                name: "Createddate",
                table: "BankTransactions",
                newName: "CreatedDate");

            migrationBuilder.RenameColumn(
                name: "Createddate",
                table: "Banks",
                newName: "CreatedDate");

            migrationBuilder.RenameColumn(
                name: "Createddate",
                table: "Accounts",
                newName: "CreatedDate");

            migrationBuilder.CreateTable(
                name: "Beneficiaries",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Iban = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Beneficiaries", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Beneficiaries");

            migrationBuilder.RenameColumn(
                name: "CreatedDate",
                table: "Users",
                newName: "Createddate");

            migrationBuilder.RenameColumn(
                name: "CreatedDate",
                table: "BankTransactions",
                newName: "Createddate");

            migrationBuilder.RenameColumn(
                name: "CreatedDate",
                table: "Banks",
                newName: "Createddate");

            migrationBuilder.RenameColumn(
                name: "CreatedDate",
                table: "Accounts",
                newName: "Createddate");
        }
    }
}
