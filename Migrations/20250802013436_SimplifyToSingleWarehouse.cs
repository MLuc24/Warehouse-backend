using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WarehouseManage.Migrations
{
    /// <inheritdoc />
    public partial class SimplifyToSingleWarehouse : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_GoodsIssues_Warehouses",
                table: "GoodsIssues");

            migrationBuilder.DropForeignKey(
                name: "FK_GoodsReceipts_Warehouses",
                table: "GoodsReceipts");

            migrationBuilder.DropForeignKey(
                name: "FK_Inventory_Warehouses",
                table: "Inventory");

            migrationBuilder.DropTable(
                name: "Warehouses");

            migrationBuilder.DropPrimaryKey(
                name: "PK__Inventor__ED4863951E874FD8",
                table: "Inventory");

            migrationBuilder.Sql(@"
                IF EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Inventory_ProductId' AND object_id = OBJECT_ID('Inventory'))
                    DROP INDEX [IX_Inventory_ProductId] ON [Inventory];
            ");

            migrationBuilder.Sql(@"
                IF EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_GoodsReceipts_WarehouseId' AND object_id = OBJECT_ID('GoodsReceipts'))
                    DROP INDEX [IX_GoodsReceipts_WarehouseId] ON [GoodsReceipts];
            ");

            migrationBuilder.Sql(@"
                IF EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_GoodsIssues_WarehouseId' AND object_id = OBJECT_ID('GoodsIssues'))
                    DROP INDEX [IX_GoodsIssues_WarehouseId] ON [GoodsIssues];
            ");

            migrationBuilder.DropColumn(
                name: "WarehouseId",
                table: "Inventory");

            migrationBuilder.DropColumn(
                name: "WarehouseId",
                table: "GoodsReceipts");

            migrationBuilder.DropColumn(
                name: "WarehouseId",
                table: "GoodsIssues");

            migrationBuilder.AddPrimaryKey(
                name: "PK__Inventory__B40CC6CD86230D33",
                table: "Inventory",
                column: "ProductId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK__Inventory__B40CC6CD86230D33",
                table: "Inventory");

            migrationBuilder.AddColumn<int>(
                name: "WarehouseId",
                table: "Inventory",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "WarehouseId",
                table: "GoodsReceipts",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "WarehouseId",
                table: "GoodsIssues",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddPrimaryKey(
                name: "PK__Inventor__ED4863951E874FD8",
                table: "Inventory",
                columns: new[] { "WarehouseId", "ProductId" });

            migrationBuilder.CreateTable(
                name: "Warehouses",
                columns: table => new
                {
                    WarehouseId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Address = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ContactPhone = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime", nullable: true, defaultValueSql: "(getdate())"),
                    WarehouseName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Warehous__2608AFF9157E6E1B", x => x.WarehouseId);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Inventory_ProductId",
                table: "Inventory",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_GoodsReceipts_WarehouseId",
                table: "GoodsReceipts",
                column: "WarehouseId");

            migrationBuilder.CreateIndex(
                name: "IX_GoodsIssues_WarehouseId",
                table: "GoodsIssues",
                column: "WarehouseId");

            migrationBuilder.AddForeignKey(
                name: "FK_GoodsIssues_Warehouses",
                table: "GoodsIssues",
                column: "WarehouseId",
                principalTable: "Warehouses",
                principalColumn: "WarehouseId");

            migrationBuilder.AddForeignKey(
                name: "FK_GoodsReceipts_Warehouses",
                table: "GoodsReceipts",
                column: "WarehouseId",
                principalTable: "Warehouses",
                principalColumn: "WarehouseId");

            migrationBuilder.AddForeignKey(
                name: "FK_Inventory_Warehouses",
                table: "Inventory",
                column: "WarehouseId",
                principalTable: "Warehouses",
                principalColumn: "WarehouseId");
        }
    }
}
