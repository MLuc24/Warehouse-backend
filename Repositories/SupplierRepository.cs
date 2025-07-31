using Microsoft.EntityFrameworkCore;
using WarehouseManage.Data;
using WarehouseManage.DTOs.Supplier;
using WarehouseManage.Interfaces;
using WarehouseManage.Model;

namespace WarehouseManage.Repositories;

public class SupplierRepository : ISupplierRepository
{
    private readonly WarehouseDbContext _context;

    public SupplierRepository(WarehouseDbContext context)
    {
        _context = context;
    }

    public async Task<Supplier?> GetByIdAsync(int supplierId)
    {
        return await _context.Suppliers
            .Include(s => s.Products)
            .Include(s => s.GoodsReceipts)
            .FirstOrDefaultAsync(s => s.SupplierId == supplierId);
    }

    public async Task<SupplierListResponseDto> GetAllAsync(SupplierSearchDto searchDto)
    {
        var query = _context.Suppliers
            .Where(s => s.Status == "Active") // Only show active suppliers by default
            .AsQueryable();

        // Apply search filters
        if (!string.IsNullOrWhiteSpace(searchDto.SearchTerm))
        {
            var searchTerm = searchDto.SearchTerm.ToLower();
            query = query.Where(s => 
                s.SupplierName.ToLower().Contains(searchTerm) ||
                (s.Email != null && s.Email.ToLower().Contains(searchTerm)) ||
                (s.PhoneNumber != null && s.PhoneNumber.Contains(searchTerm)) ||
                (s.TaxCode != null && s.TaxCode.ToLower().Contains(searchTerm)));
        }

        if (!string.IsNullOrWhiteSpace(searchDto.Address))
        {
            query = query.Where(s => s.Address != null && s.Address.ToLower().Contains(searchDto.Address.ToLower()));
        }

        if (!string.IsNullOrWhiteSpace(searchDto.Email))
        {
            query = query.Where(s => s.Email != null && s.Email.ToLower().Contains(searchDto.Email.ToLower()));
        }

        // Apply sorting
        query = searchDto.SortBy?.ToLower() switch
        {
            "suppliername" => searchDto.SortDescending ? query.OrderByDescending(s => s.SupplierName) : query.OrderBy(s => s.SupplierName),
            "email" => searchDto.SortDescending ? query.OrderByDescending(s => s.Email) : query.OrderBy(s => s.Email),
            "createdat" => searchDto.SortDescending ? query.OrderByDescending(s => s.CreatedAt) : query.OrderBy(s => s.CreatedAt),
            _ => query.OrderBy(s => s.SupplierName)
        };

        var totalCount = await query.CountAsync();
        var totalPages = (int)Math.Ceiling((double)totalCount / searchDto.PageSize);

        var suppliers = await query
            .Include(s => s.Products)
            .Include(s => s.GoodsReceipts)
            .Skip((searchDto.Page - 1) * searchDto.PageSize)
            .Take(searchDto.PageSize)
            .Select(s => new SupplierDto
            {
                SupplierId = s.SupplierId,
                SupplierName = s.SupplierName,
                Address = s.Address,
                PhoneNumber = s.PhoneNumber,
                Email = s.Email,
                TaxCode = s.TaxCode,
                CreatedAt = s.CreatedAt,
                Status = s.Status,
                TotalProducts = s.Products.Count,
                TotalReceipts = s.GoodsReceipts.Count,
                TotalPurchaseValue = s.GoodsReceipts.Sum(gr => gr.TotalAmount ?? 0)
            })
            .ToListAsync();

        return new SupplierListResponseDto
        {
            Suppliers = suppliers,
            TotalCount = totalCount,
            Page = searchDto.Page,
            PageSize = searchDto.PageSize,
            TotalPages = totalPages
        };
    }

    public async Task<Supplier> CreateAsync(Supplier supplier)
    {
        supplier.CreatedAt = DateTime.UtcNow;
        supplier.Status = "Active"; // Set default status
        _context.Suppliers.Add(supplier);
        await _context.SaveChangesAsync();
        return supplier;
    }

    public async Task<Supplier?> UpdateAsync(int supplierId, Supplier supplier)
    {
        var existingSupplier = await _context.Suppliers.FindAsync(supplierId);
        if (existingSupplier == null)
            return null;

        existingSupplier.SupplierName = supplier.SupplierName;
        existingSupplier.Address = supplier.Address;
        existingSupplier.PhoneNumber = supplier.PhoneNumber;
        existingSupplier.Email = supplier.Email;
        existingSupplier.TaxCode = supplier.TaxCode;

        await _context.SaveChangesAsync();
        return existingSupplier;
    }

    public async Task<bool> DeleteAsync(int supplierId)
    {
        var supplier = await _context.Suppliers.FindAsync(supplierId);
        if (supplier == null)
            return false;

        // Instead of deleting, change status to Expired
        supplier.Status = "Expired";
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> ExistsAsync(int supplierId)
    {
        return await _context.Suppliers.AnyAsync(s => s.SupplierId == supplierId && s.Status == "Active");
    }

    public async Task<bool> ExistsByNameAsync(string supplierName, int? excludeId = null)
    {
        var query = _context.Suppliers.Where(s => s.SupplierName.ToLower() == supplierName.ToLower() && s.Status == "Active");
        
        if (excludeId.HasValue)
            query = query.Where(s => s.SupplierId != excludeId.Value);
            
        return await query.AnyAsync();
    }

    public async Task<bool> ExistsByTaxCodeAsync(string taxCode, int? excludeId = null)
    {
        if (string.IsNullOrWhiteSpace(taxCode))
            return false;

        var query = _context.Suppliers.Where(s => s.TaxCode != null && s.TaxCode.ToLower() == taxCode.ToLower() && s.Status == "Active");
        
        if (excludeId.HasValue)
            query = query.Where(s => s.SupplierId != excludeId.Value);
            
        return await query.AnyAsync();
    }

    public async Task<bool> ExistsByEmailAsync(string email, int? excludeId = null)
    {
        if (string.IsNullOrWhiteSpace(email))
            return false;

        var query = _context.Suppliers.Where(s => s.Email != null && s.Email.ToLower() == email.ToLower() && s.Status == "Active");
        
        if (excludeId.HasValue)
            query = query.Where(s => s.SupplierId != excludeId.Value);
            
        return await query.AnyAsync();
    }

    public async Task<SupplierStatsDto?> GetSupplierStatsAsync(int supplierId)
    {
        var supplier = await _context.Suppliers
            .Include(s => s.Products)
            .Include(s => s.GoodsReceipts)
            .FirstOrDefaultAsync(s => s.SupplierId == supplierId);

        if (supplier == null)
            return null;

        var monthlyPurchases = await _context.GoodsReceipts
            .Where(gr => gr.SupplierId == supplierId && gr.ReceiptDate.HasValue)
            .GroupBy(gr => new { Year = gr.ReceiptDate!.Value.Year, Month = gr.ReceiptDate!.Value.Month })
            .Select(g => new MonthlyPurchaseDto
            {
                Year = g.Key.Year,
                Month = g.Key.Month,
                TotalValue = g.Sum(gr => gr.TotalAmount ?? 0),
                ReceiptCount = g.Count()
            })
            .OrderByDescending(mp => mp.Year)
            .ThenByDescending(mp => mp.Month)
            .Take(12)
            .ToListAsync();

        return new SupplierStatsDto
        {
            SupplierId = supplier.SupplierId,
            SupplierName = supplier.SupplierName,
            TotalProducts = supplier.Products.Count,
            TotalGoodsReceipts = supplier.GoodsReceipts.Count,
            TotalPurchaseValue = supplier.GoodsReceipts.Sum(gr => gr.TotalAmount ?? 0),
            FirstReceiptDate = supplier.GoodsReceipts.OrderBy(gr => gr.ReceiptDate).FirstOrDefault()?.ReceiptDate,
            LastReceiptDate = supplier.GoodsReceipts.OrderByDescending(gr => gr.ReceiptDate).FirstOrDefault()?.ReceiptDate,
            MonthlyPurchases = monthlyPurchases
        };
    }

    public async Task<List<SupplierDto>> GetTopSuppliersAsync(int count = 5)
    {
        return await _context.Suppliers
            .Where(s => s.Status == "Active") // Only active suppliers
            .Include(s => s.Products)
            .Include(s => s.GoodsReceipts)
            .Select(s => new SupplierDto
            {
                SupplierId = s.SupplierId,
                SupplierName = s.SupplierName,
                Address = s.Address,
                PhoneNumber = s.PhoneNumber,
                Email = s.Email,
                TaxCode = s.TaxCode,
                CreatedAt = s.CreatedAt,
                Status = s.Status,
                TotalProducts = s.Products.Count,
                TotalReceipts = s.GoodsReceipts.Count,
                TotalPurchaseValue = s.GoodsReceipts.Sum(gr => gr.TotalAmount ?? 0)
            })
            .OrderByDescending(s => s.TotalPurchaseValue)
            .Take(count)
            .ToListAsync();
    }

    public async Task<bool> HasActiveProductsAsync(int supplierId)
    {
        return await _context.Products.AnyAsync(p => p.SupplierId == supplierId);
    }

    public async Task<bool> HasGoodsReceiptsAsync(int supplierId)
    {
        return await _context.GoodsReceipts.AnyAsync(gr => gr.SupplierId == supplierId);
    }

    public async Task<List<Product>> GetSupplierProductsAsync(int supplierId)
    {
        return await _context.Products
            .Where(p => p.SupplierId == supplierId)
            .ToListAsync();
    }

    public async Task<List<SupplierDto>> GetActiveSuppliersAsync()
    {
        return await _context.Suppliers
            .Where(s => s.Status == "Active")
            .Select(s => new SupplierDto
            {
                SupplierId = s.SupplierId,
                SupplierName = s.SupplierName,
                Address = s.Address,
                PhoneNumber = s.PhoneNumber,
                Email = s.Email,
                TaxCode = s.TaxCode,
                CreatedAt = s.CreatedAt,
                Status = s.Status
            })
            .OrderBy(s => s.SupplierName)
            .ToListAsync();
    }
}
