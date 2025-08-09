using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using WarehouseManage.Constants;
using WarehouseManage.DTOs.GoodsIssue;
using WarehouseManage.DTOs.GoodsReceipt;
using WarehouseManage.Interfaces;
using GoodsIssueWorkflowDto = WarehouseManage.DTOs.GoodsIssue.WorkflowStatusDto;
using GoodsIssueApprovalDto = WarehouseManage.DTOs.GoodsIssue.ApprovalDto;

namespace WarehouseManage.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class GoodsIssueController : ControllerBase
{
    private readonly IGoodsIssueService _goodsIssueService;
    private readonly IGoodsIssueWorkflowService _workflowService;

    public GoodsIssueController(
        IGoodsIssueService goodsIssueService,
        IGoodsIssueWorkflowService workflowService)
    {
        _goodsIssueService = goodsIssueService;
        _workflowService = workflowService;
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

    // CRUD Endpoints

    [HttpGet]
    public async Task<ActionResult<PagedResult<GoodsIssueDto>>> GetGoodsIssues([FromQuery] GoodsIssueFilterDto filter)
    {
        try
        {
            var result = await _goodsIssueService.GetGoodsIssuesAsync(filter);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<GoodsIssueDto>> GetGoodsIssueById(int id)
    {
        try
        {
            var goodsIssue = await _goodsIssueService.GetGoodsIssueByIdAsync(id);
            if (goodsIssue == null)
                return NotFound("Không tìm thấy phiếu xuất kho");

            return Ok(goodsIssue);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpGet("by-number/{issueNumber}")]
    public async Task<ActionResult<GoodsIssueDto>> GetGoodsIssueByNumber(string issueNumber)
    {
        try
        {
            var goodsIssue = await _goodsIssueService.GetGoodsIssueByNumberAsync(issueNumber);
            if (goodsIssue == null)
                return NotFound("Không tìm thấy phiếu xuất kho");

            return Ok(goodsIssue);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPost]
    public async Task<ActionResult<GoodsIssueDto>> CreateGoodsIssue([FromBody] CreateGoodsIssueDto dto)
    {
        try
        {
            var currentUserId = GetCurrentUserId();
            if (currentUserId == 0)
                return Unauthorized();

            var goodsIssue = await _goodsIssueService.CreateGoodsIssueAsync(dto, currentUserId);
            return CreatedAtAction(nameof(GetGoodsIssueById), new { id = goodsIssue.GoodsIssueId }, goodsIssue);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            return StatusCode(500, ex.Message);
        }
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<GoodsIssueDto>> UpdateGoodsIssue(int id, [FromBody] UpdateGoodsIssueDto dto)
    {
        try
        {
            if (id != dto.GoodsIssueId)
                return BadRequest("ID không khớp");

            var goodsIssue = await _goodsIssueService.UpdateGoodsIssueAsync(dto);
            return Ok(goodsIssue);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            return StatusCode(500, ex.Message);
        }
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteGoodsIssue(int id)
    {
        try
        {
            var result = await _goodsIssueService.DeleteGoodsIssueAsync(id);
            if (!result)
                return BadRequest("Không thể xóa phiếu xuất kho");

            return NoContent();
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    // Workflow Endpoints

    [HttpGet("{id}/workflow-status")]
    public async Task<ActionResult<GoodsIssueWorkflowDto>> GetWorkflowStatus(int id)
    {
        try
        {
            var currentUserId = GetCurrentUserId();
            if (currentUserId == 0)
                return Unauthorized();

            var status = await _workflowService.GetWorkflowStatusAsync(id, currentUserId);
            return Ok(status);
        }
        catch (ArgumentException ex)
        {
            return NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPost("{id}/approve-reject")]
    public async Task<ActionResult> ApproveOrReject(int id, [FromBody] GoodsIssueApprovalDto approvalDto)
    {
        try
        {
            if (id != approvalDto.GoodsIssueId)
                return BadRequest("ID không khớp");

            var currentUserId = GetCurrentUserId();
            if (currentUserId == 0)
                return Unauthorized();

            var result = await _workflowService.ApproveOrRejectAsync(approvalDto, currentUserId);
            if (!result)
                return BadRequest("Không thể thực hiện hành động này");

            return Ok(new { message = "Thực hiện thành công" });
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPost("{id}/start-preparing")]
    public async Task<ActionResult> StartPreparing(int id)
    {
        try
        {
            var currentUserId = GetCurrentUserId();
            if (currentUserId == 0)
                return Unauthorized();

            var result = await _workflowService.StartPreparingAsync(id, currentUserId);
            if (!result)
                return BadRequest("Không thể bắt đầu chuẩn bị");

            return Ok(new { message = "Đã bắt đầu chuẩn bị hàng" });
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPost("{id}/mark-delivered")]
    public async Task<ActionResult> MarkDelivered(int id, [FromBody] DeliveryDto deliveryDto)
    {
        try
        {
            if (id != deliveryDto.GoodsIssueId)
                return BadRequest("ID không khớp");

            var currentUserId = GetCurrentUserId();
            if (currentUserId == 0)
                return Unauthorized();

            var result = await _workflowService.MarkDeliveredAsync(deliveryDto, currentUserId);
            if (!result)
                return BadRequest("Không thể đánh dấu đã giao");

            return Ok(new { message = "Đã đánh dấu giao hàng thành công" });
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPost("{id}/complete")]
    public async Task<ActionResult> CompleteIssue(int id, [FromBody] CompleteIssueDto completeDto)
    {
        try
        {
            if (id != completeDto.GoodsIssueId)
                return BadRequest("ID không khớp");

            var currentUserId = GetCurrentUserId();
            if (currentUserId == 0)
                return Unauthorized();

            var result = await _workflowService.CompleteIssueAsync(completeDto, currentUserId);
            if (!result)
                return BadRequest("Không thể hoàn tất phiếu xuất");

            return Ok(new { message = "Hoàn tất phiếu xuất thành công" });
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPost("{id}/cancel")]
    public async Task<ActionResult> CancelIssue(int id)
    {
        try
        {
            var currentUserId = GetCurrentUserId();
            if (currentUserId == 0)
                return Unauthorized();

            var result = await _workflowService.CancelIssueAsync(id, currentUserId);
            if (!result)
                return BadRequest("Không thể hủy phiếu xuất");

            return Ok(new { message = "Đã hủy phiếu xuất" });
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPost("{id}/resubmit")]
    public async Task<ActionResult> ResubmitIssue(int id)
    {
        try
        {
            var currentUserId = GetCurrentUserId();
            if (currentUserId == 0)
                return Unauthorized();

            var result = await _workflowService.ResubmitIssueAsync(id, currentUserId);
            if (!result)
                return BadRequest("Không thể gửi lại phiếu xuất");

            return Ok(new { message = "Đã gửi lại phiếu xuất để phê duyệt" });
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    // Inventory & Validation

    [HttpGet("{id}/check-inventory")]
    public async Task<ActionResult<bool>> CheckInventoryAvailability(int id)
    {
        try
        {
            var result = await _workflowService.CheckInventoryAvailabilityAsync(id);
            return Ok(new { available = result });
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    // Customer Related

    [HttpGet("by-customer/{customerId}")]
    public async Task<ActionResult<List<GoodsIssueDto>>> GetGoodsIssuesByCustomer(int customerId)
    {
        try
        {
            var goodsIssues = await _goodsIssueService.GetGoodsIssuesByCustomerAsync(customerId);
            return Ok(goodsIssues);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    // Validation

    [HttpGet("{id}/can-delete")]
    public async Task<ActionResult<bool>> CanDeleteGoodsIssue(int id)
    {
        try
        {
            var result = await _goodsIssueService.CanDeleteGoodsIssueAsync(id);
            return Ok(new { canDelete = result });
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }
}
