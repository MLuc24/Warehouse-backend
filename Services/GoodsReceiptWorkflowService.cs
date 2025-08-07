using Microsoft.EntityFrameworkCore;
using WarehouseManage.Constants;
using WarehouseManage.Data;
using WarehouseManage.DTOs.GoodsReceipt;
using WarehouseManage.Interfaces;
using WarehouseManage.Model;
using WarehouseManage.Services;

namespace WarehouseManage.Services;

public class GoodsReceiptWorkflowService : IGoodsReceiptWorkflowService
{
    private readonly WarehouseDbContext _context;
    private readonly ILogger<GoodsReceiptWorkflowService> _logger;
    private readonly INotificationService _notificationService;
    private readonly IConfiguration _configuration;
    private readonly IPdfService _pdfService;

    public GoodsReceiptWorkflowService(
        WarehouseDbContext context, 
        ILogger<GoodsReceiptWorkflowService> logger,
        INotificationService notificationService,
        IConfiguration configuration,
        IPdfService pdfService)
    {
        _context = context;
        _logger = logger;
        _notificationService = notificationService;
        _configuration = configuration;
        _pdfService = pdfService;
    }

    public async Task<WorkflowStatusDto> GetWorkflowStatusAsync(int goodsReceiptId, int currentUserId)
    {
        var goodsReceipt = await _context.GoodsReceipts
            .Include(gr => gr.ApprovedByUser)
            .Include(gr => gr.CompletedByUser)
            .FirstOrDefaultAsync(gr => gr.GoodsReceiptId == goodsReceiptId);

        if (goodsReceipt == null)
            throw new ArgumentException("Goods receipt not found");

        var currentUser = await _context.Users.FindAsync(currentUserId);
        if (currentUser == null)
            throw new ArgumentException("User not found");

        var availableActions = await GetAvailableActionsAsync(goodsReceiptId, currentUserId, currentUser.Role!);

        return new WorkflowStatusDto
        {
            CurrentStatus = goodsReceipt.Status!,
            AvailableActions = availableActions,
            CanEdit = CanEdit(goodsReceipt.Status!, currentUser.Role!),
            CanApprove = CanUserApprove(currentUser.Role!) && goodsReceipt.Status == GoodsReceiptConstants.Status.AwaitingApproval,
            CanComplete = CanUserComplete(currentUser.Role!) && goodsReceipt.Status == GoodsReceiptConstants.Status.SupplierConfirmed,
            RequiresSupplierConfirmation = goodsReceipt.Status == GoodsReceiptConstants.Status.Pending,
            
            ApprovalInfo = goodsReceipt.ApprovedByUserId.HasValue ? new ApprovalInfo
            {
                ApprovedByUserName = goodsReceipt.ApprovedByUser?.FullName,
                ApprovedDate = goodsReceipt.ApprovedDate,
                ApprovalNotes = goodsReceipt.ApprovalNotes
            } : null,
            
            SupplierConfirmationInfo = new SupplierConfirmationInfo
            {
                Confirmed = goodsReceipt.SupplierConfirmed,
                ConfirmedDate = goodsReceipt.SupplierConfirmedDate,
                EmailSent = !string.IsNullOrEmpty(goodsReceipt.SupplierConfirmationToken)
            },
            
            CompletionInfo = goodsReceipt.CompletedByUserId.HasValue ? new CompletionInfo
            {
                CompletedByUserName = goodsReceipt.CompletedByUser?.FullName,
                CompletedDate = goodsReceipt.CompletedDate
            } : null
        };
    }

    public async Task<bool> ApproveOrRejectAsync(ApprovalDto approvalDto, int currentUserId)
    {
        var currentUser = await _context.Users.FindAsync(currentUserId);
        if (currentUser == null || !CanUserApprove(currentUser.Role!))
            return false;

        var goodsReceipt = await _context.GoodsReceipts.FindAsync(approvalDto.GoodsReceiptId);
        if (goodsReceipt == null || goodsReceipt.Status != GoodsReceiptConstants.Status.AwaitingApproval)
            return false;

        goodsReceipt.ApprovedByUserId = currentUserId;
        goodsReceipt.ApprovedDate = DateTime.UtcNow;
        goodsReceipt.ApprovalNotes = approvalDto.Notes;

        if (approvalDto.Action == GoodsReceiptConstants.WorkflowActions.Approve)
        {
            goodsReceipt.Status = GoodsReceiptConstants.Status.Pending;
            
            // Send email to supplier
            await SendSupplierConfirmationEmailAsync(approvalDto.GoodsReceiptId);
        }
        else if (approvalDto.Action == GoodsReceiptConstants.WorkflowActions.Reject)
        {
            goodsReceipt.Status = GoodsReceiptConstants.Status.Rejected;
        }

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> SendSupplierConfirmationEmailAsync(int goodsReceiptId)
    {
        var goodsReceipt = await _context.GoodsReceipts
            .Include(gr => gr.Supplier)
            .Include(gr => gr.CreatedByUser)
            .Include(gr => gr.GoodsReceiptDetails)
                .ThenInclude(grd => grd.Product)
            .FirstOrDefaultAsync(gr => gr.GoodsReceiptId == goodsReceiptId);

        if (goodsReceipt == null || goodsReceipt.Supplier == null || string.IsNullOrEmpty(goodsReceipt.Supplier.Email))
            return false;

        // Create GoodsReceiptDto for PDF generation
        var goodsReceiptDto = new GoodsReceiptDto
        {
            GoodsReceiptId = goodsReceipt.GoodsReceiptId,
            ReceiptNumber = goodsReceipt.ReceiptNumber,
            SupplierId = goodsReceipt.SupplierId,
            SupplierName = goodsReceipt.Supplier?.SupplierName ?? "",
            ReceiptDate = goodsReceipt.ReceiptDate,
            TotalAmount = goodsReceipt.TotalAmount,
            Notes = goodsReceipt.Notes,
            Status = goodsReceipt.Status,
            CreatedByUserName = goodsReceipt.CreatedByUser?.FullName ?? "",
            Details = goodsReceipt.GoodsReceiptDetails?.Select(d => new GoodsReceiptDetailDto
            {
                ProductId = d.ProductId,
                ProductName = d.Product?.ProductName ?? "",
                ProductSku = d.Product?.Sku ?? "",
                Quantity = d.Quantity,
                UnitPrice = d.UnitPrice,
                Subtotal = d.Subtotal,
                Unit = d.Product?.Unit ?? ""
            }).ToList() ?? new List<GoodsReceiptDetailDto>()
        };

        try
        {
            // Generate PDF
            var pdfBytes = await _pdfService.GenerateGoodsReceiptPdfAsync(goodsReceiptDto);

            // Generate confirmation token if not exists
            if (string.IsNullOrEmpty(goodsReceipt.SupplierConfirmationToken))
            {
                goodsReceipt.SupplierConfirmationToken = Guid.NewGuid().ToString();
                await _context.SaveChangesAsync();
            }

            // Build confirmation URLs
            var baseUrl = _configuration["AppSettings:BaseUrl"] ?? "http://localhost:5000";
            var confirmUrl = $"{baseUrl}/api/goodsreceipt/supplier-confirm?token={goodsReceipt.SupplierConfirmationToken}&confirmed=true";
            var rejectUrl = $"{baseUrl}/api/goodsreceipt/supplier-confirm?token={goodsReceipt.SupplierConfirmationToken}&confirmed=false";

            // Create HTML email body
            var emailBody = CreateSupplierConfirmationEmailHtml(goodsReceipt, confirmUrl, rejectUrl);
            var subject = $"Xác nhận phiếu nhập hàng #{goodsReceipt.ReceiptNumber}";

            // Create PDF attachment
            var attachments = new List<(string fileName, byte[] content, string mimeType)>
            {
                ($"phieu-nhap-{goodsReceipt.ReceiptNumber}.pdf", pdfBytes, "application/pdf")
            };

            // Send email with PDF attachment  
#pragma warning disable CS8602 // Dereference of a possibly null reference
            var emailSent = await _notificationService.SendEmailWithAttachmentsAsync(
                goodsReceipt.Supplier.Email, 
                subject, 
                emailBody, 
                attachments);
#pragma warning restore CS8602

            if (emailSent)
            {
                _logger.LogInformation($"Confirmation email with PDF sent successfully to supplier {goodsReceipt.Supplier.Email} for goods receipt {goodsReceipt.ReceiptNumber}");
            }
            else
            {
                _logger.LogError($"Failed to send confirmation email with PDF to supplier {goodsReceipt.Supplier.Email} for goods receipt {goodsReceipt.ReceiptNumber}");
            }

            return emailSent;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error sending email with PDF for goods receipt {goodsReceipt.ReceiptNumber}");
            return false;
        }
    }

    public async Task<bool> ConfirmBySupplierAsync(SupplierConfirmationDto confirmationDto)
    {
        var goodsReceipt = await _context.GoodsReceipts
            .FirstOrDefaultAsync(gr => gr.SupplierConfirmationToken == confirmationDto.ConfirmationToken);

        if (goodsReceipt == null || goodsReceipt.Status != GoodsReceiptConstants.Status.Pending)
            return false;

        goodsReceipt.SupplierConfirmed = confirmationDto.Confirmed;
        goodsReceipt.SupplierConfirmedDate = DateTime.UtcNow;
        
        if (confirmationDto.Confirmed)
        {
            goodsReceipt.Status = GoodsReceiptConstants.Status.SupplierConfirmed;
        }
        else
        {
            goodsReceipt.Status = GoodsReceiptConstants.Status.Cancelled;
        }

        // Clear the token after use
        goodsReceipt.SupplierConfirmationToken = null;

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> CompleteReceiptAsync(CompleteReceiptDto completeDto, int currentUserId)
    {
        var goodsReceipt = await _context.GoodsReceipts
            .Include(gr => gr.GoodsReceiptDetails)
                .ThenInclude(grd => grd.Product)
            .FirstOrDefaultAsync(gr => gr.GoodsReceiptId == completeDto.GoodsReceiptId);

        if (goodsReceipt == null || goodsReceipt.Status != GoodsReceiptConstants.Status.SupplierConfirmed)
            return false;

        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            // Update inventory
            foreach (var detail in goodsReceipt.GoodsReceiptDetails)
            {
                // Find or create inventory record for this product
                var inventory = await _context.Inventories
                    .FirstOrDefaultAsync(i => i.ProductId == detail.ProductId);
                
                if (inventory != null)
                {
                    inventory.Quantity += detail.Quantity;
                    inventory.LastUpdatedAt = DateTime.UtcNow;
                }
                else
                {
                    // Create new inventory record if doesn't exist
                    inventory = new Inventory
                    {
                        ProductId = detail.ProductId,
                        Quantity = detail.Quantity,
                        LastUpdatedAt = DateTime.UtcNow
                    };
                    _context.Inventories.Add(inventory);
                }
            }

            // Update goods receipt status
            goodsReceipt.Status = GoodsReceiptConstants.Status.Completed;
            goodsReceipt.CompletedByUserId = currentUserId;
            goodsReceipt.CompletedDate = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();
            
            return true;
        }
        catch
        {
            await transaction.RollbackAsync();
            return false;
        }
    }

    public async Task<bool> CancelReceiptAsync(int goodsReceiptId, int currentUserId)
    {
        var goodsReceipt = await _context.GoodsReceipts.FindAsync(goodsReceiptId);
        if (goodsReceipt == null || goodsReceipt.Status != GoodsReceiptConstants.Status.AwaitingApproval)
            return false;

        // Check if current user is the creator (only creator can cancel)
        if (goodsReceipt.CreatedByUserId != currentUserId)
            return false;

        goodsReceipt.Status = GoodsReceiptConstants.Status.Cancelled;
        
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> ResubmitReceiptAsync(int goodsReceiptId, int currentUserId)
    {
        var goodsReceipt = await _context.GoodsReceipts.FindAsync(goodsReceiptId);
        if (goodsReceipt == null || goodsReceipt.Status != GoodsReceiptConstants.Status.Rejected)
            return false;

        // Check if current user is the creator (only creator can resubmit)
        if (goodsReceipt.CreatedByUserId != currentUserId)
            return false;

        goodsReceipt.Status = GoodsReceiptConstants.Status.AwaitingApproval;
        // Clear previous approval data
        goodsReceipt.ApprovedByUserId = null;
        goodsReceipt.ApprovedDate = null;
        goodsReceipt.ApprovalNotes = null;
        
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<List<string>> GetAvailableActionsAsync(int goodsReceiptId, int currentUserId, string currentUserRole)
    {
        var goodsReceipt = await _context.GoodsReceipts.FindAsync(goodsReceiptId);
        if (goodsReceipt == null)
            return new List<string>();

        var actions = new List<string>();

        switch (goodsReceipt.Status)
        {
            case GoodsReceiptConstants.Status.Draft:
                actions.Add("Edit");
                actions.Add("Submit");
                actions.Add("Delete");
                break;

            case GoodsReceiptConstants.Status.AwaitingApproval:
                if (CanUserApprove(currentUserRole))
                {
                    actions.Add(GoodsReceiptConstants.WorkflowActions.Approve);
                    actions.Add(GoodsReceiptConstants.WorkflowActions.Reject);
                }
                // User can cancel their own receipt when awaiting approval
                if (goodsReceipt.CreatedByUserId == currentUserId)
                {
                    actions.Add("Cancel");
                }
                break;

            case GoodsReceiptConstants.Status.Pending:
                actions.Add("ResendSupplierEmail");
                // User can edit when pending (waiting for supplier)
                if (goodsReceipt.CreatedByUserId == currentUserId)
                {
                    actions.Add("Edit");
                }
                break;

            case GoodsReceiptConstants.Status.SupplierConfirmed:
                if (CanUserComplete(currentUserRole))
                {
                    actions.Add(GoodsReceiptConstants.WorkflowActions.CompleteReceipt);
                }
                break;

            case GoodsReceiptConstants.Status.Rejected:
                // User can resubmit their own rejected receipt
                if (goodsReceipt.CreatedByUserId == currentUserId)
                {
                    actions.Add("Resubmit");
                    actions.Add("Edit");
                }
                break;

            case GoodsReceiptConstants.Status.Completed:
                actions.Add("View");
                actions.Add("Export");
                break;
        }

        // Common actions
        if (goodsReceipt.Status != GoodsReceiptConstants.Status.Completed)
        {
            actions.Add(GoodsReceiptConstants.WorkflowActions.Cancel);
        }

        return actions;
    }

    public bool CanUserApprove(string userRole)
    {
        return userRole == RoleConstants.Admin || userRole == RoleConstants.Manager;
    }

    public bool CanUserComplete(string userRole)
    {
        return userRole == RoleConstants.Admin || 
               userRole == RoleConstants.Manager || 
               userRole == RoleConstants.Employee;
    }

    public string GetInitialStatusByRole(string userRole)
    {
        return userRole == RoleConstants.Employee 
            ? GoodsReceiptConstants.Status.AwaitingApproval 
            : GoodsReceiptConstants.Status.Pending;
    }

    private bool CanEdit(string status, string userRole)
    {
        return status == GoodsReceiptConstants.Status.Draft || 
               (status == GoodsReceiptConstants.Status.AwaitingApproval && CanUserApprove(userRole));
    }

    private string CreateSupplierConfirmationEmailHtml(GoodsReceipt goodsReceipt, string confirmUrl, string rejectUrl)
    {
        return $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
        .header {{ background-color: #4F46E5; color: white; padding: 20px; text-align: center; }}
        .content {{ padding: 20px; background-color: #f9f9f9; }}
        .receipt-info {{ background-color: white; padding: 15px; margin: 15px 0; border-radius: 5px; }}
        .btn {{ display: inline-block; padding: 12px 24px; margin: 10px 5px; text-decoration: none; border-radius: 5px; font-weight: bold; }}
        .btn-confirm {{ background-color: #10B981; color: white; }}
        .btn-reject {{ background-color: #EF4444; color: white; }}
        .footer {{ text-align: center; padding: 20px; color: #666; font-size: 12px; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>Xác nhận phiếu nhập hàng</h1>
        </div>
        
        <div class='content'>
            <p>Kính gửi <strong>{goodsReceipt.Supplier.SupplierName}</strong>,</p>
            
            <p>Chúng tôi đã tạo phiếu nhập hàng mới và cần sự xác nhận của quý công ty.</p>
            
            <div class='receipt-info'>
                <h3>Thông tin phiếu nhập:</h3>
                <p><strong>Số phiếu:</strong> {goodsReceipt.ReceiptNumber}</p>
                <p><strong>Ngày tạo:</strong> {goodsReceipt.ReceiptDate:dd/MM/yyyy HH:mm}</p>
                <p><strong>Người tạo:</strong> {goodsReceipt.CreatedByUser.FullName}</p>
                <p><strong>Tổng tiền:</strong> {goodsReceipt.TotalAmount:N0} VNĐ</p>
                <p><strong>Ghi chú:</strong> {goodsReceipt.Notes ?? "Không có"}</p>
            </div>
            
            <p>Vui lòng xác nhận hoặc từ chối phiếu nhập này:</p>
            
            <div style='text-align: center; margin: 30px 0;'>
                <a href='{confirmUrl}' class='btn btn-confirm'>✓ XÁC NHẬN</a>
                <a href='{rejectUrl}' class='btn btn-reject'>✗ TỪ CHỐI</a>
            </div>
            
            <p style='color: #666; font-size: 14px;'>
                <strong>Lưu ý:</strong> Việc xác nhận có nghĩa là quý công ty đồng ý với nội dung và số lượng hàng hóa trong phiếu nhập này.
            </p>
        </div>
        
        <div class='footer'>
            <p>Email này được gửi tự động từ hệ thống quản lý kho.</p>
            <p>Vui lòng không trả lời email này.</p>
        </div>
    </div>
</body>
</html>";
    }
}
