using Microsoft.EntityFrameworkCore;
using WarehouseManage.Data;
using WarehouseManage.Interfaces;
using WarehouseManage.Model;

namespace WarehouseManage.Repositories;

public class WarehouseRepository : IWarehouseRepository
{
    private readonly WarehouseDbContext _context;

    public WarehouseRepository(WarehouseDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Warehouse>> GetAllAsync()
    {
        return await _context.Warehouses
            .Include(w => w.Inventories)
            .OrderBy(w => w.WarehouseName)
            .ToListAsync();
    }

    public async Task<Warehouse?> GetByIdAsync(int id)
    {
        return await _context.Warehouses
            .Include(w => w.Inventories)
            .FirstOrDefaultAsync(w => w.WarehouseId == id);
    }

    public async Task<Warehouse?> GetByNameAsync(string name)
    {
        return await _context.Warehouses
            .FirstOrDefaultAsync(w => w.WarehouseName.ToLower() == name.ToLower());
    }

    public async Task<Warehouse?> GetByAddressAsync(string address)
    {
        return await _context.Warehouses
            .FirstOrDefaultAsync(w => w.Address.ToLower() == address.ToLower());
    }

    public async Task<bool> ExistsByNameAsync(string name, int? excludeId = null)
    {
        var query = _context.Warehouses.Where(w => w.WarehouseName.ToLower() == name.ToLower());
        
        if (excludeId.HasValue)
        {
            query = query.Where(w => w.WarehouseId != excludeId.Value);
        }

        return await query.AnyAsync();
    }

    public async Task<bool> ExistsByAddressAsync(string address, int? excludeId = null)
    {
        var query = _context.Warehouses.Where(w => w.Address.ToLower() == address.ToLower());
        
        if (excludeId.HasValue)
        {
            query = query.Where(w => w.WarehouseId != excludeId.Value);
        }

        return await query.AnyAsync();
    }

    public async Task<Warehouse> CreateAsync(Warehouse warehouse)
    {
        warehouse.CreatedAt = DateTime.UtcNow;
        _context.Warehouses.Add(warehouse);
        await _context.SaveChangesAsync();
        return warehouse;
    }

    public async Task<Warehouse> UpdateAsync(Warehouse warehouse)
    {
        _context.Warehouses.Update(warehouse);
        await _context.SaveChangesAsync();
        return warehouse;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var warehouse = await _context.Warehouses.FindAsync(id);
        if (warehouse == null)
            return false;

        _context.Warehouses.Remove(warehouse);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> HasInventoryAsync(int warehouseId)
    {
        return await _context.Inventories
            .AnyAsync(i => i.WarehouseId == warehouseId);
    }

    public async Task<int> GetInventoryCountAsync(int warehouseId)
    {
        return await _context.Inventories
            .CountAsync(i => i.WarehouseId == warehouseId);
    }
}
