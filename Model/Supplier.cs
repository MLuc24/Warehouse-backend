using System;
using System.Collections.Generic;

namespace WarehouseManage.Model;

public partial class Supplier
{
    public int SupplierId { get; set; }

    public string SupplierName { get; set; } = null!;

    public string? Address { get; set; }

    public string? PhoneNumber { get; set; }

    public string? Email { get; set; }

    public string? TaxCode { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual ICollection<GoodsReceipt> GoodsReceipts { get; set; } = new List<GoodsReceipt>();

    public virtual ICollection<Product> Products { get; set; } = new List<Product>();
}
