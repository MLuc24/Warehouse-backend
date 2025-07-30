using WarehouseManage.DTOs.Supplier;

namespace WarehouseManage.Interfaces;

public interface ISupplierService
{
    // CRUD Operations
    Task<SupplierDto?> GetSupplierByIdAsync(int supplierId);
    Task<SupplierListResponseDto> GetAllSuppliersAsync(SupplierSearchDto searchDto);
    Task<SupplierDto> CreateSupplierAsync(CreateSupplierDto createDto);
    Task<SupplierDto?> UpdateSupplierAsync(int supplierId, UpdateSupplierDto updateDto);
    Task<bool> DeleteSupplierAsync(int supplierId);
    
    // Validation
    Task<bool> ValidateSupplierDataAsync(CreateSupplierDto createDto);
    Task<bool> ValidateSupplierDataAsync(int supplierId, UpdateSupplierDto updateDto);
    Task<bool> CanDeleteSupplierAsync(int supplierId);
    
    // Statistics & Analytics
    Task<SupplierStatsDto?> GetSupplierStatisticsAsync(int supplierId);
    Task<List<SupplierDto>> GetTopSuppliersAsync(int count = 5);
    
    // Business Logic
    Task<bool> SupplierExistsAsync(int supplierId);
}
