using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UsersMS.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddFriendlyMessageToUserHistory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "FriendlyMessage",
                table: "UserHistories",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FriendlyMessage",
                table: "UserHistories");
        }
    }
}
