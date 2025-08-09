using System.ComponentModel.DataAnnotations;

namespace WarehouseManage.DTOs.GoodsIssue;

public class GoodsIssueDto
{
    public int GoodsIssueId { get; set; }
    public string IssueNumber { get; set; } = null!;
    public int? CustomerId { get; set; }
    public string? CustomerName { get; set; }
    public int CreatedByUserId { get; set; }
    public string CreatedByUserName { get; set; } = null!;
    public DateTime? IssueDate { get; set; }
    public DateTime? RequestedDeliveryDate { get; set; }
    public decimal? TotalAmount { get; set; }
    public string? Notes { get; set; }
    public string? Status { get; set; }
    
    // Workflow info
    public string? ApprovedByUserName { get; set; }
    public DateTime? ApprovedDate { get; set; }
    public string? ApprovalNotes { get; set; }
    
    public string? PreparedByUserName { get; set; }
    public DateTime? PreparedDate { get; set; }
    public string? PreparationNotes { get; set; }
    
    public string? DeliveredByUserName { get; set; }
    public DateTime? DeliveredDate { get; set; }
    public string? DeliveryNotes { get; set; }
    public string? DeliveryAddress { get; set; }
    
    public string? CompletedByUserName { get; set; }
    public DateTime? CompletedDate { get; set; }
    
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    
    public List<GoodsIssueDetailDto> Details { get; set; } = new();
}

public class GoodsIssueDetailDto
{
    public int ProductId { get; set; }
    public string ProductName { get; set; } = null!;
    public string? ProductSku { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal? Subtotal { get; set; }
    public string? Unit { get; set; }
    public string? ImageUrl { get; set; }
}
