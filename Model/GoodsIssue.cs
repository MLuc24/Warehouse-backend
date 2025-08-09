using System;
using System.Collections.Generic;

namespace WarehouseManage.Model;

public partial class GoodsIssue
{
    public int GoodsIssueId { get; set; }
    public string IssueNumber { get; set; } = null!;
    public int? CustomerId { get; set; }
    public int CreatedByUserId { get; set; }
    public DateTime? IssueDate { get; set; }
    public DateTime? RequestedDeliveryDate { get; set; }
    public decimal? TotalAmount { get; set; }
    public string? Notes { get; set; }
    public string? Status { get; set; }

    // Workflow tracking fields
    public int? ApprovedByUserId { get; set; }
    public DateTime? ApprovedDate { get; set; }
    public string? ApprovalNotes { get; set; }
    
    // Preparation tracking
    public int? PreparedByUserId { get; set; }
    public DateTime? PreparedDate { get; set; }
    public string? PreparationNotes { get; set; }
    
    // Delivery tracking  
    public int? DeliveredByUserId { get; set; }
    public DateTime? DeliveredDate { get; set; }
    public string? DeliveryNotes { get; set; }
    public string? DeliveryAddress { get; set; }
    
    // Completion
    public int? CompletedByUserId { get; set; }
    public DateTime? CompletedDate { get; set; }

    // Timestamps
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    // Navigation properties
    public virtual User CreatedByUser { get; set; } = null!;
    public virtual User? ApprovedByUser { get; set; }
    public virtual User? PreparedByUser { get; set; }
    public virtual User? DeliveredByUser { get; set; }
    public virtual User? CompletedByUser { get; set; }
    public virtual Customer? Customer { get; set; }
    public virtual ICollection<GoodsIssueDetail> GoodsIssueDetails { get; set; } = new List<GoodsIssueDetail>();
}
