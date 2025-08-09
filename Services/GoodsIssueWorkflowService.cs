using Microsoft.EntityFrameworkCore;
using WarehouseManage.Constants;
using WarehouseManage.Data;
using WarehouseManage.DTOs.GoodsIssue;
using WarehouseManage.Interfaces;
using WarehouseManage.Model;

namespace WarehouseManage.Services;

public class GoodsIssueWorkflowService : IGoodsIssueWorkflowService
{
    private readonly WarehouseDbContext _context;

    public GoodsIssueWorkflowService(WarehouseDbContext context)
    {
        _context = context;
    }

    public async Task<WorkflowStatusDto> GetWorkflowStatusAsync(int goodsIssueId, int currentUserId)
    {
        var goodsIssue = await _context.GoodsIssues
            .Include(gi => gi.ApprovedByUser)
            .Include(gi => gi.PreparedByUser)
            .Include(gi => gi.DeliveredByUser)
            .Include(gi => gi.CompletedByUser)
            .FirstOrDefaultAsync(gi => gi.GoodsIssueId == goodsIssueId);

        if (goodsIssue == null)
            throw new ArgumentException(GoodsIssueConstants.ErrorMessages.GoodsIssueNotFound);

        var currentUser = await _context.Users.FindAsync(currentUserId);
        if (currentUser == null)
            throw new ArgumentException("User not found");

        var availableActions = await GetAvailableActionsAsync(goodsIssueId, currentUserId, currentUser.Role!);

        return new WorkflowStatusDto
        {
            CurrentStatus = goodsIssue.Status!,
            AvailableActions = availableActions,
            CanEdit = CanEdit(goodsIssue.Status!, currentUser.Role!),
            CanApprove = CanUserApprove(currentUser.Role!) && goodsIssue.Status == GoodsIssueConstants.Status.AwaitingApproval,
            CanPrepare = CanUserPrepare(currentUser.Role!) && goodsIssue.Status == GoodsIssueConstants.Status.Approved,
            CanDeliver = CanUserDeliver(currentUser.Role!) && goodsIssue.Status == GoodsIssueConstants.Status.Preparing,
            CanComplete = CanUserComplete(currentUser.Role!) && goodsIssue.Status == GoodsIssueConstants.Status.Delivered,

            ApprovalInfo = goodsIssue.ApprovedByUserId.HasValue ? new ApprovalInfo
            {
                ApprovedByUserName = goodsIssue.ApprovedByUser?.FullName,
                ApprovedDate = goodsIssue.ApprovedDate,
                ApprovalNotes = goodsIssue.ApprovalNotes
            } : null,

            PreparationInfo = goodsIssue.PreparedByUserId.HasValue ? new PreparationInfo
            {
                PreparedByUserName = goodsIssue.PreparedByUser?.FullName,
                PreparedDate = goodsIssue.PreparedDate,
                PreparationNotes = goodsIssue.PreparationNotes
            } : null,

            DeliveryInfo = goodsIssue.DeliveredByUserId.HasValue ? new DeliveryInfo
            {
                DeliveredByUserName = goodsIssue.DeliveredByUser?.FullName,
                DeliveredDate = goodsIssue.DeliveredDate,
                DeliveryNotes = goodsIssue.DeliveryNotes,
                DeliveryAddress = goodsIssue.DeliveryAddress
            } : null,

            CompletionInfo = goodsIssue.CompletedByUserId.HasValue ? new CompletionInfo
            {
                CompletedByUserName = goodsIssue.CompletedByUser?.FullName,
                CompletedDate = goodsIssue.CompletedDate
            } : null
        };
    }

    public async Task<bool> ApproveOrRejectAsync(ApprovalDto approvalDto, int currentUserId)
    {
        var goodsIssue = await _context.GoodsIssues.FindAsync(approvalDto.GoodsIssueId);
        if (goodsIssue == null || goodsIssue.Status != GoodsIssueConstants.Status.AwaitingApproval)
            return false;

        var user = await _context.Users.FindAsync(currentUserId);
        if (user == null || !CanUserApprove(user.Role!))
            return false;

        if (approvalDto.Action == GoodsIssueConstants.WorkflowActions.Approve)
        {
            // Check inventory availability before approving
            if (!await CheckInventoryAvailabilityAsync(approvalDto.GoodsIssueId))
                return false;

            goodsIssue.Status = GoodsIssueConstants.Status.Approved;
            goodsIssue.ApprovedByUserId = currentUserId;
            goodsIssue.ApprovedDate = DateTime.UtcNow;
            goodsIssue.ApprovalNotes = approvalDto.Notes;
        }
        else if (approvalDto.Action == GoodsIssueConstants.WorkflowActions.Reject)
        {
            goodsIssue.Status = GoodsIssueConstants.Status.Rejected;
            goodsIssue.ApprovalNotes = approvalDto.Notes;
        }
        else
        {
            return false;
        }

        goodsIssue.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> StartPreparingAsync(int goodsIssueId, int currentUserId)
    {
        var goodsIssue = await _context.GoodsIssues
            .Include(gi => gi.GoodsIssueDetails)
            .FirstOrDefaultAsync(gi => gi.GoodsIssueId == goodsIssueId);
            
        if (goodsIssue == null || (goodsIssue.Status != GoodsIssueConstants.Status.Approved && goodsIssue.Status != "Approve"))
            return false;

        var user = await _context.Users.FindAsync(currentUserId);
        if (user == null || !CanUserPrepare(user.Role!))
            return false;

        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            // Update inventory - reduce stock when starting preparation
            foreach (var detail in goodsIssue.GoodsIssueDetails)
            {
                var inventory = await _context.Inventories
                    .FirstOrDefaultAsync(i => i.ProductId == detail.ProductId);

                if (inventory != null)
                {
                    // Check if we have enough stock
                    if (inventory.Quantity < detail.Quantity)
                    {
                        await transaction.RollbackAsync();
                        return false; // Insufficient stock
                    }

                    inventory.Quantity -= detail.Quantity;
                    inventory.LastUpdatedAt = DateTime.UtcNow;
                }
                else
                {
                    // No inventory record found - cannot start preparing
                    await transaction.RollbackAsync();
                    return false;
                }
            }

            // Update goods issue status
            goodsIssue.Status = GoodsIssueConstants.Status.Preparing;
            goodsIssue.PreparedByUserId = currentUserId;
            goodsIssue.PreparedDate = DateTime.UtcNow;
            goodsIssue.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();
            return true;
        }
        catch (Exception)
        {
            await transaction.RollbackAsync();
            return false;
        }
    }

    public async Task<bool> MarkDeliveredAsync(DeliveryDto dto, int currentUserId)
    {
        var goodsIssue = await _context.GoodsIssues.FindAsync(dto.GoodsIssueId);
        if (goodsIssue == null || goodsIssue.Status != GoodsIssueConstants.Status.Preparing)
            return false;

        var user = await _context.Users.FindAsync(currentUserId);
        if (user == null || !CanUserDeliver(user.Role!))
            return false;

        goodsIssue.Status = GoodsIssueConstants.Status.Delivered;
        goodsIssue.DeliveredByUserId = currentUserId;
        goodsIssue.DeliveredDate = DateTime.UtcNow;
        goodsIssue.DeliveryAddress = dto.DeliveryAddress;
        goodsIssue.DeliveryNotes = dto.Notes;
        goodsIssue.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> CompleteIssueAsync(CompleteIssueDto completeDto, int currentUserId)
    {
        var goodsIssue = await _context.GoodsIssues
            .FirstOrDefaultAsync(gi => gi.GoodsIssueId == completeDto.GoodsIssueId);

        if (goodsIssue == null || goodsIssue.Status != GoodsIssueConstants.Status.Delivered)
            return false;

        var user = await _context.Users.FindAsync(currentUserId);
        if (user == null || !CanUserComplete(user.Role!))
            return false;

        try
        {
            // Update goods issue status only - inventory was already updated during StartPreparingAsync
            goodsIssue.Status = GoodsIssueConstants.Status.Completed;
            goodsIssue.CompletedByUserId = currentUserId;
            goodsIssue.CompletedDate = DateTime.UtcNow;
            goodsIssue.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return true;
        }
        catch
        {
            return false;
        }
    }

    public async Task<bool> CancelIssueAsync(int goodsIssueId, int currentUserId)
    {
        var goodsIssue = await _context.GoodsIssues
            .Include(gi => gi.GoodsIssueDetails)
            .FirstOrDefaultAsync(gi => gi.GoodsIssueId == goodsIssueId);
            
        if (goodsIssue == null)
            return false;

        // Cannot cancel completed issues
        if (goodsIssue.Status == GoodsIssueConstants.Status.Completed)
            return false;

        var user = await _context.Users.FindAsync(currentUserId);
        if (user == null)
            return false;

        // Employee can only cancel their own issues in Draft status
        if (user.Role == RoleConstants.Employee)
        {
            if (goodsIssue.CreatedByUserId != currentUserId || goodsIssue.Status != GoodsIssueConstants.Status.Draft)
                return false;
        }
        // Manager/Admin can cancel issues in most statuses
        else if (!CanUserApprove(user.Role!))
        {
            return false;
        }

        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            // If the goods issue is in Preparing, Delivered status, we need to restore the inventory
            var shouldRestoreInventory = goodsIssue.Status == GoodsIssueConstants.Status.Preparing ||
                                       goodsIssue.Status == GoodsIssueConstants.Status.Delivered;

            if (shouldRestoreInventory)
            {
                // Restore inventory - add back the quantities
                foreach (var detail in goodsIssue.GoodsIssueDetails)
                {
                    var inventory = await _context.Inventories
                        .FirstOrDefaultAsync(i => i.ProductId == detail.ProductId);

                    if (inventory != null)
                    {
                        inventory.Quantity += detail.Quantity;
                        inventory.LastUpdatedAt = DateTime.UtcNow;
                    }
                    else
                    {
                        // Create inventory record if it doesn't exist
                        inventory = new Inventory
                        {
                            ProductId = detail.ProductId,
                            Quantity = detail.Quantity,
                            LastUpdatedAt = DateTime.UtcNow
                        };
                        _context.Inventories.Add(inventory);
                    }
                }
            }

            goodsIssue.Status = GoodsIssueConstants.Status.Cancelled;
            goodsIssue.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();
            return true;
        }
        catch (Exception)
        {
            await transaction.RollbackAsync();
            return false;
        }
    }

    public async Task<bool> ResubmitIssueAsync(int goodsIssueId, int currentUserId)
    {
        var goodsIssue = await _context.GoodsIssues.FindAsync(goodsIssueId);
        if (goodsIssue == null || goodsIssue.Status != GoodsIssueConstants.Status.Rejected)
            return false;

        // Only the creator can resubmit
        if (goodsIssue.CreatedByUserId != currentUserId)
            return false;

        goodsIssue.Status = GoodsIssueConstants.Status.AwaitingApproval;
        goodsIssue.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<List<string>> GetAvailableActionsAsync(int goodsIssueId, int currentUserId, string currentUserRole)
    {
        var goodsIssue = await _context.GoodsIssues.FindAsync(goodsIssueId);
        if (goodsIssue == null)
            return new List<string>();

        var actions = new List<string>();

        switch (goodsIssue.Status)
        {
            case GoodsIssueConstants.Status.Draft:
                actions.Add("Edit");
                actions.Add(GoodsIssueConstants.WorkflowActions.Submit);
                actions.Add("Delete");
                break;

            case GoodsIssueConstants.Status.AwaitingApproval:
                if (CanUserApprove(currentUserRole))
                {
                    actions.Add(GoodsIssueConstants.WorkflowActions.Approve);
                    actions.Add(GoodsIssueConstants.WorkflowActions.Reject);
                }
                if (goodsIssue.CreatedByUserId == currentUserId)
                {
                    actions.Add(GoodsIssueConstants.WorkflowActions.Cancel);
                }
                break;

            case GoodsIssueConstants.Status.Approved:
                if (CanUserPrepare(currentUserRole))
                {
                    actions.Add(GoodsIssueConstants.WorkflowActions.StartPreparing);
                }
                if (CanUserApprove(currentUserRole))
                {
                    actions.Add(GoodsIssueConstants.WorkflowActions.Cancel);
                }
                break;

            case GoodsIssueConstants.Status.Preparing:
                if (CanUserDeliver(currentUserRole))
                {
                    actions.Add(GoodsIssueConstants.WorkflowActions.MarkDelivered);
                }
                if (CanUserApprove(currentUserRole))
                {
                    actions.Add(GoodsIssueConstants.WorkflowActions.Cancel);
                }
                break;

            case GoodsIssueConstants.Status.Delivered:
                if (CanUserComplete(currentUserRole))
                {
                    actions.Add(GoodsIssueConstants.WorkflowActions.CompleteIssue);
                }
                if (CanUserApprove(currentUserRole))
                {
                    actions.Add(GoodsIssueConstants.WorkflowActions.Cancel);
                }
                break;

            case GoodsIssueConstants.Status.Rejected:
                if (goodsIssue.CreatedByUserId == currentUserId)
                {
                    actions.Add(GoodsIssueConstants.WorkflowActions.Resubmit);
                    actions.Add("Edit");
                }
                break;

            case GoodsIssueConstants.Status.Completed:
                actions.Add("View");
                actions.Add("Export");
                break;
        }

        return actions;
    }

    public async Task<bool> CheckInventoryAvailabilityAsync(int goodsIssueId)
    {
        var goodsIssue = await _context.GoodsIssues
            .Include(gi => gi.GoodsIssueDetails)
            .FirstOrDefaultAsync(gi => gi.GoodsIssueId == goodsIssueId);

        if (goodsIssue == null)
            return false;

        foreach (var detail in goodsIssue.GoodsIssueDetails)
        {
            var inventory = await _context.Inventories
                .FirstOrDefaultAsync(i => i.ProductId == detail.ProductId);

            if (inventory == null || inventory.Quantity < detail.Quantity)
                return false;
        }

        return true;
    }

    public bool CanUserApprove(string userRole)
    {
        return userRole == RoleConstants.Admin || userRole == RoleConstants.Manager;
    }

    public bool CanUserPrepare(string userRole)
    {
        return userRole == RoleConstants.Admin || 
               userRole == RoleConstants.Manager || 
               userRole == RoleConstants.Employee;
    }

    public bool CanUserDeliver(string userRole)
    {
        return userRole == RoleConstants.Admin || 
               userRole == RoleConstants.Manager || 
               userRole == RoleConstants.Employee;
    }

    public bool CanUserComplete(string userRole)
    {
        return userRole == RoleConstants.Admin || userRole == RoleConstants.Manager;
    }

    public string GetInitialStatusByRole(string userRole)
    {
        return userRole == RoleConstants.Employee 
            ? GoodsIssueConstants.Status.AwaitingApproval 
            : GoodsIssueConstants.Status.Approved;
    }

    private bool CanEdit(string status, string userRole)
    {
        return status == GoodsIssueConstants.Status.Draft || 
               (status == GoodsIssueConstants.Status.AwaitingApproval && CanUserApprove(userRole));
    }
}
