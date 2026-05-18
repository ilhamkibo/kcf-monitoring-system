using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace KcfMonitoringSystem.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddProductTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Productions_Machines_MachineId",
                table: "Productions");

            migrationBuilder.DropForeignKey(
                name: "FK_Productions_Users_UserId",
                table: "Productions");

            migrationBuilder.CreateTable(
                name: "Products",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ProductNo = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    PartName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    PartNo = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Products", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Products_ProductNo",
                table: "Products",
                column: "ProductNo",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Productions_Machines_MachineId",
                table: "Productions",
                column: "MachineId",
                principalTable: "Machines",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Productions_Users_UserId",
                table: "Productions",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Productions_Machines_MachineId",
                table: "Productions");

            migrationBuilder.DropForeignKey(
                name: "FK_Productions_Users_UserId",
                table: "Productions");

            migrationBuilder.DropTable(
                name: "Products");

            migrationBuilder.AddForeignKey(
                name: "FK_Productions_Machines_MachineId",
                table: "Productions",
                column: "MachineId",
                principalTable: "Machines",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Productions_Users_UserId",
                table: "Productions",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
