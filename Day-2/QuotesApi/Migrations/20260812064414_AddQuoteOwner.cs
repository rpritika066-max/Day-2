using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuotesApi.Migrations
{
    /// <inheritdoc />
    public partial class AddQuoteOwner : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
migrationBuilder.AddColumn<int>(
    name: "UserId",
    table: "Quotes",
    type: "INTEGER",
    nullable: false,
    defaultValue: 1);

            migrationBuilder.CreateIndex(
                name: "IX_Quotes_UserId",
                table: "Quotes",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Quotes_Users_UserId",
                table: "Quotes",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Quotes_Users_UserId",
                table: "Quotes");

            migrationBuilder.DropIndex(
                name: "IX_Quotes_UserId",
                table: "Quotes");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "Quotes");
        }
    }
}
