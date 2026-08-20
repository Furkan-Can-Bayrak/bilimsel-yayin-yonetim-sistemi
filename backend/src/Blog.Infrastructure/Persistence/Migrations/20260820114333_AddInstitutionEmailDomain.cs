using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Blog.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddInstitutionEmailDomain : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "EmailDomain",
                table: "Institutions",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EmailDomain",
                table: "Institutions");
        }
    }
}
