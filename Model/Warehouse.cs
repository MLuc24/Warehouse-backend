using System;
using System.Collections.Generic;

namespace WarehouseManage.Model;

public partial class Warehouse
{
    public int WarehouseId { get; set; }

    public string WarehouseName { get; set; } = null!;

    public string Address { get; set; } = null!;

    public string? ContactPhone { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual ICollection<GoodsIssue> GoodsIssues { get; set; } = new List<GoodsIssue>();

    public virtual ICollection<GoodsReceipt> GoodsReceipts { get; set; } = new List<GoodsReceipt>();

    public virtual ICollection<Inventory> Inventories { get; set; } = new List<Inventory>();
}
