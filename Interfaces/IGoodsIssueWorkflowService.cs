using WarehouseManage.DTOs.GoodsIssue;

namespace WarehouseManage.Interfaces;

public interface IGoodsIssueWorkflowService
{
    Task<WorkflowStatusDto> GetWorkflowStatusAsync(int goodsIssueId, int currentUserId);
    
    // Core workflow actions
    Task<bool> ApproveOrRejectAsync(ApprovalDto approvalDto, int currentUserId);
    Task<bool> StartPreparingAsync(int goodsIssueId, int currentUserId);
    Task<bool> MarkDeliveredAsync(DeliveryDto dto, int currentUserId);
    Task<bool> CompleteIssueAsync(CompleteIssueDto completeDto, int currentUserId);
    
    // Common actions
    Task<bool> CancelIssueAsync(int goodsIssueId, int currentUserId);
    Task<bool> ResubmitIssueAsync(int goodsIssueId, int currentUserId);
    
    // Utility methods
    Task<List<string>> GetAvailableActionsAsync(int goodsIssueId, int currentUserId, string currentUserRole);
    Task<bool> CheckInventoryAvailabilityAsync(int goodsIssueId);
    
    // Helper methods
    bool CanUserApprove(string userRole); // Manager, Admin
    bool CanUserPrepare(string userRole); // Employee, Manager, Admin
    bool CanUserDeliver(string userRole); // Employee, Manager, Admin
    bool CanUserComplete(string userRole); // Manager, Admin
    string GetInitialStatusByRole(string userRole); // Employee: AwaitingApproval, Manager/Admin: Approved
}
