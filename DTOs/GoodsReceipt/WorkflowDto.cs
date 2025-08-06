namespace WarehouseManage.DTOs.GoodsReceipt;

public class ApprovalDto
{
    public int GoodsReceiptId { get; set; }
    public string Action { get; set; } = null!; // "Approve" or "Reject"
    public string? Notes { get; set; }
}

public class SupplierConfirmationDto
{
    public string ConfirmationToken { get; set; } = null!;
    public bool Confirmed { get; set; }
    public string? Notes { get; set; }
}

public class CompleteReceiptDto
{
    public int GoodsReceiptId { get; set; }
    public string? Notes { get; set; }
}

public class WorkflowStatusDto
{
    public string CurrentStatus { get; set; } = null!;
    public List<string> AvailableActions { get; set; } = new();
    public bool CanEdit { get; set; }
    public bool CanApprove { get; set; }
    public bool CanComplete { get; set; }
    public bool RequiresSupplierConfirmation { get; set; }
    
    public ApprovalInfo? ApprovalInfo { get; set; }
    public SupplierConfirmationInfo? SupplierConfirmationInfo { get; set; }
    public CompletionInfo? CompletionInfo { get; set; }
}

public class ApprovalInfo
{
    public string? ApprovedByUserName { get; set; }
    public DateTime? ApprovedDate { get; set; }
    public string? ApprovalNotes { get; set; }
}

public class SupplierConfirmationInfo
{
    public bool? Confirmed { get; set; }
    public DateTime? ConfirmedDate { get; set; }
    public bool EmailSent { get; set; }
}

public class CompletionInfo
{
    public string? CompletedByUserName { get; set; }
    public DateTime? CompletedDate { get; set; }
}
