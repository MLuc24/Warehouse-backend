using System;
using System.Collections.Generic;

namespace WarehouseManage.Model;

public partial class Inventory
{
    public int ProductId { get; set; }

    public int Quantity { get; set; }

    public DateTime? LastUpdatedAt { get; set; }

    public virtual Product Product { get; set; } = null!;
}
