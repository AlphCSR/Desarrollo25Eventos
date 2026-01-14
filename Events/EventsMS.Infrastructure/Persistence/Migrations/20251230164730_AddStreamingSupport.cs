using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EventsMS.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddStreamingSupport : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "StreamingUrl",
                table: "Events",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Type",
                table: "Events",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "StreamingUrl",
                table: "Events");

            migrationBuilder.DropColumn(
                name: "Type",
                table: "Events");
        }
    }
}
