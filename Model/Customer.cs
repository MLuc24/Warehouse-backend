using System;
using System.Collections.Generic;

namespace WarehouseManage.Model;

public partial class Customer
{
    public int CustomerId { get; set; }

    public string CustomerName { get; set; } = null!;

    public string? Address { get; set; }

    public string? PhoneNumber { get; set; }

    public string? Email { get; set; }

    // 🧋 THÊM MỚI - Phân loại khách hàng đơn giản
    public string CustomerType { get; set; } = "Regular"; // Regular, VIP, Wholesale
    public string Status { get; set; } = "Active";        // Active, Inactive

    public DateTime? CreatedAt { get; set; }

    public virtual ICollection<GoodsIssue> GoodsIssues { get; set; } = new List<GoodsIssue>();
}
