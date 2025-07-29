using System;
using System.Collections.Generic;

namespace WarehouseManage.Model;

public partial class GoodsReceipt
{
    public int GoodsReceiptId { get; set; }

    public int SupplierId { get; set; }

    public int WarehouseId { get; set; }

    public int CreatedByUserId { get; set; }

    public DateTime? ReceiptDate { get; set; }

    public decimal? TotalAmount { get; set; }

    public string? Notes { get; set; }

    public string? Status { get; set; }

    public virtual User CreatedByUser { get; set; } = null!;

    public virtual ICollection<GoodsReceiptDetail> GoodsReceiptDetails { get; set; } = new List<GoodsReceiptDetail>();

    public virtual Supplier Supplier { get; set; } = null!;

    public virtual Warehouse Warehouse { get; set; } = null!;
}
