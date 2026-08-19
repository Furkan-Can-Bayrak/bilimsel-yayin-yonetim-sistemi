using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Blog.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddInstitutions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Institutions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Abbreviation = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    DeletedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Institutions", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Institutions_Name",
                table: "Institutions",
                column: "Name",
                unique: true,
                filter: "[DeletedAtUtc] IS NULL");

            migrationBuilder.AddColumn<int>(
                name: "InstitutionId",
                table: "Users",
                type: "int",
                nullable: true);

            migrationBuilder.Sql("""
                INSERT INTO [Institutions] ([Name])
                SELECT DISTINCT LTRIM(RTRIM([Affiliation]))
                FROM [Users]
                WHERE [Affiliation] IS NOT NULL
                  AND LTRIM(RTRIM([Affiliation])) <> N''
                  AND NOT EXISTS (
                      SELECT 1
                      FROM [Institutions] AS [i]
                      WHERE [i].[Name] = LTRIM(RTRIM([Users].[Affiliation]))
                        AND [i].[DeletedAtUtc] IS NULL);

                UPDATE [u]
                SET [u].[InstitutionId] = [i].[Id]
                FROM [Users] AS [u]
                INNER JOIN [Institutions] AS [i]
                    ON [i].[Name] = LTRIM(RTRIM([u].[Affiliation]))
                   AND [i].[DeletedAtUtc] IS NULL;
                """);

            migrationBuilder.DropColumn(
                name: "Affiliation",
                table: "Users");

            migrationBuilder.CreateIndex(
                name: "IX_Users_InstitutionId",
                table: "Users",
                column: "InstitutionId");

            migrationBuilder.AddForeignKey(
                name: "FK_Users_Institutions_InstitutionId",
                table: "Users",
                column: "InstitutionId",
                principalTable: "Institutions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Affiliation",
                table: "Users",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.Sql("""
                UPDATE [u]
                SET [u].[Affiliation] = [i].[Name]
                FROM [Users] AS [u]
                INNER JOIN [Institutions] AS [i] ON [i].[Id] = [u].[InstitutionId];
                """);

            migrationBuilder.DropForeignKey(
                name: "FK_Users_Institutions_InstitutionId",
                table: "Users");

            migrationBuilder.DropTable(
                name: "Institutions");

            migrationBuilder.DropIndex(
                name: "IX_Users_InstitutionId",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "InstitutionId",
                table: "Users");
        }
    }
}
