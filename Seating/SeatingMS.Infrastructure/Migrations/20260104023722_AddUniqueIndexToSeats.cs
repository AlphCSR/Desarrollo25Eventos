using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SeatingMS.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddUniqueIndexToSeats : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_EventSeats_EventId_SectionId",
                table: "EventSeats");

            migrationBuilder.CreateIndex(
                name: "IX_EventSeats_EventId_SectionId_Row_Number",
                table: "EventSeats",
                columns: new[] { "EventId", "SectionId", "Row", "Number" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_EventSeats_EventId_SectionId_Row_Number",
                table: "EventSeats");

            migrationBuilder.CreateIndex(
                name: "IX_EventSeats_EventId_SectionId",
                table: "EventSeats",
                columns: new[] { "EventId", "SectionId" });
        }
    }
}
