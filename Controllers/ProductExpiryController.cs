using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WarehouseManage.DTOs.Product;
using WarehouseManage.Interfaces;

namespace WarehouseManage.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ProductExpiryController : ControllerBase
{
    private readonly IProductExpiryService _expiryService;
    private readonly ILogger<ProductExpiryController> _logger;

    public ProductExpiryController(IProductExpiryService expiryService, ILogger<ProductExpiryController> logger)
    {
        _expiryService = expiryService;
        _logger = logger;
    }

    /// <summary>
    /// Lấy thông tin hạn sử dụng sản phẩm với tìm kiếm và lọc
    /// </summary>
    [HttpGet]
    [Authorize(Roles = "Admin,Manager,Employee")]
    public async Task<ActionResult<List<ProductExpiryDto>>> GetExpiryInfo([FromQuery] ExpirySearchDto searchDto)
    {
        try
        {
            var expiryInfo = await _expiryService.GetExpiryInfoAsync(searchDto);
            return Ok(expiryInfo);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting expiry information");
            return StatusCode(500, new { message = "Đã xảy ra lỗi khi lấy thông tin hạn sử dụng" });
        }
    }

    /// <summary>
    /// Lấy báo cáo hạn sử dụng tổng quan
    /// </summary>
    [HttpGet("report")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<ActionResult<ExpiryReportDto>> GetExpiryReport()
    {
        try
        {
            var report = await _expiryService.GetExpiryReportAsync();
            return Ok(report);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating expiry report");
            return StatusCode(500, new { message = "Đã xảy ra lỗi khi tạo báo cáo hạn sử dụng" });
        }
    }

    /// <summary>
    /// Lấy danh sách sản phẩm đã hết hạn
    /// </summary>
    [HttpGet("expired")]
    [Authorize(Roles = "Admin,Manager,Employee")]
    public async Task<ActionResult<List<ProductExpiryDto>>> GetExpiredProducts()
    {
        try
        {
            var expiredProducts = await _expiryService.GetExpiredProductsAsync();
            return Ok(expiredProducts);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting expired products");
            return StatusCode(500, new { message = "Đã xảy ra lỗi khi lấy danh sách sản phẩm hết hạn" });
        }
    }

    /// <summary>
    /// Lấy danh sách sản phẩm sắp hết hạn
    /// </summary>
    [HttpGet("expiring-soon")]
    [Authorize(Roles = "Admin,Manager,Employee")]
    public async Task<ActionResult<List<ProductExpiryDto>>> GetExpiringSoonProducts([FromQuery] int days = 7)
    {
        try
        {
            var expiringSoonProducts = await _expiryService.GetExpiringSoonProductsAsync(days);
            return Ok(expiringSoonProducts);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting expiring soon products");
            return StatusCode(500, new { message = "Đã xảy ra lỗi khi lấy danh sách sản phẩm sắp hết hạn" });
        }
    }

    /// <summary>
    /// Lấy cảnh báo hạn sử dụng
    /// </summary>
    [HttpGet("alerts")]
    [Authorize(Roles = "Admin,Manager,Employee")]
    public async Task<ActionResult<List<ExpiryAlertDto>>> GetExpiryAlerts()
    {
        try
        {
            var alerts = await _expiryService.GetExpiryAlertsAsync();
            return Ok(alerts);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting expiry alerts");
            return StatusCode(500, new { message = "Đã xảy ra lỗi khi lấy cảnh báo hạn sử dụng" });
        }
    }

    /// <summary>
    /// Cập nhật thông tin hạn sử dụng
    /// </summary>
    [HttpPut]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<ActionResult> UpdateExpiryInfo([FromBody] UpdateProductExpiryDto updateDto)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _expiryService.UpdateExpiryInfoAsync(updateDto);
            if (!result)
                return NotFound(new { message = "Sản phẩm không tồn tại" });

            return Ok(new { message = "Cập nhật thông tin hạn sử dụng thành công" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating expiry information");
            return StatusCode(500, new { message = "Đã xảy ra lỗi khi cập nhật thông tin hạn sử dụng" });
        }
    }
}
