namespace WarehouseManage.DTOs.GoodsIssue;

public class ApprovalDto
{
    public int GoodsIssueId { get; set; }
    public string Action { get; set; } = string.Empty; // "Approve" or "Reject"
    public string? Notes { get; set; }
}

public class PreparationDto
{
    public int GoodsIssueId { get; set; }
    public string Action { get; set; } = string.Empty; // "StartPreparing"
    public string? Notes { get; set; }
}

public class DeliveryDto
{
    public int GoodsIssueId { get; set; }
    public string? DeliveryAddress { get; set; }
    public string? Notes { get; set; }
}

public class CompleteIssueDto
{
    public int GoodsIssueId { get; set; }
    public string? Notes { get; set; }
}

public class WorkflowStatusDto
{
    public string CurrentStatus { get; set; } = string.Empty;
    public List<string> AvailableActions { get; set; } = new();
    public bool CanEdit { get; set; }
    public bool CanApprove { get; set; }
    public bool CanPrepare { get; set; }
    public bool CanDeliver { get; set; }
    public bool CanComplete { get; set; }
    
    public ApprovalInfo? ApprovalInfo { get; set; }
    public PreparationInfo? PreparationInfo { get; set; }
    public DeliveryInfo? DeliveryInfo { get; set; }
    public CompletionInfo? CompletionInfo { get; set; }
}

public class ApprovalInfo
{
    public string? ApprovedByUserName { get; set; }
    public DateTime? ApprovedDate { get; set; }
    public string? ApprovalNotes { get; set; }
}

public class PreparationInfo
{
    public string? PreparedByUserName { get; set; }
    public DateTime? PreparedDate { get; set; }
    public string? PreparationNotes { get; set; }
}

public class DeliveryInfo
{
    public string? DeliveredByUserName { get; set; }
    public DateTime? DeliveredDate { get; set; }
    public string? DeliveryNotes { get; set; }
    public string? DeliveryAddress { get; set; }
}

public class CompletionInfo
{
    public string? CompletedByUserName { get; set; }
    public DateTime? CompletedDate { get; set; }
}
