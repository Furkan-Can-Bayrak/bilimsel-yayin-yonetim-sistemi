using Blog.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Blog.Infrastructure.Persistence.Migrations
{
    [DbContext(typeof(BlogDbContext))]
    [Migration("20260820143500_MakeManuscriptResearchAreaOptional")]
    public partial class MakeManuscriptResearchAreaOptional : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "ResearchAreaId",
                table: "Manuscripts",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "ResearchAreaId",
                table: "Manuscripts",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);
        }
    }
}
