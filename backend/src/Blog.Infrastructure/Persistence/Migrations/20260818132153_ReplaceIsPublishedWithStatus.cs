using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Blog.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class ReplaceIsPublishedWithStatus : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "Manuscripts",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.Sql("""
                UPDATE Manuscripts
                SET Status = CASE WHEN IsPublished = 1 THEN 5 ELSE 0 END;
                """);

            migrationBuilder.DropColumn(
                name: "IsPublished",
                table: "Manuscripts");

            migrationBuilder.CreateIndex(
                name: "IX_Manuscripts_Status",
                table: "Manuscripts",
                column: "Status");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Manuscripts_Status",
                table: "Manuscripts");

            migrationBuilder.AddColumn<bool>(
                name: "IsPublished",
                table: "Manuscripts",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.Sql("""
                UPDATE Manuscripts
                SET IsPublished = CASE WHEN Status = 5 THEN 1 ELSE 0 END;
                """);

            migrationBuilder.DropColumn(
                name: "Status",
                table: "Manuscripts");
        }
    }
}
