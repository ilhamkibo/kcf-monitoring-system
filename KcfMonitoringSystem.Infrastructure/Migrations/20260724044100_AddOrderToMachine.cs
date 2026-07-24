using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KcfMonitoringSystem.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddOrderToMachine : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Order",
                table: "Machines",
                type: "int",
                nullable: false,
                defaultValue: 0);

            // Update existing machines with correct order based on appsettings.json MachineOrder
            migrationBuilder.Sql("UPDATE `Machines` SET `Order` = 1 WHERE `Name` = 'SF-200-1'");
            migrationBuilder.Sql("UPDATE `Machines` SET `Order` = 2 WHERE `Name` = 'SF-200-2'");
            migrationBuilder.Sql("UPDATE `Machines` SET `Order` = 3 WHERE `Name` = 'SF-100-1'");
            migrationBuilder.Sql("UPDATE `Machines` SET `Order` = 4 WHERE `Name` = 'SF-100-2'");
            migrationBuilder.Sql("UPDATE `Machines` SET `Order` = 5 WHERE `Name` = 'SF-80'");
            migrationBuilder.Sql("UPDATE `Machines` SET `Order` = 6 WHERE `Name` = 'SF-50-1'");
            migrationBuilder.Sql("UPDATE `Machines` SET `Order` = 7 WHERE `Name` = 'SF-50-2'");
            migrationBuilder.Sql("UPDATE `Machines` SET `Order` = 8 WHERE `Name` = 'SF-50-3'");
            migrationBuilder.Sql("UPDATE `Machines` SET `Order` = 9 WHERE `Name` NOT IN ('SF-200-1','SF-200-2','SF-100-1','SF-100-2','SF-80','SF-50-1','SF-50-2','SF-50-3')");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Order",
                table: "Machines");
        }
    }
}
