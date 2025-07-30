namespace WarehouseManage.Interfaces;

public interface ISupplierRepository
{
    // Basic method needed for ProductService
    Task<bool> ExistsAsync(int supplierId);
}
