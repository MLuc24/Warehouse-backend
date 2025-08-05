using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WarehouseManage.DTOs.Product;
using WarehouseManage.Interfaces;

namespace WarehouseManage.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ProductStockController : ControllerBase
{
    private readonly IProductStockService _stockService;
    private readonly ILogger<ProductStockController> _logger;

    public ProductStockController(IProductStockService stockService, ILogger<ProductStockController> logger)
    {
        _stockService = stockService;
        _logger = logger;
    }

    /// <summary>
    /// Lấy thông tin tồn kho của tất cả sản phẩm
    /// </summary>
    [HttpGet]
    [Authorize(Roles = "Admin,Manager,Employee")]
    public async Task<ActionResult<List<ProductStockDto>>> GetAllStockInfo()
    {
        try
        {
            var stockInfo = await _stockService.GetAllStockInfoAsync();
            return Ok(stockInfo);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all stock information");
            return StatusCode(500, new { message = "Đã xảy ra lỗi khi lấy thông tin tồn kho" });
        }
    }

    /// <summary>
    /// Lấy thông tin tồn kho của một sản phẩm
    /// </summary>
    [HttpGet("{productId}")]
    [Authorize(Roles = "Admin,Manager,Employee")]
    public async Task<ActionResult<ProductStockDto>> GetStockInfo(int productId)
    {
        try
        {
            var stockInfo = await _stockService.GetStockInfoAsync(productId);
            if (stockInfo == null)
                return NotFound(new { message = "Sản phẩm không tồn tại" });

            return Ok(stockInfo);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting stock information for product {ProductId}", productId);
            return StatusCode(500, new { message = "Đã xảy ra lỗi khi lấy thông tin tồn kho" });
        }
    }

    /// <summary>
    /// Lấy báo cáo tồn kho tổng quan
    /// </summary>
    [HttpGet("report")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<ActionResult<StockReportDto>> GetStockReport()
    {
        try
        {
            var report = await _stockService.GetStockReportAsync();
            return Ok(report);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating stock report");
            return StatusCode(500, new { message = "Đã xảy ra lỗi khi tạo báo cáo tồn kho" });
        }
    }

    /// <summary>
    /// Lấy danh sách sản phẩm sắp hết hàng
    /// </summary>
    [HttpGet("low-stock")]
    [Authorize(Roles = "Admin,Manager,Employee")]
    public async Task<ActionResult<List<ProductStockDto>>> GetLowStockProducts()
    {
        try
        {
            var lowStockProducts = await _stockService.GetLowStockProductsAsync();
            return Ok(lowStockProducts);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting low stock products");
            return StatusCode(500, new { message = "Đã xảy ra lỗi khi lấy danh sách sản phẩm sắp hết hàng" });
        }
    }

    /// <summary>
    /// Lấy danh sách sản phẩm hết hàng
    /// </summary>
    [HttpGet("out-of-stock")]
    [Authorize(Roles = "Admin,Manager,Employee")]
    public async Task<ActionResult<List<ProductStockDto>>> GetOutOfStockProducts()
    {
        try
        {
            var outOfStockProducts = await _stockService.GetOutOfStockProductsAsync();
            return Ok(outOfStockProducts);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting out of stock products");
            return StatusCode(500, new { message = "Đã xảy ra lỗi khi lấy danh sách sản phẩm hết hàng" });
        }
    }

    /// <summary>
    /// Cập nhật mức tồn kho tối thiểu/tối đa
    /// </summary>
    [HttpPut("levels")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<ActionResult> UpdateStockLevels([FromBody] UpdateStockLevelsDto updateDto)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _stockService.UpdateStockLevelsAsync(updateDto);
            if (!result)
                return NotFound(new { message = "Sản phẩm không tồn tại" });

            return Ok(new { message = "Cập nhật mức tồn kho thành công" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating stock levels");
            return StatusCode(500, new { message = "Đã xảy ra lỗi khi cập nhật mức tồn kho" });
        }
    }

    /// <summary>
    /// Điều chỉnh tồn kho (tăng/giảm)
    /// </summary>
    [HttpPost("adjust")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<ActionResult> AdjustStock([FromBody] StockAdjustmentDto adjustmentDto)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _stockService.AdjustStockAsync(adjustmentDto);
            if (!result)
                return NotFound(new { message = "Sản phẩm không tồn tại" });

            return Ok(new { message = "Điều chỉnh tồn kho thành công" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adjusting stock");
            return StatusCode(500, new { message = "Đã xảy ra lỗi khi điều chỉnh tồn kho" });
        }
    }

    /// <summary>
    /// Thiết lập điểm đặt hàng lại (reorder point)
    /// </summary>
    [HttpPut("{productId}/reorder-point")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<ActionResult> SetReorderPoint(int productId, [FromBody] UpdateStockLevelsDto updateDto)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // Set the productId from route parameter
            updateDto.ProductId = productId;
            
            var result = await _stockService.UpdateStockLevelsAsync(updateDto);
            if (!result)
                return NotFound(new { message = "Sản phẩm không tồn tại" });

            return Ok(new { message = "Thiết lập điểm đặt hàng lại thành công" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting reorder point for product {ProductId}", productId);
            return StatusCode(500, new { message = "Đã xảy ra lỗi khi thiết lập điểm đặt hàng lại" });
        }
    }

    /// <summary>
    /// Lấy lịch sử thay đổi tồn kho
    /// </summary>
    [HttpGet("{productId}/history")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<ActionResult<List<StockHistoryDto>>> GetStockHistory(int productId, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        try
        {
            var history = await _stockService.GetStockHistoryAsync(productId, page, pageSize);
            return Ok(history);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting stock history for product {ProductId}", productId);
            return StatusCode(500, new { message = "Đã xảy ra lỗi khi lấy lịch sử tồn kho" });
        }
    }
}
