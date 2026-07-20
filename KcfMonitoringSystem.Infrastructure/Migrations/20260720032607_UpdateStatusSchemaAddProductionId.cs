using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KcfMonitoringSystem.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateStatusSchemaAddProductionId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Statuses_Products_ProductId",
                table: "Statuses");

            migrationBuilder.DropForeignKey(
                name: "FK_Statuses_Users_UserId",
                table: "Statuses");

            migrationBuilder.DropIndex(
                name: "IX_Statuses_ProductId",
                table: "Statuses");

            migrationBuilder.DropColumn(
                name: "ProductId",
                table: "Statuses");

            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "Statuses",
                newName: "ProductionId");

            migrationBuilder.RenameIndex(
                name: "IX_Statuses_UserId",
                table: "Statuses",
                newName: "IX_Statuses_ProductionId");

            migrationBuilder.AddForeignKey(
                name: "FK_Statuses_Productions_ProductionId",
                table: "Statuses",
                column: "ProductionId",
                principalTable: "Productions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Statuses_Productions_ProductionId",
                table: "Statuses");

            migrationBuilder.RenameColumn(
                name: "ProductionId",
                table: "Statuses",
                newName: "UserId");

            migrationBuilder.RenameIndex(
                name: "IX_Statuses_ProductionId",
                table: "Statuses",
                newName: "IX_Statuses_UserId");

            migrationBuilder.AddColumn<int>(
                name: "ProductId",
                table: "Statuses",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Statuses_ProductId",
                table: "Statuses",
                column: "ProductId");

            migrationBuilder.AddForeignKey(
                name: "FK_Statuses_Products_ProductId",
                table: "Statuses",
                column: "ProductId",
                principalTable: "Products",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Statuses_Users_UserId",
                table: "Statuses",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
