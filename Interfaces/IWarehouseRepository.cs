using WarehouseManage.Model;

namespace WarehouseManage.Interfaces;

public interface IWarehouseRepository
{
    Task<IEnumerable<Warehouse>> GetAllAsync();
    Task<Warehouse?> GetByIdAsync(int id);
    Task<Warehouse?> GetByNameAsync(string name);
    Task<Warehouse?> GetByAddressAsync(string address);
    Task<bool> ExistsByNameAsync(string name, int? excludeId = null);
    Task<bool> ExistsByAddressAsync(string address, int? excludeId = null);
    Task<Warehouse> CreateAsync(Warehouse warehouse);
    Task<Warehouse> UpdateAsync(Warehouse warehouse);
    Task<bool> DeleteAsync(int id);
    Task<bool> HasInventoryAsync(int warehouseId);
    Task<int> GetInventoryCountAsync(int warehouseId);
}
