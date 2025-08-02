using Microsoft.EntityFrameworkCore;
using WarehouseManage.Data;
using WarehouseManage.DTOs.Product;
using WarehouseManage.Interfaces;
using WarehouseManage.Model;

namespace WarehouseManage.Repositories;

public class ProductRepository : IProductRepository
{
    private readonly WarehouseDbContext _context;

    public ProductRepository(WarehouseDbContext context)
    {
        _context = context;
    }

    public async Task<Product?> GetByIdAsync(int productId)
    {
        return await _context.Products
            .Include(p => p.Supplier)
            .Include(p => p.Inventories)
            .Include(p => p.GoodsReceiptDetails)
            .Include(p => p.GoodsIssueDetails)
            .FirstOrDefaultAsync(p => p.ProductId == productId);
    }

    public async Task<ProductListResponseDto> GetAllAsync(ProductSearchDto searchDto)
    {
        var query = _context.Products
            .Include(p => p.Supplier)
            .AsQueryable();

        // Apply search filters
        if (!string.IsNullOrWhiteSpace(searchDto.SearchTerm))
        {
            var searchTerm = searchDto.SearchTerm.ToLower();
            query = query.Where(p => 
                p.ProductName.ToLower().Contains(searchTerm) ||
                p.Sku.ToLower().Contains(searchTerm) ||
                (p.Description != null && p.Description.ToLower().Contains(searchTerm)));
        }

        if (!string.IsNullOrWhiteSpace(searchDto.Sku))
        {
            query = query.Where(p => p.Sku.ToLower().Contains(searchDto.Sku.ToLower()));
        }

        if (searchDto.SupplierId.HasValue)
        {
            query = query.Where(p => p.SupplierId == searchDto.SupplierId.Value);
        }

        if (!string.IsNullOrWhiteSpace(searchDto.Unit))
        {
            query = query.Where(p => p.Unit == searchDto.Unit);
        }

        if (searchDto.MinPrice.HasValue)
        {
            query = query.Where(p => p.SellingPrice >= searchDto.MinPrice.Value);
        }

        if (searchDto.MaxPrice.HasValue)
        {
            query = query.Where(p => p.SellingPrice <= searchDto.MaxPrice.Value);
        }

        if (searchDto.Status.HasValue)
        {
            query = query.Where(p => p.Status == searchDto.Status.Value);
        }

        // Get total count before pagination
        var totalCount = await query.CountAsync();

        // Apply sorting
        query = searchDto.SortBy?.ToLower() switch
        {
            "sku" => searchDto.SortDescending 
                ? query.OrderByDescending(p => p.Sku) 
                : query.OrderBy(p => p.Sku),
            "productname" => searchDto.SortDescending 
                ? query.OrderByDescending(p => p.ProductName) 
                : query.OrderBy(p => p.ProductName),
            "purchaseprice" => searchDto.SortDescending 
                ? query.OrderByDescending(p => p.PurchasePrice) 
                : query.OrderBy(p => p.PurchasePrice),
            "sellingprice" => searchDto.SortDescending 
                ? query.OrderByDescending(p => p.SellingPrice) 
                : query.OrderBy(p => p.SellingPrice),
            "createdat" => searchDto.SortDescending 
                ? query.OrderByDescending(p => p.CreatedAt) 
                : query.OrderBy(p => p.CreatedAt),
            _ => searchDto.SortDescending 
                ? query.OrderByDescending(p => p.ProductName) 
                : query.OrderBy(p => p.ProductName)
        };

        // Apply pagination
        var skip = (searchDto.Page - 1) * searchDto.PageSize;
        var products = await query
            .Skip(skip)
            .Take(searchDto.PageSize)
            .Select(p => new ProductDto
            {
                ProductId = p.ProductId,
                Sku = p.Sku,
                ProductName = p.ProductName,
                Description = p.Description,
                SupplierId = p.SupplierId,
                SupplierName = p.Supplier != null ? p.Supplier.SupplierName : null,
                Unit = p.Unit,
                PurchasePrice = p.PurchasePrice,
                SellingPrice = p.SellingPrice,
                ImageUrl = p.ImageUrl,
                Status = p.Status,
                CreatedAt = p.CreatedAt,
                CurrentStock = p.Inventories.Sum(i => i.Quantity),
                TotalReceived = p.GoodsReceiptDetails.Sum(grd => grd.Quantity),
                TotalIssued = p.GoodsIssueDetails.Sum(gid => gid.Quantity),
                TotalValue = (p.PurchasePrice ?? 0) * p.Inventories.Sum(i => i.Quantity)
            })
            .ToListAsync();

        var totalPages = (int)Math.Ceiling((double)totalCount / searchDto.PageSize);

        return new ProductListResponseDto
        {
            Products = products,
            TotalCount = totalCount,
            Page = searchDto.Page,
            PageSize = searchDto.PageSize,
            TotalPages = totalPages
        };
    }

    public async Task<Product> CreateAsync(Product product)
    {
        product.CreatedAt = DateTime.UtcNow;
        _context.Products.Add(product);
        await _context.SaveChangesAsync();
        return product;
    }

    public async Task<Product?> UpdateAsync(int productId, Product product)
    {
        var existingProduct = await _context.Products.FindAsync(productId);
        if (existingProduct == null)
            return null;

        // Update properties
        existingProduct.Sku = product.Sku;
        existingProduct.ProductName = product.ProductName;
        existingProduct.Description = product.Description;
        existingProduct.SupplierId = product.SupplierId;
        existingProduct.Unit = product.Unit;
        existingProduct.PurchasePrice = product.PurchasePrice;
        existingProduct.SellingPrice = product.SellingPrice;
        existingProduct.ImageUrl = product.ImageUrl;
        existingProduct.Status = product.Status;

        await _context.SaveChangesAsync();
        return existingProduct;
    }

    public async Task<bool> DeleteAsync(int productId)
    {
        var product = await _context.Products.FindAsync(productId);
        if (product == null)
            return false;

        // Always soft delete by setting status to false (same as supplier logic)
        product.Status = false;
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> ReactivateAsync(int productId)
    {
        var product = await _context.Products.FindAsync(productId);
        if (product == null || product.Status == true)
            return false;

        // Reactivate the product by setting status to true
        product.Status = true;
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> ExistsAsync(int productId)
    {
        return await _context.Products.AnyAsync(p => p.ProductId == productId);
    }

    public async Task<bool> ExistsBySkuAsync(string sku, int? excludeId = null)
    {
        var query = _context.Products.Where(p => p.Sku == sku);
        
        if (excludeId.HasValue)
        {
            query = query.Where(p => p.ProductId != excludeId.Value);
        }

        return await query.AnyAsync();
    }

    public async Task<Product?> GetBySkuAsync(string sku)
    {
        return await _context.Products
            .Include(p => p.Supplier)
            .FirstOrDefaultAsync(p => p.Sku == sku);
    }

    public async Task<ProductStatsDto?> GetProductStatsAsync(int productId)
    {
        var product = await _context.Products
            .Include(p => p.Inventories)
            .Include(p => p.GoodsReceiptDetails)
                .ThenInclude(grd => grd.GoodsReceipt)
            .Include(p => p.GoodsIssueDetails)
                .ThenInclude(gid => gid.GoodsIssue)
            .FirstOrDefaultAsync(p => p.ProductId == productId);

        if (product == null)
            return null;

        var currentStock = product.Inventories.Sum(i => i.Quantity);
        var totalReceived = product.GoodsReceiptDetails.Sum(grd => grd.Quantity);
        var totalIssued = product.GoodsIssueDetails.Sum(gid => gid.Quantity);

        var monthlyMovements = product.GoodsReceiptDetails
            .Where(grd => grd.GoodsReceipt != null && grd.GoodsReceipt.ReceiptDate.HasValue)
            .GroupBy(grd => new { 
                Year = grd.GoodsReceipt!.ReceiptDate!.Value.Year,
                Month = grd.GoodsReceipt.ReceiptDate.Value.Month 
            })
            .Select(g => new MonthlyMovementDto
            {
                Year = g.Key.Year,
                Month = g.Key.Month,
                ReceivedQuantity = g.Sum(grd => grd.Quantity),
                ReceivedValue = g.Sum(grd => grd.Quantity * grd.UnitPrice)
            })
            .ToList();

        // Add issue data to monthly movements
        var monthlyIssues = product.GoodsIssueDetails
            .Where(gid => gid.GoodsIssue != null && gid.GoodsIssue.IssueDate.HasValue)
            .GroupBy(gid => new { 
                Year = gid.GoodsIssue!.IssueDate!.Value.Year,
                Month = gid.GoodsIssue.IssueDate.Value.Month 
            })
            .ToList();

        foreach (var issueGroup in monthlyIssues)
        {
            var existingMovement = monthlyMovements
                .FirstOrDefault(m => m.Year == issueGroup.Key.Year && m.Month == issueGroup.Key.Month);
            
            if (existingMovement != null)
            {
                existingMovement.IssuedQuantity = issueGroup.Sum(gid => gid.Quantity);
                existingMovement.IssuedValue = issueGroup.Sum(gid => gid.Quantity * gid.UnitPrice);
            }
            else
            {
                monthlyMovements.Add(new MonthlyMovementDto
                {
                    Year = issueGroup.Key.Year,
                    Month = issueGroup.Key.Month,
                    IssuedQuantity = issueGroup.Sum(gid => gid.Quantity),
                    IssuedValue = issueGroup.Sum(gid => gid.Quantity * gid.UnitPrice)
                });
            }
        }

        return new ProductStatsDto
        {
            ProductId = product.ProductId,
            ProductName = product.ProductName,
            Sku = product.Sku,
            CurrentStock = currentStock,
            TotalReceived = totalReceived,
            TotalIssued = totalIssued,
            TotalPurchaseValue = product.GoodsReceiptDetails.Sum(grd => grd.Quantity * grd.UnitPrice),
            TotalSaleValue = product.GoodsIssueDetails.Sum(gid => gid.Quantity * gid.UnitPrice),
            LastReceiptDate = product.GoodsReceiptDetails
                .Where(grd => grd.GoodsReceipt != null)
                .Max(grd => grd.GoodsReceipt!.ReceiptDate),
            LastIssueDate = product.GoodsIssueDetails
                .Where(gid => gid.GoodsIssue != null)
                .Max(gid => gid.GoodsIssue!.IssueDate),
            MonthlyMovements = monthlyMovements.OrderBy(m => m.Year).ThenBy(m => m.Month).ToList()
        };
    }

    public async Task<List<ProductDto>> GetTopProductsAsync(int count = 5)
    {
        return await _context.Products
            .Include(p => p.Supplier)
            .Include(p => p.Inventories)
            .Include(p => p.GoodsIssueDetails)
            .Where(p => p.Status == true)
            .OrderByDescending(p => p.GoodsIssueDetails.Sum(gid => gid.Quantity))
            .Take(count)
            .Select(p => new ProductDto
            {
                ProductId = p.ProductId,
                Sku = p.Sku,
                ProductName = p.ProductName,
                Description = p.Description,
                SupplierId = p.SupplierId,
                SupplierName = p.Supplier != null ? p.Supplier.SupplierName : null,
                Unit = p.Unit,
                PurchasePrice = p.PurchasePrice,
                SellingPrice = p.SellingPrice,
                ImageUrl = p.ImageUrl,
                Status = p.Status,
                CreatedAt = p.CreatedAt,
                CurrentStock = p.Inventories.Sum(i => i.Quantity),
                TotalIssued = p.GoodsIssueDetails.Sum(gid => gid.Quantity)
            })
            .ToListAsync();
    }

    public async Task<List<ProductInventoryDto>> GetLowStockProductsAsync()
    {
        // For now, consider products with stock < 10 as low stock
        // In a real implementation, this would be configurable per product
        return await _context.Inventories
            .Include(i => i.Product)
            .Where(i => i.Quantity < 10)
            .Select(i => new ProductInventoryDto
            {
                ProductId = i.ProductId,
                ProductName = i.Product != null ? i.Product.ProductName : "",
                Sku = i.Product != null ? i.Product.Sku : "",
                Unit = i.Product != null ? i.Product.Unit : null,
                CurrentStock = i.Quantity,
                MinStock = 10,
                MaxStock = 100,
                IsLowStock = true,
                LastUpdated = i.LastUpdatedAt
            })
            .ToListAsync();
    }

    public async Task<bool> HasInventoryMovementsAsync(int productId)
    {
        var hasReceipts = await _context.GoodsReceiptDetails.AnyAsync(grd => grd.ProductId == productId);
        var hasIssues = await _context.GoodsIssueDetails.AnyAsync(gid => gid.ProductId == productId);
        
        return hasReceipts || hasIssues;
    }

    public async Task<List<ProductInventoryDto>> GetProductInventoryAsync(int productId)
    {
        return await _context.Inventories
            .Include(i => i.Product)
            .Where(i => i.ProductId == productId)
            .Select(i => new ProductInventoryDto
            {
                ProductId = i.ProductId,
                ProductName = i.Product != null ? i.Product.ProductName : "",
                Sku = i.Product != null ? i.Product.Sku : "",
                Unit = i.Product != null ? i.Product.Unit : null,
                CurrentStock = i.Quantity,
                MinStock = 10,
                MaxStock = 100,
                IsLowStock = i.Quantity < 10,
                LastUpdated = i.LastUpdatedAt
            })
            .ToListAsync();
    }

    public async Task<List<ProductDto>> GetProductsBySupplierId(int supplierId)
    {
        return await _context.Products
            .Include(p => p.Supplier)
            .Include(p => p.Inventories)
            .Where(p => p.SupplierId == supplierId)
            .Select(p => new ProductDto
            {
                ProductId = p.ProductId,
                Sku = p.Sku,
                ProductName = p.ProductName,
                Description = p.Description,
                SupplierId = p.SupplierId,
                SupplierName = p.Supplier != null ? p.Supplier.SupplierName : null,
                Unit = p.Unit,
                PurchasePrice = p.PurchasePrice,
                SellingPrice = p.SellingPrice,
                ImageUrl = p.ImageUrl,
                Status = p.Status,
                CreatedAt = p.CreatedAt,
                CurrentStock = p.Inventories.Sum(i => i.Quantity)
            })
            .ToListAsync();
    }

    public async Task<List<ProductDto>> GetActiveProductsAsync()
    {
        return await _context.Products
            .Include(p => p.Supplier)
            .Include(p => p.Inventories)
            .Where(p => p.Status == true)
            .Select(p => new ProductDto
            {
                ProductId = p.ProductId,
                Sku = p.Sku,
                ProductName = p.ProductName,
                Description = p.Description,
                SupplierId = p.SupplierId,
                SupplierName = p.Supplier != null ? p.Supplier.SupplierName : null,
                Unit = p.Unit,
                PurchasePrice = p.PurchasePrice,
                SellingPrice = p.SellingPrice,
                ImageUrl = p.ImageUrl,
                Status = p.Status,
                CreatedAt = p.CreatedAt,
                CurrentStock = p.Inventories.Sum(i => i.Quantity)
            })
            .OrderBy(p => p.ProductName)
            .ToListAsync();
    }
}
