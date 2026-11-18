using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KcfMonitoringSystem.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AllowMultipleMaintenancesPerStatus : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Maintenances_Statuses_StatusId",
                table: "Maintenances");

            migrationBuilder.DropIndex(
                name: "IX_Maintenances_StatusId",
                table: "Maintenances");

            migrationBuilder.CreateIndex(
                name: "IX_Maintenances_StatusId",
                table: "Maintenances",
                column: "StatusId");

            migrationBuilder.AddForeignKey(
                name: "FK_Maintenances_Statuses_StatusId",
                table: "Maintenances",
                column: "StatusId",
                principalTable: "Statuses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Maintenances_Statuses_StatusId",
                table: "Maintenances");

            migrationBuilder.DropIndex(
                name: "IX_Maintenances_StatusId",
                table: "Maintenances");

            migrationBuilder.CreateIndex(
                name: "IX_Maintenances_StatusId",
                table: "Maintenances",
                column: "StatusId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Maintenances_Statuses_StatusId",
                table: "Maintenances",
                column: "StatusId",
                principalTable: "Statuses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
