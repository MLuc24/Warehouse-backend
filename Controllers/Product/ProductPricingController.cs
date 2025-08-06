using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WarehouseManage.DTOs.Product;
using WarehouseManage.Interfaces;

namespace WarehouseManage.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ProductPricingController : ControllerBase
{
    private readonly IProductPricingService _pricingService;
    private readonly ILogger<ProductPricingController> _logger;

    public ProductPricingController(IProductPricingService pricingService, ILogger<ProductPricingController> logger)
    {
        _pricingService = pricingService;
        _logger = logger;
    }

    /// <summary>
    /// Lấy thông tin giá của tất cả sản phẩm
    /// </summary>
    [HttpGet]
    [Authorize(Roles = "Admin,Manager,Employee")]
    public async Task<ActionResult<List<ProductPricingDto>>> GetAllPricingInfo()
    {
        try
        {
            var pricingInfo = await _pricingService.GetAllPricingInfoAsync();
            return Ok(pricingInfo);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all pricing information");
            return StatusCode(500, new { message = "Đã xảy ra lỗi khi lấy thông tin giá" });
        }
    }

    /// <summary>
    /// Lấy thông tin giá của một sản phẩm
    /// </summary>
    [HttpGet("{productId}")]
    [Authorize(Roles = "Admin,Manager,Employee")]
    public async Task<ActionResult<ProductPricingDto>> GetPricingInfo(int productId)
    {
        try
        {
            var pricingInfo = await _pricingService.GetPricingInfoAsync(productId);
            if (pricingInfo == null)
                return NotFound(new { message = "Sản phẩm không tồn tại" });

            return Ok(pricingInfo);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting pricing information for product {ProductId}", productId);
            return StatusCode(500, new { message = "Đã xảy ra lỗi khi lấy thông tin giá" });
        }
    }

    /// <summary>
    /// Cập nhật giá sản phẩm
    /// </summary>
    [HttpPut]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<ActionResult> UpdatePricing([FromBody] UpdateProductPricingDto updateDto)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _pricingService.UpdatePricingAsync(updateDto);
            if (!result)
                return NotFound(new { message = "Sản phẩm không tồn tại" });

            return Ok(new { message = "Cập nhật giá thành công" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating pricing");
            return StatusCode(500, new { message = "Đã xảy ra lỗi khi cập nhật giá" });
        }
    }

    /// <summary>
    /// Cập nhật giá hàng loạt
    /// </summary>
    [HttpPut("bulk-update")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<ActionResult<BulkOperationResultDto>> UpdatePricingBulk([FromBody] BulkUpdatePricingDto updateDto)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _pricingService.UpdatePricingBulkAsync(updateDto);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in bulk pricing update");
            return StatusCode(500, new { message = "Đã xảy ra lỗi khi cập nhật giá hàng loạt" });
        }
    }

    /// <summary>
    /// Lấy lịch sử thay đổi giá
    /// </summary>
    [HttpGet("{productId}/history")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<ActionResult<List<PriceHistoryDto>>> GetPriceHistory(int productId, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        try
        {
            var history = await _pricingService.GetPriceHistoryAsync(productId, page, pageSize);
            return Ok(history);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting price history for product {ProductId}", productId);
            return StatusCode(500, new { message = "Đã xảy ra lỗi khi lấy lịch sử giá" });
        }
    }

    /// <summary>
    /// Lấy phân tích giá sản phẩm
    /// </summary>
    [HttpGet("analysis")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<ActionResult<PricingAnalysisDto>> GetPricingAnalysis()
    {
        try
        {
            var analysis = await _pricingService.GetPricingAnalysisAsync();
            return Ok(analysis);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting pricing analysis");
            return StatusCode(500, new { message = "Đã xảy ra lỗi khi phân tích giá" });
        }
    }
}
