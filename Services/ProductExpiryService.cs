using Microsoft.EntityFrameworkCore;
using WarehouseManage.Data;
using WarehouseManage.DTOs.Product;
using WarehouseManage.Interfaces;

namespace WarehouseManage.Services;

public class ProductExpiryService : IProductExpiryService
{
    private readonly WarehouseDbContext _context;
    private readonly ILogger<ProductExpiryService> _logger;

    public ProductExpiryService(WarehouseDbContext context, ILogger<ProductExpiryService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<List<ProductExpiryDto>> GetExpiryInfoAsync(ExpirySearchDto searchDto)
    {
        try
        {
            var query = _context.Products
                .Include(p => p.Inventories)
                .Include(p => p.Category)
                .AsQueryable();

            // Apply filters
            if (searchDto.Status.HasValue)
            {
                switch (searchDto.Status.Value)
                {
                    case ExpiryStatus.Expired:
                        query = query.Where(p => p.ExpiryDate.HasValue && p.ExpiryDate.Value < DateTime.Now);
                        break;
                    case ExpiryStatus.ExpiringSoon:
                        var soonDate = DateTime.Now.AddDays(7);
                        query = query.Where(p => p.ExpiryDate.HasValue && 
                            p.ExpiryDate.Value >= DateTime.Now && p.ExpiryDate.Value <= soonDate);
                        break;
                    case ExpiryStatus.ExpiringWithinMonth:
                        var monthDate = DateTime.Now.AddDays(30);
                        query = query.Where(p => p.ExpiryDate.HasValue && 
                            p.ExpiryDate.Value > DateTime.Now.AddDays(7) && p.ExpiryDate.Value <= monthDate);
                        break;
                    case ExpiryStatus.Fresh:
                        query = query.Where(p => p.ExpiryDate.HasValue && p.ExpiryDate.Value > DateTime.Now.AddDays(30));
                        break;
                    case ExpiryStatus.NoExpiryDate:
                        query = query.Where(p => !p.ExpiryDate.HasValue);
                        break;
                }
            }

            if (searchDto.ExpiryFromDate.HasValue)
                query = query.Where(p => p.ExpiryDate >= searchDto.ExpiryFromDate.Value);

            if (searchDto.ExpiryToDate.HasValue)
                query = query.Where(p => p.ExpiryDate <= searchDto.ExpiryToDate.Value);

            if (!string.IsNullOrEmpty(searchDto.Category))
                query = query.Where(p => p.Category != null && p.Category.Name == searchDto.Category);

            if (!string.IsNullOrEmpty(searchDto.StorageType))
                query = query.Where(p => p.StorageType == searchDto.StorageType);

            if (searchDto.IsPerishable.HasValue)
                query = query.Where(p => p.IsPerishable == searchDto.IsPerishable.Value);

            // Apply sorting
            switch (searchDto.SortBy?.ToLower())
            {
                case "expirydate":
                    query = searchDto.SortDescending 
                        ? query.OrderByDescending(p => p.ExpiryDate) 
                        : query.OrderBy(p => p.ExpiryDate);
                    break;
                case "productname":
                    query = searchDto.SortDescending 
                        ? query.OrderByDescending(p => p.ProductName) 
                        : query.OrderBy(p => p.ProductName);
                    break;
                default:
                    query = query.OrderBy(p => p.ExpiryDate);
                    break;
            }

            // Apply pagination
            var totalCount = await query.CountAsync();
            var products = await query
                .Skip((searchDto.Page - 1) * searchDto.PageSize)
                .Take(searchDto.PageSize)
                .Select(p => new ProductExpiryDto
                {
                    ProductId = p.ProductId,
                    Sku = p.Sku,
                    ProductName = p.ProductName,
                    Category = p.Category != null ? p.Category.Name : null,
                    ExpiryDate = p.ExpiryDate,
                    IsPerishable = p.IsPerishable,
                    StorageType = p.StorageType,
                    CurrentStock = p.Inventories.Sum(i => i.Quantity),
                    Unit = p.Unit  // Added Unit mapping
                })
                .ToListAsync();

            return products;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting expiry information");
            throw;
        }
    }

    public async Task<ExpiryReportDto> GetExpiryReportAsync()
    {
        try
        {
            var perishableProducts = await _context.Products
                .Include(p => p.Inventories)
                .Where(p => p.IsPerishable)
                .ToListAsync();

            var now = DateTime.Now;
            var expiredProducts = perishableProducts.Where(p => p.ExpiryDate.HasValue && p.ExpiryDate.Value < now).ToList();
            var expiringSoonProducts = perishableProducts.Where(p => p.ExpiryDate.HasValue && 
                p.ExpiryDate.Value >= now && p.ExpiryDate.Value <= now.AddDays(7)).ToList();
            var expiringWithinMonthProducts = perishableProducts.Where(p => p.ExpiryDate.HasValue && 
                p.ExpiryDate.Value > now.AddDays(7) && p.ExpiryDate.Value <= now.AddDays(30)).ToList();

            var report = new ExpiryReportDto
            {
                TotalPerishableProducts = perishableProducts.Count,
                ExpiredProducts = expiredProducts.Count,
                ExpiringSoonProducts = expiringSoonProducts.Count,
                ExpiringWithinMonthProducts = expiringWithinMonthProducts.Count,
                TotalExpiredValue = expiredProducts.Sum(p => (p.PurchasePrice ?? 0) * p.Inventories.Sum(i => i.Quantity)),
                TotalExpiringSoonValue = expiringSoonProducts.Sum(p => (p.PurchasePrice ?? 0) * p.Inventories.Sum(i => i.Quantity)),
                ExpiredItems = expiredProducts.Select(p => new ProductExpiryDto
                {
                    ProductId = p.ProductId,
                    Sku = p.Sku,
                    ProductName = p.ProductName,
                    Category = p.Category != null ? p.Category.Name : null,
                    ExpiryDate = p.ExpiryDate,
                    IsPerishable = p.IsPerishable,
                    StorageType = p.StorageType,
                    CurrentStock = p.Inventories.Sum(i => i.Quantity),
                    Unit = p.Unit  // Added Unit mapping
                }).ToList(),
                ExpiringSoonItems = expiringSoonProducts.Select(p => new ProductExpiryDto
                {
                    ProductId = p.ProductId,
                    Sku = p.Sku,
                    ProductName = p.ProductName,
                    Category = p.Category != null ? p.Category.Name : null,
                    ExpiryDate = p.ExpiryDate,
                    IsPerishable = p.IsPerishable,
                    StorageType = p.StorageType,
                    CurrentStock = p.Inventories.Sum(i => i.Quantity),
                    Unit = p.Unit  // Added Unit mapping
                }).ToList(),
                ExpiringWithinMonthItems = expiringWithinMonthProducts.Select(p => new ProductExpiryDto
                {
                    ProductId = p.ProductId,
                    Sku = p.Sku,
                    ProductName = p.ProductName,
                    Category = p.Category != null ? p.Category.Name : null,
                    ExpiryDate = p.ExpiryDate,
                    IsPerishable = p.IsPerishable,
                    StorageType = p.StorageType,
                    CurrentStock = p.Inventories.Sum(i => i.Quantity),
                    Unit = p.Unit  // Added Unit mapping
                }).ToList()
            };

            return report;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating expiry report");
            throw;
        }
    }

    public async Task<bool> UpdateExpiryInfoAsync(UpdateProductExpiryDto updateDto)
    {
        try
        {
            var product = await _context.Products
                .FirstOrDefaultAsync(p => p.ProductId == updateDto.ProductId);

            if (product == null)
                return false;

            product.ExpiryDate = updateDto.ExpiryDate;
            product.IsPerishable = updateDto.IsPerishable;
            product.StorageType = updateDto.StorageType;

            await _context.SaveChangesAsync();

            _logger.LogInformation("Expiry information updated for product {ProductId}", updateDto.ProductId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating expiry information for product {ProductId}", updateDto.ProductId);
            throw;
        }
    }

    public async Task<List<ExpiryAlertDto>> GetExpiryAlertsAsync()
    {
        try
        {
            var now = DateTime.Now;
            var alertDate = now.AddDays(7); // Alert for products expiring within 7 days

            var alerts = await _context.Products
                .Include(p => p.Inventories)
                .Include(p => p.Category)
                .Where(p => p.ExpiryDate.HasValue && p.ExpiryDate.Value <= alertDate && p.ExpiryDate.Value >= now)
                .Select(p => new ExpiryAlertDto
                {
                    ProductId = p.ProductId,
                    Sku = p.Sku,
                    ProductName = p.ProductName,
                    ExpiryDate = p.ExpiryDate,
                    CurrentStock = p.Inventories.Sum(i => i.Quantity),
                    Unit = p.Unit,  // Added Unit mapping
                    TotalValue = (p.PurchasePrice ?? 0) * p.Inventories.Sum(i => i.Quantity),
                    Status = p.ExpiryDate.HasValue && p.ExpiryDate.Value < now ? ExpiryStatus.Expired :
                             p.ExpiryDate.HasValue && p.ExpiryDate.Value <= now.AddDays(7) ? ExpiryStatus.ExpiringSoon :
                             ExpiryStatus.ExpiringWithinMonth,
                    DaysUntilExpiry = p.ExpiryDate.HasValue ? (int)(p.ExpiryDate.Value - now).TotalDays : int.MaxValue,
                    AlertCreatedAt = DateTime.Now,
                    AlertLevel = p.ExpiryDate.HasValue && p.ExpiryDate.Value <= now.AddDays(3) ? "Urgent" : "Warning"
                })
                .OrderBy(a => a.ExpiryDate)
                .ToListAsync();

            return alerts;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting expiry alerts");
            throw;
        }
    }

    public async Task<List<ProductExpiryDto>> GetExpiredProductsAsync()
    {
        try
        {
            var expiredProducts = await _context.Products
                .Include(p => p.Inventories)
                .Include(p => p.Category)
                .Where(p => p.ExpiryDate.HasValue && p.ExpiryDate.Value < DateTime.Now)
                .Select(p => new ProductExpiryDto
                {
                    ProductId = p.ProductId,
                    Sku = p.Sku,
                    ProductName = p.ProductName,
                    Category = p.Category != null ? p.Category.Name : null,
                    ExpiryDate = p.ExpiryDate,
                    IsPerishable = p.IsPerishable,
                    StorageType = p.StorageType,
                    CurrentStock = p.Inventories.Sum(i => i.Quantity),
                    Unit = p.Unit  // Added Unit mapping
                })
                .OrderBy(p => p.ExpiryDate)
                .ToListAsync();

            return expiredProducts;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting expired products");
            throw;
        }
    }

    public async Task<List<ProductExpiryDto>> GetExpiringSoonProductsAsync(int days = 7)
    {
        try
        {
            var now = DateTime.Now;
            var futureDate = now.AddDays(days);

            var expiringSoonProducts = await _context.Products
                .Include(p => p.Inventories)
                .Include(p => p.Category)
                .Where(p => p.ExpiryDate.HasValue && p.ExpiryDate.Value >= now && p.ExpiryDate.Value <= futureDate)
                .Select(p => new ProductExpiryDto
                {
                    ProductId = p.ProductId,
                    Sku = p.Sku,
                    ProductName = p.ProductName,
                    Category = p.Category != null ? p.Category.Name : null,
                    ExpiryDate = p.ExpiryDate,
                    IsPerishable = p.IsPerishable,
                    StorageType = p.StorageType,
                    CurrentStock = p.Inventories.Sum(i => i.Quantity),
                    Unit = p.Unit  // Added Unit mapping
                })
                .OrderBy(p => p.ExpiryDate)
                .ToListAsync();

            return expiringSoonProducts;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting expiring soon products");
            throw;
        }
    }
}
