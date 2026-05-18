using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KcfMonitoringSystem.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddProductIdToProduction : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ProductId",
                table: "Productions",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Productions_ProductId",
                table: "Productions",
                column: "ProductId");

            migrationBuilder.AddForeignKey(
                name: "FK_Productions_Products_ProductId",
                table: "Productions",
                column: "ProductId",
                principalTable: "Products",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Productions_Products_ProductId",
                table: "Productions");

            migrationBuilder.DropIndex(
                name: "IX_Productions_ProductId",
                table: "Productions");

            migrationBuilder.DropColumn(
                name: "ProductId",
                table: "Productions");
        }
    }
}
