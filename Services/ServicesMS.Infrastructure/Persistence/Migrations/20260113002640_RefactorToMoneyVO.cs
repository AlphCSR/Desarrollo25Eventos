using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ServicesMS.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class RefactorToMoneyVO : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ImageUrl",
                table: "ServiceDefinitions");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ImageUrl",
                table: "ServiceDefinitions",
                type: "text",
                nullable: true);
        }
    }
}
