using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WarehouseManage.Migrations
{
    /// <inheritdoc />
    public partial class AddGoodsIssueWorkflowFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_GoodsIssues_Customers",
                table: "GoodsIssues");

            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "GoodsIssues",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true,
                defaultValue: "Draft",
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50,
                oldNullable: true,
                oldDefaultValue: "New");

            migrationBuilder.AlterColumn<int>(
                name: "CustomerId",
                table: "GoodsIssues",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<string>(
                name: "ApprovalNotes",
                table: "GoodsIssues",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ApprovedByUserId",
                table: "GoodsIssues",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ApprovedDate",
                table: "GoodsIssues",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CompletedByUserId",
                table: "GoodsIssues",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CompletedDate",
                table: "GoodsIssues",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "GoodsIssues",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "(getutcdate())");

            migrationBuilder.AddColumn<int>(
                name: "DeliveredByUserId",
                table: "GoodsIssues",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeliveredDate",
                table: "GoodsIssues",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DeliveryAddress",
                table: "GoodsIssues",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DeliveryNotes",
                table: "GoodsIssues",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PreparationNotes",
                table: "GoodsIssues",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PreparedByUserId",
                table: "GoodsIssues",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "PreparedDate",
                table: "GoodsIssues",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "RequestedDeliveryDate",
                table: "GoodsIssues",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "GoodsIssues",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "(getutcdate())");

            migrationBuilder.CreateIndex(
                name: "IX_GoodsIssues_ApprovedByUserId",
                table: "GoodsIssues",
                column: "ApprovedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_GoodsIssues_CompletedByUserId",
                table: "GoodsIssues",
                column: "CompletedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_GoodsIssues_DeliveredByUserId",
                table: "GoodsIssues",
                column: "DeliveredByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_GoodsIssues_PreparedByUserId",
                table: "GoodsIssues",
                column: "PreparedByUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_GoodsIssues_ApprovedByUser",
                table: "GoodsIssues",
                column: "ApprovedByUserId",
                principalTable: "Users",
                principalColumn: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_GoodsIssues_CompletedByUser",
                table: "GoodsIssues",
                column: "CompletedByUserId",
                principalTable: "Users",
                principalColumn: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_GoodsIssues_Customers",
                table: "GoodsIssues",
                column: "CustomerId",
                principalTable: "Customers",
                principalColumn: "CustomerId",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_GoodsIssues_DeliveredByUser",
                table: "GoodsIssues",
                column: "DeliveredByUserId",
                principalTable: "Users",
                principalColumn: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_GoodsIssues_PreparedByUser",
                table: "GoodsIssues",
                column: "PreparedByUserId",
                principalTable: "Users",
                principalColumn: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_GoodsIssues_ApprovedByUser",
                table: "GoodsIssues");

            migrationBuilder.DropForeignKey(
                name: "FK_GoodsIssues_CompletedByUser",
                table: "GoodsIssues");

            migrationBuilder.DropForeignKey(
                name: "FK_GoodsIssues_Customers",
                table: "GoodsIssues");

            migrationBuilder.DropForeignKey(
                name: "FK_GoodsIssues_DeliveredByUser",
                table: "GoodsIssues");

            migrationBuilder.DropForeignKey(
                name: "FK_GoodsIssues_PreparedByUser",
                table: "GoodsIssues");

            migrationBuilder.DropIndex(
                name: "IX_GoodsIssues_ApprovedByUserId",
                table: "GoodsIssues");

            migrationBuilder.DropIndex(
                name: "IX_GoodsIssues_CompletedByUserId",
                table: "GoodsIssues");

            migrationBuilder.DropIndex(
                name: "IX_GoodsIssues_DeliveredByUserId",
                table: "GoodsIssues");

            migrationBuilder.DropIndex(
                name: "IX_GoodsIssues_PreparedByUserId",
                table: "GoodsIssues");

            migrationBuilder.DropColumn(
                name: "ApprovalNotes",
                table: "GoodsIssues");

            migrationBuilder.DropColumn(
                name: "ApprovedByUserId",
                table: "GoodsIssues");

            migrationBuilder.DropColumn(
                name: "ApprovedDate",
                table: "GoodsIssues");

            migrationBuilder.DropColumn(
                name: "CompletedByUserId",
                table: "GoodsIssues");

            migrationBuilder.DropColumn(
                name: "CompletedDate",
                table: "GoodsIssues");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "GoodsIssues");

            migrationBuilder.DropColumn(
                name: "DeliveredByUserId",
                table: "GoodsIssues");

            migrationBuilder.DropColumn(
                name: "DeliveredDate",
                table: "GoodsIssues");

            migrationBuilder.DropColumn(
                name: "DeliveryAddress",
                table: "GoodsIssues");

            migrationBuilder.DropColumn(
                name: "DeliveryNotes",
                table: "GoodsIssues");

            migrationBuilder.DropColumn(
                name: "PreparationNotes",
                table: "GoodsIssues");

            migrationBuilder.DropColumn(
                name: "PreparedByUserId",
                table: "GoodsIssues");

            migrationBuilder.DropColumn(
                name: "PreparedDate",
                table: "GoodsIssues");

            migrationBuilder.DropColumn(
                name: "RequestedDeliveryDate",
                table: "GoodsIssues");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "GoodsIssues");

            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "GoodsIssues",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true,
                defaultValue: "New",
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50,
                oldNullable: true,
                oldDefaultValue: "Draft");

            migrationBuilder.AlterColumn<int>(
                name: "CustomerId",
                table: "GoodsIssues",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_GoodsIssues_Customers",
                table: "GoodsIssues",
                column: "CustomerId",
                principalTable: "Customers",
                principalColumn: "CustomerId");
        }
    }
}
