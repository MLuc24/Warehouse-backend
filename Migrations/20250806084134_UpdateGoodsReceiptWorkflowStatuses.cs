using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WarehouseManage.Migrations
{
    /// <inheritdoc />
    public partial class UpdateGoodsReceiptWorkflowStatuses : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ApprovalNotes",
                table: "GoodsReceipts",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ApprovedByUserId",
                table: "GoodsReceipts",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ApprovedDate",
                table: "GoodsReceipts",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CompletedByUserId",
                table: "GoodsReceipts",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CompletedDate",
                table: "GoodsReceipts",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SupplierConfirmationToken",
                table: "GoodsReceipts",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "SupplierConfirmed",
                table: "GoodsReceipts",
                type: "bit",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "SupplierConfirmedDate",
                table: "GoodsReceipts",
                type: "datetime2",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_GoodsReceipts_ApprovedByUserId",
                table: "GoodsReceipts",
                column: "ApprovedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_GoodsReceipts_CompletedByUserId",
                table: "GoodsReceipts",
                column: "CompletedByUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_GoodsReceipts_ApprovedByUsers",
                table: "GoodsReceipts",
                column: "ApprovedByUserId",
                principalTable: "Users",
                principalColumn: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_GoodsReceipts_CompletedByUsers",
                table: "GoodsReceipts",
                column: "CompletedByUserId",
                principalTable: "Users",
                principalColumn: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_GoodsReceipts_ApprovedByUsers",
                table: "GoodsReceipts");

            migrationBuilder.DropForeignKey(
                name: "FK_GoodsReceipts_CompletedByUsers",
                table: "GoodsReceipts");

            migrationBuilder.DropIndex(
                name: "IX_GoodsReceipts_ApprovedByUserId",
                table: "GoodsReceipts");

            migrationBuilder.DropIndex(
                name: "IX_GoodsReceipts_CompletedByUserId",
                table: "GoodsReceipts");

            migrationBuilder.DropColumn(
                name: "ApprovalNotes",
                table: "GoodsReceipts");

            migrationBuilder.DropColumn(
                name: "ApprovedByUserId",
                table: "GoodsReceipts");

            migrationBuilder.DropColumn(
                name: "ApprovedDate",
                table: "GoodsReceipts");

            migrationBuilder.DropColumn(
                name: "CompletedByUserId",
                table: "GoodsReceipts");

            migrationBuilder.DropColumn(
                name: "CompletedDate",
                table: "GoodsReceipts");

            migrationBuilder.DropColumn(
                name: "SupplierConfirmationToken",
                table: "GoodsReceipts");

            migrationBuilder.DropColumn(
                name: "SupplierConfirmed",
                table: "GoodsReceipts");

            migrationBuilder.DropColumn(
                name: "SupplierConfirmedDate",
                table: "GoodsReceipts");
        }
    }
}
