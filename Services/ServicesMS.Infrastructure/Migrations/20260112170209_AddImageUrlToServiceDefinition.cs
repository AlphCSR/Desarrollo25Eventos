using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ServicesMS.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddImageUrlToServiceDefinition : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ImageUrl",
                table: "ServiceDefinitions",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ImageUrl",
                table: "ServiceDefinitions");
        }
    }
}
