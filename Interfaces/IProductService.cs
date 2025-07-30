using WarehouseManage.DTOs.Product;

namespace WarehouseManage.Interfaces;

public interface IProductService
{
    // Core CRUD operations
    Task<ProductDto?> GetProductByIdAsync(int productId);
    Task<ProductListResponseDto> GetAllProductsAsync(ProductSearchDto searchDto);
    Task<ProductDto> CreateProductAsync(CreateProductDto createDto);
    Task<ProductDto?> UpdateProductAsync(int productId, UpdateProductDto updateDto);
    Task<bool> DeleteProductAsync(int productId);
    
    // Additional queries
    Task<ProductDto?> GetProductBySkuAsync(string sku);
    Task<ProductStatsDto?> GetProductStatsAsync(int productId);
    Task<List<ProductDto>> GetTopProductsAsync(int count = 5);
    Task<List<ProductInventoryDto>> GetLowStockProductsAsync();
    
    // Supplier related
    Task<List<ProductDto>> GetProductsBySupplierAsync(int supplierId);
}
