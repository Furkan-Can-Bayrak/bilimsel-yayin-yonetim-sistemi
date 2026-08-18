using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Blog.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class SplitUserFullName : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "FirstName",
                table: "Users",
                type: "nvarchar(80)",
                maxLength: 80,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "LastName",
                table: "Users",
                type: "nvarchar(80)",
                maxLength: 80,
                nullable: false,
                defaultValue: "");

            migrationBuilder.Sql("""
                UPDATE [Users]
                SET
                    [FirstName] = CASE
                        WHEN CHARINDEX(' ', LTRIM(RTRIM([FullName]))) = 0 THEN LTRIM(RTRIM([FullName]))
                        ELSE LEFT(LTRIM(RTRIM([FullName])), LEN(LTRIM(RTRIM([FullName]))) - CHARINDEX(N' ', REVERSE(LTRIM(RTRIM([FullName])))))
                    END,
                    [LastName] = CASE
                        WHEN CHARINDEX(' ', LTRIM(RTRIM([FullName]))) = 0 THEN N'-'
                        ELSE LTRIM(RIGHT(LTRIM(RTRIM([FullName])), CHARINDEX(N' ', REVERSE(LTRIM(RTRIM([FullName])))) - 1))
                    END
                """);

            migrationBuilder.DropColumn(
                name: "FullName",
                table: "Users");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "FullName",
                table: "Users",
                type: "nvarchar(150)",
                maxLength: 150,
                nullable: false,
                defaultValue: "");

            migrationBuilder.Sql("""
                UPDATE [Users]
                SET [FullName] = LTRIM(RTRIM([FirstName] + N' ' + [LastName]))
                """);

            migrationBuilder.DropColumn(
                name: "FirstName",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "LastName",
                table: "Users");
        }
    }
}
