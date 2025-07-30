using WarehouseManage.DTOs.Supplier;
using WarehouseManage.Model;

namespace WarehouseManage.Interfaces;

public interface ISupplierRepository
{
    // CRUD Operations
    Task<Supplier?> GetByIdAsync(int supplierId);
    Task<SupplierListResponseDto> GetAllAsync(SupplierSearchDto searchDto);
    Task<Supplier> CreateAsync(Supplier supplier);
    Task<Supplier?> UpdateAsync(int supplierId, Supplier supplier);
    Task<bool> DeleteAsync(int supplierId);
    
    // Additional queries
    Task<bool> ExistsAsync(int supplierId);
    Task<bool> ExistsByNameAsync(string supplierName, int? excludeId = null);
    Task<bool> ExistsByTaxCodeAsync(string taxCode, int? excludeId = null);
    Task<bool> ExistsByEmailAsync(string email, int? excludeId = null);
    
    // Statistics
    Task<SupplierStatsDto?> GetSupplierStatsAsync(int supplierId);
    Task<List<SupplierDto>> GetTopSuppliersAsync(int count = 5);
    
    // Relationships
    Task<bool> HasActiveProductsAsync(int supplierId);
    Task<bool> HasGoodsReceiptsAsync(int supplierId);
    Task<List<Product>> GetSupplierProductsAsync(int supplierId);
}
