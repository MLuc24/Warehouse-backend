using WarehouseManage.DTOs.Warehouse;
using WarehouseManage.Model;

namespace WarehouseManage.Interfaces;

public interface IWarehouseService
{
    Task<IEnumerable<WarehouseDto>> GetAllWarehousesAsync();
    Task<WarehouseDto?> GetWarehouseByIdAsync(int id);
    Task<WarehouseDto> CreateWarehouseAsync(WarehouseDto warehouseDto);
    Task<WarehouseDto> UpdateWarehouseAsync(WarehouseDto warehouseDto);
    Task<bool> DeleteWarehouseAsync(int id);
    Task<bool> WarehouseExistsAsync(int id);
}
