using Microsoft.EntityFrameworkCore;
using WarehouseManage.Data;
using WarehouseManage.DTOs.Product;
using WarehouseManage.Interfaces;
using WarehouseManage.Model;

namespace WarehouseManage.Services;

public class ProductStockService : IProductStockService
{
    private readonly WarehouseDbContext _context;
    private readonly ILogger<ProductStockService> _logger;

    public ProductStockService(WarehouseDbContext context, ILogger<ProductStockService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<List<ProductStockDto>> GetAllStockInfoAsync()
    {
        try
        {
            var stockInfo = await _context.Products
                .Include(p => p.Inventories)
                .Include(p => p.Category)
                .Select(p => new ProductStockDto
                {
                    ProductId = p.ProductId,
                    Sku = p.Sku,
                    ProductName = p.ProductName,
                    Category = p.Category != null ? p.Category.Name : null,
                    CurrentStock = p.Inventories.Sum(i => i.Quantity),
                    MinStockLevel = p.MinStockLevel,
                    MaxStockLevel = p.MaxStockLevel,
                    PurchasePrice = p.PurchasePrice
                })
                .OrderBy(p => p.ProductName)
                .ToListAsync();

            return stockInfo;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all stock info");
            throw;
        }
    }

    public async Task<ProductStockDto?> GetStockInfoAsync(int productId)
    {
        try
        {
            var stockInfo = await _context.Products
                .Include(p => p.Inventories)
                .Include(p => p.Category)
                .Where(p => p.ProductId == productId)
                .Select(p => new ProductStockDto
                {
                    ProductId = p.ProductId,
                    Sku = p.Sku,
                    ProductName = p.ProductName,
                    Category = p.Category != null ? p.Category.Name : null,
                    CurrentStock = p.Inventories.Sum(i => i.Quantity),
                    MinStockLevel = p.MinStockLevel,
                    MaxStockLevel = p.MaxStockLevel,
                    PurchasePrice = p.PurchasePrice
                })
                .FirstOrDefaultAsync();

            return stockInfo;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting stock info for product {ProductId}", productId);
            throw;
        }
    }

    public async Task<StockReportDto> GetStockReportAsync()
    {
        try
        {
            var products = await _context.Products
                .Include(p => p.Inventories)
                .ToListAsync();

            var report = new StockReportDto
            {
                TotalProducts = products.Count,
                LowStockProducts = products.Count(p => p.MinStockLevel.HasValue && 
                    p.Inventories.Sum(i => i.Quantity) <= p.MinStockLevel.Value),
                OutOfStockProducts = products.Count(p => p.Inventories.Sum(i => i.Quantity) == 0),
                TotalInventoryValue = products.Sum(p => (p.PurchasePrice ?? 0) * p.Inventories.Sum(i => i.Quantity))
            };

            return report;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting stock report");
            throw;
        }
    }

    public async Task<bool> UpdateStockLevelsAsync(UpdateStockLevelsDto updateDto)
    {
        try
        {
            var product = await _context.Products.FindAsync(updateDto.ProductId);
            if (product == null)
                return false;

            product.MinStockLevel = updateDto.MinStockLevel;
            product.MaxStockLevel = updateDto.MaxStockLevel;
            product.UpdatedAt = DateTime.Now;

            await _context.SaveChangesAsync();
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating stock levels for product {ProductId}", updateDto.ProductId);
            throw;
        }
    }

    public async Task<bool> AdjustStockAsync(StockAdjustmentDto adjustmentDto)
    {
        try
        {
            var product = await _context.Products
                .Include(p => p.Inventories)
                .FirstOrDefaultAsync(p => p.ProductId == adjustmentDto.ProductId);

            if (product == null)
                return false;

            var inventory = new Inventory
            {
                ProductId = adjustmentDto.ProductId,
                Quantity = adjustmentDto.Quantity
            };

            _context.Inventories.Add(inventory);
            await _context.SaveChangesAsync();
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adjusting stock for product {ProductId}", adjustmentDto.ProductId);
            throw;
        }
    }

    public Task<List<StockHistoryDto>> GetStockHistoryAsync(int productId, int page = 1, int pageSize = 10)
    {
        throw new NotImplementedException();
    }

    public async Task<List<ProductStockDto>> GetLowStockProductsAsync()
    {
        try
        {
            var lowStockProducts = await _context.Products
                .Include(p => p.Inventories)
                .Include(p => p.Category)
                .Where(p => p.MinStockLevel.HasValue && p.Inventories.Sum(i => i.Quantity) <= p.MinStockLevel.Value)
                .Select(p => new ProductStockDto
                {
                    ProductId = p.ProductId,
                    Sku = p.Sku,
                    ProductName = p.ProductName,
                    Category = p.Category != null ? p.Category.Name : null,
                    CurrentStock = p.Inventories.Sum(i => i.Quantity),
                    MinStockLevel = p.MinStockLevel,
                    PurchasePrice = p.PurchasePrice
                })
                .OrderBy(p => p.CurrentStock)
                .ToListAsync();

            return lowStockProducts;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting low stock products");
            throw;
        }
    }

    public async Task<List<ProductStockDto>> GetOutOfStockProductsAsync()
    {
        try
        {
            var outOfStockProducts = await _context.Products
                .Include(p => p.Inventories)
                .Include(p => p.Category)
                .Where(p => p.Inventories.Sum(i => i.Quantity) == 0)
                .Select(p => new ProductStockDto
                {
                    ProductId = p.ProductId,
                    Sku = p.Sku,
                    ProductName = p.ProductName,
                    Category = p.Category != null ? p.Category.Name : null,
                    CurrentStock = 0,
                    MinStockLevel = p.MinStockLevel,
                    PurchasePrice = p.PurchasePrice
                })
                .OrderBy(p => p.ProductName)
                .ToListAsync();

            return outOfStockProducts;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting out of stock products");
            throw;
        }
    }
}