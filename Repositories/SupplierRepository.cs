using Microsoft.EntityFrameworkCore;
using WarehouseManage.Data;
using WarehouseManage.Interfaces;

namespace WarehouseManage.Repositories;

public class SupplierRepository : ISupplierRepository
{
    private readonly WarehouseDbContext _context;

    public SupplierRepository(WarehouseDbContext context)
    {
        _context = context;
    }

    public async Task<bool> ExistsAsync(int supplierId)
    {
        return await _context.Suppliers.AnyAsync(s => s.SupplierId == supplierId);
    }
}
