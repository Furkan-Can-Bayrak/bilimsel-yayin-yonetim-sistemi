using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Blog.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AcademicTitleEnum : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "AcademicTitleCode",
                table: "Users",
                type: "int",
                nullable: true);

            migrationBuilder.Sql("""
                UPDATE [Users]
                SET [AcademicTitleCode] = CASE [AcademicTitle]
                    WHEN N'Prof. Dr.' THEN 1
                    WHEN N'Doç. Dr.' THEN 2
                    WHEN N'Dr. Öğr. Üyesi' THEN 3
                    WHEN N'Öğr. Gör.' THEN 4
                    WHEN N'Arş. Gör.' THEN 5
                    WHEN N'Dr.' THEN 6
                    ELSE NULL
                END
                """);

            migrationBuilder.DropColumn(
                name: "AcademicTitle",
                table: "Users");

            migrationBuilder.RenameColumn(
                name: "AcademicTitleCode",
                table: "Users",
                newName: "AcademicTitle");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AcademicTitleText",
                table: "Users",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.Sql("""
                UPDATE [Users]
                SET [AcademicTitleText] = CASE [AcademicTitle]
                    WHEN 1 THEN N'Prof. Dr.'
                    WHEN 2 THEN N'Doç. Dr.'
                    WHEN 3 THEN N'Dr. Öğr. Üyesi'
                    WHEN 4 THEN N'Öğr. Gör.'
                    WHEN 5 THEN N'Arş. Gör.'
                    WHEN 6 THEN N'Dr.'
                    ELSE NULL
                END
                """);

            migrationBuilder.DropColumn(
                name: "AcademicTitle",
                table: "Users");

            migrationBuilder.RenameColumn(
                name: "AcademicTitleText",
                table: "Users",
                newName: "AcademicTitle");
        }
    }
}
