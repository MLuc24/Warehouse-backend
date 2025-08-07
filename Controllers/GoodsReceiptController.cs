using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using WarehouseManage.Constants;
using WarehouseManage.DTOs.GoodsReceipt;
using WarehouseManage.Interfaces;
using WarehouseManage.Services;

namespace WarehouseManage.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class GoodsReceiptController : ControllerBase
{
    private readonly IGoodsReceiptService _goodsReceiptService;
    private readonly IGoodsReceiptWorkflowService _workflowService;

    public GoodsReceiptController(
        IGoodsReceiptService goodsReceiptService,
        IGoodsReceiptWorkflowService workflowService)
    {
        _goodsReceiptService = goodsReceiptService;
        _workflowService = workflowService;
    }

    /// <summary>
    /// Lấy danh sách phiếu nhập kho với phân trang và bộ lọc
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<PagedResult<GoodsReceiptDto>>> GetGoodsReceipts([FromQuery] GoodsReceiptFilterDto filter)
    {
        try
        {
            var result = await _goodsReceiptService.GetGoodsReceiptsAsync(filter);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Lỗi khi lấy danh sách phiếu nhập", error = ex.Message });
        }
    }

    /// <summary>
    /// Lấy chi tiết phiếu nhập kho theo ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<GoodsReceiptDto>> GetGoodsReceiptById(int id)
    {
        try
        {
            if (id <= 0)
            {
                return BadRequest(new { message = "ID phiếu nhập không hợp lệ" });
            }

            var receipt = await _goodsReceiptService.GetGoodsReceiptByIdAsync(id);
            if (receipt == null)
            {
                return NotFound(new { message = GoodsReceiptConstants.ErrorMessages.GoodsReceiptNotFound });
            }

            return Ok(receipt);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Lỗi khi lấy chi tiết phiếu nhập", error = ex.Message });
        }
    }

    /// <summary>
    /// Lấy chi tiết phiếu nhập kho theo số phiếu
    /// </summary>
    [HttpGet("by-number/{receiptNumber}")]
    public async Task<ActionResult<GoodsReceiptDto>> GetGoodsReceiptByNumber(string receiptNumber)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(receiptNumber))
            {
                return BadRequest(new { message = "Số phiếu nhập không được để trống" });
            }

            var receipt = await _goodsReceiptService.GetGoodsReceiptByNumberAsync(receiptNumber);
            if (receipt == null)
            {
                return NotFound(new { message = GoodsReceiptConstants.ErrorMessages.GoodsReceiptNotFound });
            }

            return Ok(receipt);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Lỗi khi lấy chi tiết phiếu nhập", error = ex.Message });
        }
    }

    /// <summary>
    /// Tạo phiếu nhập kho mới
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<GoodsReceiptDto>> CreateGoodsReceipt([FromBody] CreateGoodsReceiptDto dto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var userId = GetCurrentUserId();
            if (userId == 0)
            {
                return Unauthorized(new { message = "Không thể xác thực người dùng" });
            }

            var receipt = await _goodsReceiptService.CreateGoodsReceiptAsync(dto, userId);
            return CreatedAtAction(nameof(GetGoodsReceiptById), new { id = receipt.GoodsReceiptId }, receipt);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Lỗi khi tạo phiếu nhập", error = ex.Message });
        }
    }

    /// <summary>
    /// Cập nhật phiếu nhập kho
    /// </summary>
    [HttpPut("{id}")]
    public async Task<ActionResult<GoodsReceiptDto>> UpdateGoodsReceipt(int id, [FromBody] UpdateGoodsReceiptDto dto)
    {
        try
        {
            if (id != dto.GoodsReceiptId)
            {
                return BadRequest(new { message = "ID trong URL và body không khớp" });
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var receipt = await _goodsReceiptService.UpdateGoodsReceiptAsync(dto);
            return Ok(receipt);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Lỗi khi cập nhật phiếu nhập", error = ex.Message });
        }
    }

    /// <summary>
    /// Xóa phiếu nhập kho
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteGoodsReceipt(int id)
    {
        try
        {
            if (id <= 0)
            {
                return BadRequest(new { message = "ID phiếu nhập không hợp lệ" });
            }

            var result = await _goodsReceiptService.DeleteGoodsReceiptAsync(id);
            if (!result)
            {
                return NotFound(new { message = GoodsReceiptConstants.ErrorMessages.GoodsReceiptNotFound });
            }

            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Lỗi khi xóa phiếu nhập", error = ex.Message });
        }
    }

    /// <summary>
    /// Lấy danh sách phiếu nhập theo nhà cung cấp
    /// </summary>
    [HttpGet("by-supplier/{supplierId}")]
    public async Task<ActionResult<List<GoodsReceiptDto>>> GetGoodsReceiptsBySupplier(int supplierId)
    {
        try
        {
            if (supplierId <= 0)
            {
                return BadRequest(new { message = "ID nhà cung cấp không hợp lệ" });
            }

            var receipts = await _goodsReceiptService.GetGoodsReceiptsBySupplierAsync(supplierId);
            return Ok(receipts);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Lỗi khi lấy danh sách phiếu nhập theo nhà cung cấp", error = ex.Message });
        }
    }

    /// <summary>
    /// Kiểm tra xem có thể xóa phiếu nhập hay không
    /// </summary>
    [HttpGet("{id}/can-delete")]
    public async Task<ActionResult<bool>> CanDeleteGoodsReceipt(int id)
    {
        try
        {
            if (id <= 0)
            {
                return BadRequest(new { message = "ID phiếu nhập không hợp lệ" });
            }

            var canDelete = await _goodsReceiptService.CanDeleteGoodsReceiptAsync(id);
            return Ok(new { canDelete });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Lỗi khi kiểm tra quyền xóa phiếu nhập", error = ex.Message });
        }
    }

    /// <summary>
    /// Lấy trạng thái workflow và các hành động có thể thực hiện
    /// </summary>
    [HttpGet("{id}/workflow-status")]
    public async Task<ActionResult<WorkflowStatusDto>> GetWorkflowStatus(int id)
    {
        try
        {
            var currentUserId = GetCurrentUserId();
            if (currentUserId == 0)
            {
                return Unauthorized(new { message = "Không thể xác định người dùng" });
            }

            var status = await _workflowService.GetWorkflowStatusAsync(id, currentUserId);
            return Ok(status);
        }
        catch (ArgumentException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Lỗi khi lấy trạng thái workflow", error = ex.Message });
        }
    }

    /// <summary>
    /// Phê duyệt hoặc từ chối phiếu nhập (Admin/Manager only)
    /// </summary>
    [HttpPost("{id}/approve-reject")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> ApproveOrReject(int id, [FromBody] ApprovalDto dto)
    {
        try
        {
            dto.GoodsReceiptId = id;
            var currentUserId = GetCurrentUserId();
            if (currentUserId == 0)
            {
                return Unauthorized(new { message = "Không thể xác định người dùng" });
            }

            var result = await _workflowService.ApproveOrRejectAsync(dto, currentUserId);
            if (!result)
            {
                return BadRequest(new { message = "Không thể thực hiện hành động này" });
            }

            return Ok(new { message = dto.Action == "Approve" ? "Phiếu nhập đã được phê duyệt" : "Phiếu nhập đã bị từ chối" });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Lỗi khi xử lý phê duyệt", error = ex.Message });
        }
    }



    /// <summary>
    /// Nhà cung cấp xác nhận phiếu nhập qua link email (GET request)
    /// </summary>
    [HttpGet("supplier-confirm")]
    [AllowAnonymous]
    public async Task<IActionResult> SupplierConfirmViaEmail([FromQuery] string token, [FromQuery] bool confirmed)
    {
        try
        {
            var dto = new SupplierConfirmationDto
            {
                ConfirmationToken = token,
                Confirmed = confirmed
            };

            var result = await _workflowService.ConfirmBySupplierAsync(dto);
            if (!result)
            {
                return Content(@"
                    <!DOCTYPE html>
                    <html>
                    <head><title>Lỗi xác nhận</title></head>
                    <body style='font-family: Arial, sans-serif; text-align: center; padding: 50px;'>
                        <h2 style='color: #EF4444;'>❌ Lỗi xác nhận</h2>
                        <p>Token không hợp lệ hoặc phiếu nhập không ở trạng thái phù hợp để xác nhận.</p>
                    </body>
                    </html>", "text/html");
            }

            var message = confirmed 
                ? "✅ Cảm ơn bạn đã xác nhận phiếu nhập. Phiếu nhập sẽ được xử lý." 
                : "❌ Phiếu nhập đã bị từ chối theo yêu cầu của bạn.";

            return Content($@"
                <!DOCTYPE html>
                <html>
                <head>
                    <title>Xác nhận thành công</title>
                    <meta charset='utf-8'>
                </head>
                <body style='font-family: Arial, sans-serif; text-align: center; padding: 50px;'>
                    <h2 style='color: {(confirmed ? "#10B981" : "#EF4444")};'>{message}</h2>
                    <p>Cảm ơn bạn đã phản hồi.</p>
                    <script>
                        setTimeout(function() {{
                            window.close();
                        }}, 3000);
                    </script>
                </body>
                </html>", "text/html");
        }
        catch (Exception ex)
        {
            return Content($@"
                <!DOCTYPE html>
                <html>
                <head><title>Lỗi hệ thống</title></head>
                <body style='font-family: Arial, sans-serif; text-align: center; padding: 50px;'>
                    <h2 style='color: #EF4444;'>❌ Lỗi hệ thống</h2>
                    <p>Đã xảy ra lỗi khi xử lý yêu cầu: {ex.Message}</p>
                </body>
                </html>", "text/html");
        }
    }

    /// <summary>
    /// Nhà cung cấp xác nhận phiếu nhập (Public endpoint - không cần authorize)
    /// </summary>
    [HttpPost("supplier-confirm")]
    [AllowAnonymous]
    public async Task<IActionResult> SupplierConfirm([FromBody] SupplierConfirmationDto dto)
    {
        try
        {
            var result = await _workflowService.ConfirmBySupplierAsync(dto);
            if (!result)
            {
                return BadRequest(new { message = "Token không hợp lệ hoặc phiếu nhập không ở trạng thái phù hợp" });
            }

            return Ok(new { 
                message = dto.Confirmed 
                    ? "Cảm ơn bạn đã xác nhận phiếu nhập. Phiếu nhập sẽ được xử lý." 
                    : "Phiếu nhập đã bị hủy theo yêu cầu của bạn." 
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Lỗi khi xác nhận", error = ex.Message });
        }
    }

    /// <summary>
    /// Hoàn thành phiếu nhập - nhập kho (All roles can complete)
    /// </summary>
    [HttpPost("{id}/complete")]
    public async Task<IActionResult> CompleteReceipt(int id, [FromBody] CompleteReceiptDto dto)
    {
        try
        {
            dto.GoodsReceiptId = id;
            var currentUserId = GetCurrentUserId();
            if (currentUserId == 0)
            {
                return Unauthorized(new { message = "Không thể xác định người dùng" });
            }

            var result = await _workflowService.CompleteReceiptAsync(dto, currentUserId);
            if (!result)
            {
                return BadRequest(new { message = "Không thể hoàn thành phiếu nhập. Phiếu nhập phải ở trạng thái 'Nhà cung cấp đã xác nhận'" });
            }

            return Ok(new { message = "Phiếu nhập đã được hoàn thành và số lượng tồn kho đã được cập nhật" });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Lỗi khi hoàn thành phiếu nhập", error = ex.Message });
        }
    }

    /// <summary>
    /// Hủy phiếu nhập (User can cancel when AwaitingApproval)
    /// </summary>
    [HttpPost("{id}/cancel")]
    public async Task<IActionResult> CancelReceipt(int id)
    {
        try
        {
            var currentUserId = GetCurrentUserId();
            if (currentUserId == 0)
            {
                return Unauthorized(new { message = "Không thể xác định người dùng" });
            }

            var result = await _workflowService.CancelReceiptAsync(id, currentUserId);
            if (!result)
            {
                return BadRequest(new { message = "Không thể hủy phiếu nhập. Chỉ có thể hủy phiếu nhập ở trạng thái 'Chờ phê duyệt'" });
            }

            return Ok(new { message = "Phiếu nhập đã được hủy" });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Lỗi khi hủy phiếu nhập", error = ex.Message });
        }
    }

    /// <summary>
    /// Gửi lại phiếu nhập để phê duyệt (After rejected)
    /// </summary>
    [HttpPost("{id}/resubmit")]
    public async Task<IActionResult> ResubmitReceipt(int id)
    {
        try
        {
            var currentUserId = GetCurrentUserId();
            if (currentUserId == 0)
            {
                return Unauthorized(new { message = "Không thể xác định người dùng" });
            }

            var result = await _workflowService.ResubmitReceiptAsync(id, currentUserId);
            if (!result)
            {
                return BadRequest(new { message = "Không thể gửi lại phiếu nhập. Chỉ có thể gửi lại phiếu nhập ở trạng thái 'Bị từ chối'" });
            }

            return Ok(new { message = "Phiếu nhập đã được gửi lại để phê duyệt" });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Lỗi khi gửi lại phiếu nhập", error = ex.Message });
        }
    }

    /// <summary>
    /// Xuất phiếu nhập ra PDF
    /// </summary>
    [HttpGet("{id}/pdf")]
    public async Task<IActionResult> ExportToPDF(int id)
    {
        try
        {
            if (id <= 0)
            {
                return BadRequest(new { message = "ID phiếu nhập không hợp lệ" });
            }

            var receipt = await _goodsReceiptService.GetGoodsReceiptByIdAsync(id);
            if (receipt == null)
            {
                return NotFound(new { message = "Không tìm thấy phiếu nhập" });
            }

            var pdfService = HttpContext.RequestServices.GetRequiredService<IPdfService>();
            var pdfBytes = await pdfService.GenerateGoodsReceiptPdfAsync(receipt);

            var fileName = $"phieu-nhap-{receipt.ReceiptNumber ?? receipt.GoodsReceiptId.ToString()}.pdf";
            
            return File(pdfBytes, "application/pdf", fileName);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Lỗi khi xuất PDF", error = ex.Message });
        }
    }

    /// <summary>
    /// Gửi lại email xác nhận cho nhà cung cấp (luôn có PDF đính kèm)
    /// </summary>
    [HttpPost("{id}/resend-supplier-email")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> ResendSupplierEmail(int id)
    {
        try
        {
            var result = await _workflowService.SendSupplierConfirmationEmailAsync(id);
            if (!result)
            {
                return BadRequest(new { message = "Không thể gửi email với PDF" });
            }

            return Ok(new { message = "Email kèm PDF đã được gửi cho nhà cung cấp" });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Lỗi khi gửi email với PDF", error = ex.Message });
        }
    }

    private int GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(AuthConstants.ClaimTypes.USER_ID);
        if (userIdClaim != null && int.TryParse(userIdClaim.Value, out int userId))
        {
            return userId;
        }
        return 0;
    }
}
