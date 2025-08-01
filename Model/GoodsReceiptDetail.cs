﻿using System;
using System.Collections.Generic;

namespace WarehouseManage.Model;

public partial class GoodsReceiptDetail
{
    public int GoodsReceiptId { get; set; }

    public int ProductId { get; set; }

    public int Quantity { get; set; }

    public decimal UnitPrice { get; set; }

    public decimal? Subtotal { get; set; }

    public virtual GoodsReceipt GoodsReceipt { get; set; } = null!;

    public virtual Product Product { get; set; } = null!;
}
