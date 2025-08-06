using WarehouseManage.DTOs.GoodsReceipt;

namespace WarehouseManage.Interfaces;

public interface IGoodsReceiptWorkflowService
{
    Task<WorkflowStatusDto> GetWorkflowStatusAsync(int goodsReceiptId, int currentUserId);
    Task<bool> ApproveOrRejectAsync(ApprovalDto approvalDto, int currentUserId);
    Task<bool> SendSupplierConfirmationEmailAsync(int goodsReceiptId);
    Task<bool> ConfirmBySupplierAsync(SupplierConfirmationDto confirmationDto);
    Task<bool> CompleteReceiptAsync(CompleteReceiptDto completeDto, int currentUserId);
    Task<List<string>> GetAvailableActionsAsync(int goodsReceiptId, int currentUserId, string currentUserRole);
    
    // Helper methods
    bool CanUserApprove(string userRole);
    bool CanUserComplete(string userRole);
    string GetInitialStatusByRole(string userRole);
}
