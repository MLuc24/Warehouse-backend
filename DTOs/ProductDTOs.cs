using System.ComponentModel.DataAnnotations;

namespace WarehouseManage.DTOs
{
    public class ProductDto
    {
        public int ProductId { get; set; }
        public string Sku { get; set; } = string.Empty;
        public string ProductName { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int? SupplierId { get; set; }
        public string? SupplierName { get; set; }
        public string? Unit { get; set; }
        public decimal? PurchasePrice { get; set; }
        public decimal? SellingPrice { get; set; }
        public string? ImageUrl { get; set; }
        public bool? Status { get; set; }
        public DateTime? CreatedAt { get; set; }
        public int? StockQuantity { get; set; } // Từ Inventory
    }

    public class CreateProductDto
    {
        [Required]
        public string Sku { get; set; } = string.Empty;

        [Required]
        public string ProductName { get; set; } = string.Empty;

        public string? Description { get; set; }

        public int? SupplierId { get; set; }

        public string? Unit { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Purchase price must be greater than or equal to 0")]
        public decimal? PurchasePrice { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Selling price must be greater than or equal to 0")]
        public decimal? SellingPrice { get; set; }

        public string? ImageUrl { get; set; }
    }

    public class UpdateProductDto
    {
        public string? ProductName { get; set; }
        public string? Description { get; set; }
        public int? SupplierId { get; set; }
        public string? Unit { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Purchase price must be greater than or equal to 0")]
        public decimal? PurchasePrice { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Selling price must be greater than or equal to 0")]
        public decimal? SellingPrice { get; set; }

        public string? ImageUrl { get; set; }
        public bool? Status { get; set; }
    }
}
