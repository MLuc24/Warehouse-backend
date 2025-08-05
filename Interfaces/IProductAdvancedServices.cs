using WarehouseManage.DTOs.Product;

namespace WarehouseManage.Interfaces;

/// <summary>
/// Interface cho dịch vụ quản lý tồn kho sản phẩm
/// </summary>
public interface IProductStockService
{
    Task<List<ProductStockDto>> GetAllStockInfoAsync();
    Task<ProductStockDto?> GetStockInfoAsync(int productId);
    Task<StockReportDto> GetStockReportAsync();
    Task<bool> UpdateStockLevelsAsync(UpdateStockLevelsDto updateDto);
    Task<bool> AdjustStockAsync(StockAdjustmentDto adjustmentDto);
    Task<List<StockHistoryDto>> GetStockHistoryAsync(int productId, int page = 1, int pageSize = 10);
    Task<List<ProductStockDto>> GetLowStockProductsAsync();
    Task<List<ProductStockDto>> GetOutOfStockProductsAsync();
}

/// <summary>
/// Interface cho dịch vụ quản lý giá sản phẩm
/// </summary>
public interface IProductPricingService
{
    Task<List<ProductPricingDto>> GetAllPricingInfoAsync();
    Task<ProductPricingDto?> GetPricingInfoAsync(int productId);
    Task<bool> UpdatePricingAsync(UpdateProductPricingDto updateDto);
    Task<BulkOperationResultDto> UpdatePricingBulkAsync(BulkUpdatePricingDto updateDto);
    Task<List<PriceHistoryDto>> GetPriceHistoryAsync(int productId, int page = 1, int pageSize = 10);
    Task<PricingAnalysisDto> GetPricingAnalysisAsync();
}

/// <summary>
/// Interface cho dịch vụ quản lý hạn sử dụng sản phẩm
/// </summary>
public interface IProductExpiryService
{
    Task<List<ProductExpiryDto>> GetExpiryInfoAsync(ExpirySearchDto searchDto);
    Task<ExpiryReportDto> GetExpiryReportAsync();
    Task<bool> UpdateExpiryInfoAsync(UpdateProductExpiryDto updateDto);
    Task<List<ExpiryAlertDto>> GetExpiryAlertsAsync();
    Task<List<ProductExpiryDto>> GetExpiredProductsAsync();
    Task<List<ProductExpiryDto>> GetExpiringSoonProductsAsync(int days = 7);
}

/// <summary>
/// Interface cho dịch vụ thao tác hàng loạt
/// </summary>
public interface IProductBulkService
{
    Task<BulkOperationResultDto> ExecuteBulkOperationAsync(BulkProductOperationDto operationDto);
    Task<ProductImportResultDto> ImportProductsAsync(ProductImportDto importDto);
    Task<byte[]> ExportProductsAsync(ProductExportDto exportDto);
    Task<BulkOperationResultDto> GenerateBarcodesBulkAsync(BulkBarcodeGenerationDto barcodeDto);
}

/// <summary>
/// Interface cho dịch vụ phân tích và báo cáo
/// </summary>
public interface IProductAnalyticsService
{
    Task<ProductAnalyticsDto> GetProductAnalyticsAsync(ReportParametersDto parameters);
    Task<ProductDashboardDto> GetDashboardDataAsync();
    Task<List<ProductPerformanceDto>> GetTopPerformingProductsAsync(int count = 10);
    Task<List<ProductPerformanceDto>> GetLowPerformingProductsAsync(int count = 10);
    Task<byte[]> GenerateAnalyticsReportAsync(ReportParametersDto parameters, ExportFormat format);
}
