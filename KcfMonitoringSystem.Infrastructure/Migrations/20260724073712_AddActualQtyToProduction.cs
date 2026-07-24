using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KcfMonitoringSystem.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddActualQtyToProduction : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ActualQty",
                table: "Productions",
                type: "int",
                nullable: false,
                defaultValue: 0);

            // Set ActualQty = Quantity for existing data
            migrationBuilder.Sql("UPDATE `Productions` SET `ActualQty` = `Quantity`");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ActualQty",
                table: "Productions");
        }
    }
}
