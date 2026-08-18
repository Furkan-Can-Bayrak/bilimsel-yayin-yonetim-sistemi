using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Blog.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddManuscriptAuthor : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "AuthorId",
                table: "Manuscripts",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.Sql("""
                UPDATE m
                SET m.AuthorId = COALESCE(
                    (SELECT TOP 1 u.Id FROM Users u WHERE u.Email = N'author@yayin.local'),
                    (SELECT TOP 1 u.Id FROM Users u ORDER BY u.Id)
                )
                FROM Manuscripts m;
                """);

            migrationBuilder.AlterColumn<int>(
                name: "AuthorId",
                table: "Manuscripts",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int",
                oldDefaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Manuscripts_AuthorId",
                table: "Manuscripts",
                column: "AuthorId");

            migrationBuilder.AddForeignKey(
                name: "FK_Manuscripts_Users_AuthorId",
                table: "Manuscripts",
                column: "AuthorId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Manuscripts_Users_AuthorId",
                table: "Manuscripts");

            migrationBuilder.DropIndex(
                name: "IX_Manuscripts_AuthorId",
                table: "Manuscripts");

            migrationBuilder.DropColumn(
                name: "AuthorId",
                table: "Manuscripts");
        }
    }
}
