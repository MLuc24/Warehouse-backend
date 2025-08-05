using System.ComponentModel.DataAnnotations;

namespace WarehouseManage.DTOs.Product;

/// <summary>
/// DTO cho báo cáo và thống kê sản phẩm
/// </summary>
public class ProductAnalyticsDto
{
    public ProductOverviewDto Overview { get; set; } = new();
    public List<CategoryAnalyticsDto> CategoryAnalytics { get; set; } = new();
    public List<SupplierAnalyticsDto> SupplierAnalytics { get; set; } = new();
    public StockAnalyticsDto StockAnalytics { get; set; } = new();
    public ProfitabilityAnalyticsDto ProfitabilityAnalytics { get; set; } = new();
    public List<ProductPerformanceDto> TopPerformingProducts { get; set; } = new();
    public List<ProductPerformanceDto> LowPerformingProducts { get; set; } = new();
}

/// <summary>
/// DTO cho tổng quan sản phẩm
/// </summary>
public class ProductOverviewDto
{
    public int TotalProducts { get; set; }
    public int ActiveProducts { get; set; }
    public int InactiveProducts { get; set; }
    public int TotalCategories { get; set; }
    public int TotalSuppliers { get; set; }
    public decimal TotalInventoryValue { get; set; }
    public decimal AverageProductValue { get; set; }
    public DateTime LastUpdated { get; set; }
}

/// <summary>
/// DTO cho phân tích theo danh mục
/// </summary>
public class CategoryAnalyticsDto
{
    public string Category { get; set; } = null!;
    public int ProductCount { get; set; }
    public int ActiveProducts { get; set; }
    public int InactiveProducts { get; set; }
    public decimal TotalValue { get; set; }
    public decimal AveragePrice { get; set; }
    public int TotalStock { get; set; }
    public int LowStockProducts { get; set; }
    public decimal Percentage { get; set; }
}

/// <summary>
/// DTO cho phân tích theo nhà cung cấp
/// </summary>
public class SupplierAnalyticsDto
{
    public int SupplierId { get; set; }
    public string SupplierName { get; set; } = null!;
    public int ProductCount { get; set; }
    public decimal TotalValue { get; set; }
    public decimal AveragePrice { get; set; }
    public int TotalStock { get; set; }
    public decimal Percentage { get; set; }
    public List<string> TopCategories { get; set; } = new();
}

/// <summary>
/// DTO cho phân tích tồn kho
/// </summary>
public class StockAnalyticsDto
{
    public int TotalProducts { get; set; }
    public int InStockProducts { get; set; }
    public int OutOfStockProducts { get; set; }
    public int LowStockProducts { get; set; }
    public int OverStockProducts { get; set; }
    public decimal TotalStockValue { get; set; }
    public decimal AverageStockLevel { get; set; }
    public decimal StockTurnoverRatio { get; set; }
}

/// <summary>
/// DTO cho phân tích lợi nhuận
/// </summary>
public class ProfitabilityAnalyticsDto
{
    public decimal TotalPurchaseValue { get; set; }
    public decimal TotalSellingValue { get; set; }
    public decimal TotalPotentialProfit { get; set; }
    public decimal AverageMarginPercent { get; set; }
    public int HighMarginProducts { get; set; }  // > 30%
    public int LowMarginProducts { get; set; }   // < 10%
    public int NegativeMarginProducts { get; set; }
}

/// <summary>
/// DTO cho hiệu suất sản phẩm
/// </summary>
public class ProductPerformanceDto
{
    public int ProductId { get; set; }
    public string Sku { get; set; } = null!;
    public string ProductName { get; set; } = null!;
    public string? Category { get; set; }
    public int CurrentStock { get; set; }
    public decimal? PurchasePrice { get; set; }
    public decimal? SellingPrice { get; set; }
    public decimal? MarginPercent { get; set; }
    public decimal TotalValue { get; set; }
    public int TotalTransactions { get; set; }
    public decimal TurnoverRate { get; set; }
    public DateTime? LastTransactionDate { get; set; }
}

/// <summary>
/// DTO cho tham số báo cáo
/// </summary>
public class ReportParametersDto
{
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
    public List<string>? Categories { get; set; }
    public List<int>? SupplierIds { get; set; }
    public bool? Status { get; set; }
    public decimal? MinValue { get; set; }
    public decimal? MaxValue { get; set; }
    public ReportType ReportType { get; set; } = ReportType.Overview;
}

/// <summary>
/// Enum cho loại báo cáo
/// </summary>
public enum ReportType
{
    Overview,           // Tổng quan
    CategoryAnalysis,   // Phân tích theo danh mục
    SupplierAnalysis,   // Phân tích theo nhà cung cấp
    StockAnalysis,      // Phân tích tồn kho
    ProfitabilityAnalysis, // Phân tích lợi nhuận
    PerformanceAnalysis    // Phân tích hiệu suất
}

/// <summary>
/// DTO cho dashboard sản phẩm
/// </summary>
public class ProductDashboardDto
{
    public ProductOverviewDto Overview { get; set; } = new();
    public List<ProductStockDto> CriticalStockItems { get; set; } = new();
    public List<ProductExpiryDto> ExpiringItems { get; set; } = new();
    public List<ProductPerformanceDto> TopMovingProducts { get; set; } = new();
    public List<CategoryAnalyticsDto> TopCategories { get; set; } = new();
    public ProfitabilityAnalyticsDto ProfitabilityInsights { get; set; } = new();
    public List<RecentActivityDto> RecentActivities { get; set; } = new();
}

/// <summary>
/// DTO cho hoạt động gần đây
/// </summary>
public class RecentActivityDto
{
    public string ActivityType { get; set; } = null!;
    public string Description { get; set; } = null!;
    public int ProductId { get; set; }
    public string ProductName { get; set; } = null!;
    public string UserName { get; set; } = null!;
    public DateTime Timestamp { get; set; }
}
