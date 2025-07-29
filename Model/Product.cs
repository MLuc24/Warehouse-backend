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

    public bool? Status { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual ICollection<GoodsIssueDetail> GoodsIssueDetails { get; set; } = new List<GoodsIssueDetail>();

    public virtual ICollection<GoodsReceiptDetail> GoodsReceiptDetails { get; set; } = new List<GoodsReceiptDetail>();

    public virtual ICollection<Inventory> Inventories { get; set; } = new List<Inventory>();

    public virtual Supplier? Supplier { get; set; }
}
