using Microsoft.Extensions.Logging;
using WarehouseManage.Constants;
using WarehouseManage.DTOs.Warehouse;
using WarehouseManage.Extensions;
using WarehouseManage.Interfaces;
using WarehouseManage.Model;

namespace WarehouseManage.Services;

public class WarehouseService : IWarehouseService
{
    private readonly IWarehouseRepository _warehouseRepository;
    private readonly ILogger<WarehouseService> _logger;

    public WarehouseService(IWarehouseRepository warehouseRepository, ILogger<WarehouseService> logger)
    {
        _warehouseRepository = warehouseRepository;
        _logger = logger;
    }

    public async Task<IEnumerable<WarehouseDto>> GetAllWarehousesAsync()
    {
        try
        {
            var warehouses = await _warehouseRepository.GetAllAsync();
            var warehouseDtos = new List<WarehouseDto>();

            foreach (var warehouse in warehouses)
            {
                var inventoryCount = await _warehouseRepository.GetInventoryCountAsync(warehouse.WarehouseId);
                warehouseDtos.Add(MapToDto(warehouse, inventoryCount));
            }

            return warehouseDtos;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all warehouses");
            throw new Exception(ErrorMessages.General.INTERNAL_ERROR, ex);
        }
    }

    public async Task<WarehouseDto?> GetWarehouseByIdAsync(int id)
    {
        try
        {
            if (!id.IsValidId())
                throw new ArgumentException(ErrorMessages.Warehouse.INVALID_ID);

            var warehouse = await _warehouseRepository.GetByIdAsync(id);
            if (warehouse == null)
                return null;

            var inventoryCount = await _warehouseRepository.GetInventoryCountAsync(warehouse.WarehouseId);
            return MapToDto(warehouse, inventoryCount);
        }
        catch (ArgumentException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting warehouse {WarehouseId}", id);
            throw new Exception(ErrorMessages.General.INTERNAL_ERROR, ex);
        }
    }

    public async Task<WarehouseDto> CreateWarehouseAsync(WarehouseDto warehouseDto)
    {
        try
        {
            await ValidateCreateWarehouseAsync(warehouseDto);

            var warehouse = new Warehouse
            {
                WarehouseName = warehouseDto.WarehouseName.Trim(),
                Address = warehouseDto.Address.Trim(),
                ContactPhone = warehouseDto.ContactPhone?.Trim()
            };

            var createdWarehouse = await _warehouseRepository.CreateAsync(warehouse);
            _logger.LogInformation("Warehouse created successfully. WarehouseId: {WarehouseId}, Name: {Name}", 
                createdWarehouse.WarehouseId, createdWarehouse.WarehouseName);

            return MapToDto(createdWarehouse, 0);
        }
        catch (ArgumentException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating warehouse {WarehouseName}", warehouseDto.WarehouseName);
            throw new Exception(ErrorMessages.Warehouse.CREATE_FAILED, ex);
        }
    }

    public async Task<WarehouseDto> UpdateWarehouseAsync(WarehouseDto warehouseDto)
    {
        try
        {
            if (!warehouseDto.WarehouseId.HasValue || !warehouseDto.WarehouseId.Value.IsValidId())
                throw new ArgumentException(ErrorMessages.Warehouse.INVALID_ID);

            var existingWarehouse = await _warehouseRepository.GetByIdAsync(warehouseDto.WarehouseId.Value);
            if (existingWarehouse == null)
                throw new ArgumentException(ErrorMessages.Warehouse.NOT_FOUND);

            await ValidateUpdateWarehouseAsync(warehouseDto);

            existingWarehouse.WarehouseName = warehouseDto.WarehouseName.Trim();
            existingWarehouse.Address = warehouseDto.Address.Trim();
            existingWarehouse.ContactPhone = warehouseDto.ContactPhone?.Trim();

            var updatedWarehouse = await _warehouseRepository.UpdateAsync(existingWarehouse);
            var inventoryCount = await _warehouseRepository.GetInventoryCountAsync(updatedWarehouse.WarehouseId);

            _logger.LogInformation("Warehouse updated successfully. WarehouseId: {WarehouseId}, Name: {Name}", 
                updatedWarehouse.WarehouseId, updatedWarehouse.WarehouseName);

            return MapToDto(updatedWarehouse, inventoryCount);
        }
        catch (ArgumentException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating warehouse {WarehouseId}", warehouseDto.WarehouseId);
            throw new Exception(ErrorMessages.Warehouse.UPDATE_FAILED, ex);
        }
    }

    public async Task<bool> DeleteWarehouseAsync(int id)
    {
        try
        {
            if (!id.IsValidId())
                throw new ArgumentException(ErrorMessages.Warehouse.INVALID_ID);

            var warehouse = await _warehouseRepository.GetByIdAsync(id);
            if (warehouse == null)
                throw new ArgumentException(ErrorMessages.Warehouse.NOT_FOUND);

            var hasInventory = await _warehouseRepository.HasInventoryAsync(id);
            if (hasInventory)
                throw new ArgumentException(ErrorMessages.Warehouse.CANNOT_DELETE_HAS_INVENTORY);

            var result = await _warehouseRepository.DeleteAsync(id);
            if (result)
            {
                _logger.LogInformation("Warehouse deleted successfully. WarehouseId: {WarehouseId}, Name: {Name}", 
                    warehouse.WarehouseId, warehouse.WarehouseName);
            }

            return result;
        }
        catch (ArgumentException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting warehouse {WarehouseId}", id);
            throw new Exception(ErrorMessages.Warehouse.DELETE_FAILED, ex);
        }
    }

    public async Task<bool> WarehouseExistsAsync(int id)
    {
        try
        {
            if (!id.IsValidId())
                return false;

            var warehouse = await _warehouseRepository.GetByIdAsync(id);
            return warehouse != null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking warehouse existence {WarehouseId}", id);
            return false;
        }
    }

    private async Task ValidateCreateWarehouseAsync(WarehouseDto warehouseDto)
    {
        if (!warehouseDto.WarehouseName.IsNotEmpty())
            throw new ArgumentException("Tên kho hàng không được để trống");

        if (!warehouseDto.Address.IsNotEmpty())
            throw new ArgumentException("Địa chỉ không được để trống");

        var nameExists = await _warehouseRepository.ExistsByNameAsync(warehouseDto.WarehouseName);
        if (nameExists)
            throw new ArgumentException(ErrorMessages.Warehouse.NAME_ALREADY_EXISTS);

        var addressExists = await _warehouseRepository.ExistsByAddressAsync(warehouseDto.Address);
        if (addressExists)
            throw new ArgumentException(ErrorMessages.Warehouse.ADDRESS_ALREADY_EXISTS);
    }

    private async Task ValidateUpdateWarehouseAsync(WarehouseDto warehouseDto)
    {
        if (!warehouseDto.WarehouseName.IsNotEmpty())
            throw new ArgumentException("Tên kho hàng không được để trống");

        if (!warehouseDto.Address.IsNotEmpty())
            throw new ArgumentException("Địa chỉ không được để trống");

        var nameExists = await _warehouseRepository.ExistsByNameAsync(warehouseDto.WarehouseName, warehouseDto.WarehouseId);
        if (nameExists)
            throw new ArgumentException(ErrorMessages.Warehouse.NAME_ALREADY_EXISTS);

        var addressExists = await _warehouseRepository.ExistsByAddressAsync(warehouseDto.Address, warehouseDto.WarehouseId);
        if (addressExists)
            throw new ArgumentException(ErrorMessages.Warehouse.ADDRESS_ALREADY_EXISTS);
    }

    private static WarehouseDto MapToDto(Warehouse warehouse, int inventoryCount)
    {
        return new WarehouseDto
        {
            WarehouseId = warehouse.WarehouseId,
            WarehouseName = warehouse.WarehouseName,
            Address = warehouse.Address,
            ContactPhone = warehouse.ContactPhone,
            CreatedAt = warehouse.CreatedAt,
            TotalInventoryItems = inventoryCount
        };
    }
}
