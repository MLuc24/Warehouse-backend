using System;
using System.Collections.Generic;

namespace WarehouseManage.Model;

public partial class GoodsIssue
{
    public int GoodsIssueId { get; set; }

    // 🧋 THÊM MỚI - Số phiếu xuất cho dễ tracking
    public string IssueNumber { get; set; } = null!;     // VD: XK20250803001

    public int CustomerId { get; set; }

    public int CreatedByUserId { get; set; }

    public DateTime? IssueDate { get; set; }

    public decimal? TotalAmount { get; set; }

    public string? Notes { get; set; }

    public string? Status { get; set; }

    public virtual User CreatedByUser { get; set; } = null!;

    public virtual Customer Customer { get; set; } = null!;

    public virtual ICollection<GoodsIssueDetail> GoodsIssueDetails { get; set; } = new List<GoodsIssueDetail>();
}
