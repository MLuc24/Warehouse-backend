using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WarehouseManage.DTOs.Product;
using WarehouseManage.Interfaces;

namespace WarehouseManage.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ProductDashboardController : ControllerBase
{
    private readonly ICategoryService _categoryService;
    private readonly IProductStockService _stockService;
    private readonly IProductPricingService _pricingService;
    private readonly IProductExpiryService _expiryService;
    private readonly ILogger<ProductDashboardController> _logger;

    public ProductDashboardController(
        ICategoryService categoryService,
        IProductStockService stockService,
        IProductPricingService pricingService,
        IProductExpiryService expiryService,
        ILogger<ProductDashboardController> logger)
    {
        _categoryService = categoryService;
        _stockService = stockService;
        _pricingService = pricingService;
        _expiryService = expiryService;
        _logger = logger;
    }

    /// <summary>
    /// Lấy dữ liệu dashboard tổng quan
    /// </summary>
    [HttpGet("overview")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<ActionResult<ProductDashboardDto>> GetDashboardOverview()
    {
        try
        {
            var dashboard = new ProductDashboardDto();

            // Get stock report
            var stockReport = await _stockService.GetStockReportAsync();
            dashboard.Overview = new ProductOverviewDto
            {
                TotalProducts = stockReport.TotalProducts,
                ActiveProducts = stockReport.TotalProducts - stockReport.OutOfStockProducts,
                InactiveProducts = stockReport.OutOfStockProducts,
                TotalInventoryValue = stockReport.TotalInventoryValue,
                AverageProductValue = stockReport.TotalProducts > 0 ? stockReport.TotalInventoryValue / stockReport.TotalProducts : 0,
                LastUpdated = DateTime.Now
            };

            // Get critical stock items
            dashboard.CriticalStockItems = (await _stockService.GetLowStockProductsAsync()).Take(10).ToList();

            // Get expiring items
            dashboard.ExpiringItems = (await _expiryService.GetExpiringSoonProductsAsync()).Take(10).ToList();

            // Get category analytics
            var categories = await _categoryService.GetAllCategoriesAsync();
            dashboard.TopCategories = categories.Take(5).Select(c => new CategoryAnalyticsDto
            {
                Category = c.Name,
                ProductCount = c.ProductCount,
                TotalValue = 0, // Tạm thời set 0, sẽ tính sau nếu cần
                TotalStock = 0, // Tạm thời set 0, sẽ tính sau nếu cần
                Percentage = categories.Sum(cat => cat.ProductCount) > 0 ? 
                    (decimal)c.ProductCount / categories.Sum(cat => cat.ProductCount) * 100 : 0
            }).ToList();

            // Get pricing insights
            var pricingAnalysis = await _pricingService.GetPricingAnalysisAsync();
            dashboard.ProfitabilityInsights = new ProfitabilityAnalyticsDto
            {
                TotalPurchaseValue = pricingAnalysis.AveragePurchasePrice * stockReport.TotalProducts,
                TotalSellingValue = pricingAnalysis.AverageSellingPrice * stockReport.TotalProducts,
                AverageMarginPercent = pricingAnalysis.AverageMarginPercent,
                HighMarginProducts = pricingAnalysis.ProductsWithHighMargin,
                LowMarginProducts = pricingAnalysis.ProductsWithNegativeMargin,
                NegativeMarginProducts = pricingAnalysis.ProductsWithNegativeMargin
            };

            return Ok(dashboard);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting dashboard overview");
            return StatusCode(500, new { message = "Đã xảy ra lỗi khi lấy dữ liệu dashboard" });
        }
    }

    /// <summary>
    /// Lấy thống kê nhanh
    /// </summary>
    [HttpGet("quick-stats")]
    [Authorize(Roles = "Admin,Manager,Employee")]
    public async Task<ActionResult> GetQuickStats()
    {
        try
        {
            var stockReport = await _stockService.GetStockReportAsync();
            var expiryReport = await _expiryService.GetExpiryReportAsync();
            var categories = await _categoryService.GetAllCategoriesAsync();

            var quickStats = new
            {
                TotalProducts = stockReport.TotalProducts,
                LowStockProducts = stockReport.LowStockProducts,
                OutOfStockProducts = stockReport.OutOfStockProducts,
                ExpiredProducts = expiryReport.ExpiredProducts,
                ExpiringSoonProducts = expiryReport.ExpiringSoonProducts,
                TotalCategories = categories.Count,
                TotalInventoryValue = stockReport.TotalInventoryValue
            };

            return Ok(quickStats);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting quick stats");
            return StatusCode(500, new { message = "Đã xảy ra lỗi khi lấy thống kê nhanh" });
        }
    }

    /// <summary>
    /// Lấy cảnh báo và thông báo quan trọng
    /// </summary>
    [HttpGet("alerts")]
    [Authorize(Roles = "Admin,Manager,Employee")]
    public async Task<ActionResult> GetAlerts()
    {
        try
        {
            var alerts = new
            {
                StockAlerts = new
                {
                    LowStock = await _stockService.GetLowStockProductsAsync(),
                    OutOfStock = await _stockService.GetOutOfStockProductsAsync()
                },
                ExpiryAlerts = new
                {
                    Expired = await _expiryService.GetExpiredProductsAsync(),
                    ExpiringSoon = await _expiryService.GetExpiringSoonProductsAsync(),
                    SystemAlerts = await _expiryService.GetExpiryAlertsAsync()
                }
            };

            return Ok(alerts);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting alerts");
            return StatusCode(500, new { message = "Đã xảy ra lỗi khi lấy cảnh báo" });
        }
    }

    /// <summary>
    /// Lấy phân tích xu hướng
    /// </summary>
    [HttpGet("trends")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<ActionResult> GetTrends()
    {
        try
        {
            var categories = await _categoryService.GetAllCategoriesAsync();
            var pricingAnalysis = await _pricingService.GetPricingAnalysisAsync();

            var trends = new
            {
                CategoryTrends = categories.OrderByDescending(c => c.ProductCount).Take(5),
                PricingTrends = new
                {
                    AverageMargin = pricingAnalysis.AverageMarginPercent,
                    HighMarginProducts = pricingAnalysis.TopProfitableProducts.Take(5),
                    LowMarginProducts = pricingAnalysis.LowMarginProducts.Take(5)
                }
            };

            return Ok(trends);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting trends");
            return StatusCode(500, new { message = "Đã xảy ra lỗi khi lấy phân tích xu hướng" });
        }
    }
}
