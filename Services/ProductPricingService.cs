using Microsoft.EntityFrameworkCore;
using WarehouseManage.Data;
using WarehouseManage.DTOs.Product;
using WarehouseManage.Interfaces;

namespace WarehouseManage.Services;

public class ProductPricingService : IProductPricingService
{
    private readonly WarehouseDbContext _context;
    private readonly ILogger<ProductPricingService> _logger;

    public ProductPricingService(WarehouseDbContext context, ILogger<ProductPricingService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<List<ProductPricingDto>> GetAllPricingInfoAsync()
    {
        try
        {
            var pricingInfo = await _context.Products
                .Select(p => new ProductPricingDto
                {
                    ProductId = p.ProductId,
                    Sku = p.Sku,
                    ProductName = p.ProductName,
                    PurchasePrice = p.PurchasePrice,
                    SellingPrice = p.SellingPrice,
                    LastPriceUpdate = p.CreatedAt // In real app, you'd have a LastPriceUpdate field
                })
                .OrderBy(p => p.ProductName)
                .ToListAsync();

            return pricingInfo;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all pricing information");
            throw;
        }
    }

    public async Task<ProductPricingDto?> GetPricingInfoAsync(int productId)
    {
        try
        {
            var pricingInfo = await _context.Products
                .Where(p => p.ProductId == productId)
                .Select(p => new ProductPricingDto
                {
                    ProductId = p.ProductId,
                    Sku = p.Sku,
                    ProductName = p.ProductName,
                    PurchasePrice = p.PurchasePrice,
                    SellingPrice = p.SellingPrice,
                    LastPriceUpdate = p.CreatedAt
                })
                .FirstOrDefaultAsync();

            return pricingInfo;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting pricing information for product {ProductId}", productId);
            throw;
        }
    }

    public async Task<bool> UpdatePricingAsync(UpdateProductPricingDto updateDto)
    {
        try
        {
            var product = await _context.Products
                .FirstOrDefaultAsync(p => p.ProductId == updateDto.ProductId);

            if (product == null)
                return false;

            // Store old prices for history (in real app, you'd save to PriceHistory table)
            var oldPurchasePrice = product.PurchasePrice;
            var oldSellingPrice = product.SellingPrice;

            product.PurchasePrice = updateDto.PurchasePrice;
            product.SellingPrice = updateDto.SellingPrice;
            product.UpdatedAt = DateTime.UtcNow; // Cập nhật thời gian thay đổi

            await _context.SaveChangesAsync();

            _logger.LogInformation("Pricing updated for product {ProductId}. Purchase: {Old} -> {New}, Selling: {OldSell} -> {NewSell}", 
                updateDto.ProductId, oldPurchasePrice, updateDto.PurchasePrice, oldSellingPrice, updateDto.SellingPrice);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating pricing for product {ProductId}", updateDto.ProductId);
            throw;
        }
    }

    public async Task<BulkOperationResultDto> UpdatePricingBulkAsync(BulkUpdatePricingDto updateDto)
    {
        var result = new BulkOperationResultDto
        {
            Operation = BulkOperationType.UpdatePricing,
            TotalItems = updateDto.ProductIds.Count,
            ProcessedAt = DateTime.Now
        };

        var startTime = DateTime.Now;

        try
        {
            var products = await _context.Products
                .Where(p => updateDto.ProductIds.Contains(p.ProductId))
                .ToListAsync();

            foreach (var product in products)
            {
                try
                {
                    switch (updateDto.UpdateType)
                    {
                        case PriceUpdateType.SetPurchasePrice:
                            product.PurchasePrice = updateDto.Value;
                            break;
                        case PriceUpdateType.SetSellingPrice:
                            product.SellingPrice = updateDto.Value;
                            break;
                        case PriceUpdateType.IncreasePurchasePercent:
                            if (product.PurchasePrice.HasValue)
                                product.PurchasePrice = product.PurchasePrice.Value * (1 + updateDto.Value / 100);
                            break;
                        case PriceUpdateType.DecreasePurchasePercent:
                            if (product.PurchasePrice.HasValue)
                                product.PurchasePrice = product.PurchasePrice.Value * (1 - updateDto.Value / 100);
                            break;
                        case PriceUpdateType.IncreaseSellingPercent:
                            if (product.SellingPrice.HasValue)
                                product.SellingPrice = product.SellingPrice.Value * (1 + updateDto.Value / 100);
                            break;
                        case PriceUpdateType.DecreaseSellingPercent:
                            if (product.SellingPrice.HasValue)
                                product.SellingPrice = product.SellingPrice.Value * (1 - updateDto.Value / 100);
                            break;
                        case PriceUpdateType.SetMarginPercent:
                            if (product.PurchasePrice.HasValue && product.PurchasePrice.Value > 0)
                                product.SellingPrice = product.PurchasePrice.Value * (1 + updateDto.Value / 100);
                            break;
                    }

                    // Cập nhật thời gian thay đổi
                    product.UpdatedAt = DateTime.UtcNow;

                    result.SuccessCount++;
                }
                catch (Exception ex)
                {
                    result.FailureCount++;
                    result.Errors.Add(new BulkOperationError
                    {
                        ProductId = product.ProductId,
                        ProductName = product.ProductName,
                        Sku = product.Sku,
                        ErrorMessage = ex.Message
                    });
                }
            }

            await _context.SaveChangesAsync();
            result.ProcessingTime = DateTime.Now - startTime;

            _logger.LogInformation("Bulk pricing update completed. Success: {Success}, Failures: {Failures}", 
                result.SuccessCount, result.FailureCount);

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in bulk pricing update");
            throw;
        }
    }

    public async Task<List<PriceHistoryDto>> GetPriceHistoryAsync(int productId, int page = 1, int pageSize = 10)
    {
        try
        {
            // Tạm thời tạo fake history từ dữ liệu hiện tại
            // Trong thực tế, bạn sẽ cần một PriceHistory table riêng để lưu trữ lịch sử thay đổi
            var product = await _context.Products
                .FirstOrDefaultAsync(p => p.ProductId == productId);

            if (product == null)
            {
                return new List<PriceHistoryDto>();
            }

            var history = new List<PriceHistoryDto>();

            // Tạo entry hiện tại (giả lập là lần cập nhật gần nhất)
            if (product.UpdatedAt.HasValue)
            {
                history.Add(new PriceHistoryDto
                {
                    Id = 1,
                    ProductId = product.ProductId,
                    ProductName = product.ProductName,
                    Sku = product.Sku,
                    OldPurchasePrice = null, // Không có dữ liệu cũ
                    NewPurchasePrice = product.PurchasePrice,
                    OldSellingPrice = null, // Không có dữ liệu cũ  
                    NewSellingPrice = product.SellingPrice,
                    Reason = "Cập nhật giá sản phẩm",
                    ChangedBy = "System", // Trong thực tế sẽ lấy từ user context
                    ChangedAt = product.UpdatedAt.Value
                });
            }

            // Giả lập thêm một vài entry cũ hơn (demo purpose)
            if (product.CreatedAt.HasValue)
            {
                history.Add(new PriceHistoryDto
                {
                    Id = 2,
                    ProductId = product.ProductId,
                    ProductName = product.ProductName,
                    Sku = product.Sku,
                    OldPurchasePrice = null,
                    NewPurchasePrice = product.PurchasePrice * 0.9m, // Giả lập giá cũ thấp hơn 10%
                    OldSellingPrice = null,
                    NewSellingPrice = product.SellingPrice * 0.9m,
                    Reason = "Tạo sản phẩm mới",
                    ChangedBy = "Admin",
                    ChangedAt = product.CreatedAt.Value
                });
            }

            // Sắp xếp theo thời gian giảm dần và phân trang
            var result = history
                .OrderByDescending(h => h.ChangedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting price history for product {ProductId}", productId);
            throw;
        }
    }

    public async Task<PricingAnalysisDto> GetPricingAnalysisAsync()
    {
        try
        {
            var allProducts = await _context.Products
                .Include(p => p.Inventories)
                .ToListAsync();

            var analysis = new PricingAnalysisDto
            {
                TotalProducts = allProducts.Count,
                ProductsWithoutPurchasePrice = allProducts.Count(p => !p.PurchasePrice.HasValue),
                ProductsWithoutSellingPrice = allProducts.Count(p => !p.SellingPrice.HasValue),
                ProductsWithNegativeMargin = allProducts.Count(p => 
                    p.PurchasePrice.HasValue && p.SellingPrice.HasValue && p.SellingPrice.Value < p.PurchasePrice.Value),
                ProductsWithHighMargin = allProducts.Count(p => 
                    p.PurchasePrice.HasValue && p.SellingPrice.HasValue && p.PurchasePrice.Value > 0 &&
                    ((p.SellingPrice.Value - p.PurchasePrice.Value) / p.PurchasePrice.Value) > 0.5m),
                AveragePurchasePrice = allProducts.Where(p => p.PurchasePrice.HasValue).Any() 
                    ? allProducts.Where(p => p.PurchasePrice.HasValue).Average(p => p.PurchasePrice!.Value) 
                    : 0,
                AverageSellingPrice = allProducts.Where(p => p.SellingPrice.HasValue).Any() 
                    ? allProducts.Where(p => p.SellingPrice.HasValue).Average(p => p.SellingPrice!.Value) 
                    : 0
            };

            // Calculate average margin
            var productsWithBothPrices = allProducts.Where(p => 
                p.PurchasePrice.HasValue && p.SellingPrice.HasValue && p.PurchasePrice.Value > 0).ToList();
            
            if (productsWithBothPrices.Any())
            {
                analysis.AverageMarginPercent = productsWithBothPrices.Average(p => 
                    ((p.SellingPrice!.Value - p.PurchasePrice!.Value) / p.PurchasePrice!.Value) * 100);
            }

            // Get top profitable products
            analysis.TopProfitableProducts = productsWithBothPrices
                .Select(p => new ProductPricingDto
                {
                    ProductId = p.ProductId,
                    Sku = p.Sku,
                    ProductName = p.ProductName,
                    PurchasePrice = p.PurchasePrice,
                    SellingPrice = p.SellingPrice
                })
                .OrderByDescending(p => p.ProfitAmount)
                .Take(10)
                .ToList();

            // Get low margin products
            analysis.LowMarginProducts = productsWithBothPrices
                .Where(p => ((p.SellingPrice!.Value - p.PurchasePrice!.Value) / p.PurchasePrice!.Value) < 0.1m)
                .Select(p => new ProductPricingDto
                {
                    ProductId = p.ProductId,
                    Sku = p.Sku,
                    ProductName = p.ProductName,
                    PurchasePrice = p.PurchasePrice,
                    SellingPrice = p.SellingPrice
                })
                .OrderBy(p => p.ProfitMargin)
                .Take(10)
                .ToList();

            return analysis;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating pricing analysis");
            throw;
        }
    }
}
