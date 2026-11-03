using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BankingBackend.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class UniqueAccountNumber : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_accounts_number",
                table: "accounts",
                column: "number",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_accounts_number",
                table: "accounts");
        }
    }
}
