using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ReportsMS.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddDashboardMetrics : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DashboardMetrics",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    MetricName = table.Column<string>(type: "text", nullable: false),
                    Value = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    Timestamp = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Dimensions = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DashboardMetrics", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DashboardMetrics_MetricName",
                table: "DashboardMetrics",
                column: "MetricName",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DashboardMetrics");
        }
    }
}
