using System;
using System.Collections.Generic;

namespace WarehouseManage.Model;

public partial class Product
{
    public int ProductId { get; set; }

    public string Sku { get; set; } = null!;

    public string ProductName { get; set; } = null!;

    public string? Description { get; set; }

    public int? SupplierId { get; set; }

    public string? Unit { get; set; }

    public decimal? PurchasePrice { get; set; }

    public decimal? SellingPrice { get; set; }

    public string? ImageUrl { get; set; }

    // 🧋 THÊM MỚI cho TocoToco - Liên kết với Category model
    public int? CategoryId { get; set; }                  // Foreign key tới Category
    public DateTime? ExpiryDate { get; set; }             // Hạn sử dụng (QUAN TRỌNG cho F&B)
    public int? MinStockLevel { get; set; }               // Mức cảnh báo hết hàng  
    public int? MaxStockLevel { get; set; }               // Mức tồn kho tối đa
    public string? StorageType { get; set; }              // "Lạnh", "Khô", "Đông lạnh"
    public bool IsPerishable { get; set; } = false;      // Hàng dễ hỏng không?
    public DateTime? UpdatedAt { get; set; }              // Ngày cập nhật

    public bool? Status { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual ICollection<GoodsIssueDetail> GoodsIssueDetails { get; set; } = new List<GoodsIssueDetail>();

    public virtual ICollection<GoodsReceiptDetail> GoodsReceiptDetails { get; set; } = new List<GoodsReceiptDetail>();

    public virtual ICollection<Inventory> Inventories { get; set; } = new List<Inventory>();

    public virtual Supplier? Supplier { get; set; }

    // Navigation property cho Category
    public virtual Category? Category { get; set; }
}
