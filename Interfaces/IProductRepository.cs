using WarehouseManage.DTOs.Product;
using WarehouseManage.Model;

namespace WarehouseManage.Interfaces;

public interface IProductRepository
{
    // CRUD Operations
    Task<Product?> GetByIdAsync(int productId);
    Task<ProductListResponseDto> GetAllAsync(ProductSearchDto searchDto);
    Task<Product> CreateAsync(Product product);
    Task<Product?> UpdateAsync(int productId, Product product);
    Task<bool> DeleteAsync(int productId);
    
    // Additional queries
    Task<bool> ExistsAsync(int productId);
    Task<bool> ExistsBySkuAsync(string sku, int? excludeId = null);
    Task<Product?> GetBySkuAsync(string sku);
    
    // Statistics
    Task<ProductStatsDto?> GetProductStatsAsync(int productId);
    Task<List<ProductDto>> GetTopProductsAsync(int count = 5);
    Task<List<ProductInventoryDto>> GetLowStockProductsAsync();
    
    // Relationships
    Task<bool> HasInventoryMovementsAsync(int productId);
    Task<List<ProductInventoryDto>> GetProductInventoryAsync(int productId);
    
    // Supplier related
    Task<List<ProductDto>> GetProductsBySupplierId(int supplierId);
}
