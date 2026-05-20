using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KcfMonitoringSystem.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddDurationToStatus : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Duration",
                table: "Statuses",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Duration",
                table: "Statuses");
        }
    }
}
