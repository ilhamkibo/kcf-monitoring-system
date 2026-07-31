using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KcfMonitoringSystem.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateAlarmHistoryStatus : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Status",
                table: "AlarmHistories");

            migrationBuilder.AddColumn<string>(
                name: "AlarmState",
                table: "AlarmHistories",
                type: "varchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "StatusId",
                table: "AlarmHistories",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_AlarmHistories_StatusId",
                table: "AlarmHistories",
                column: "StatusId");

            migrationBuilder.AddForeignKey(
                name: "FK_AlarmHistories_Statuses_StatusId",
                table: "AlarmHistories",
                column: "StatusId",
                principalTable: "Statuses",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AlarmHistories_Statuses_StatusId",
                table: "AlarmHistories");

            migrationBuilder.DropIndex(
                name: "IX_AlarmHistories_StatusId",
                table: "AlarmHistories");

            migrationBuilder.DropColumn(
                name: "AlarmState",
                table: "AlarmHistories");

            migrationBuilder.DropColumn(
                name: "StatusId",
                table: "AlarmHistories");

            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: "AlarmHistories",
                type: "longtext",
                nullable: false);
        }
    }
}
