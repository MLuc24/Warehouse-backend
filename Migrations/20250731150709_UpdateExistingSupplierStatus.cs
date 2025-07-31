using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WarehouseManage.Migrations
{
    /// <inheritdoc />
    public partial class UpdateExistingSupplierStatus : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Update existing suppliers to have Active status
            migrationBuilder.Sql("UPDATE [Suppliers] SET [Status] = 'Active' WHERE [Status] = '' OR [Status] IS NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
