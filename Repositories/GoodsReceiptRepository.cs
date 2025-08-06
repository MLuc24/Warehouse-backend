using Microsoft.EntityFrameworkCore;
using WarehouseManage.Constants;
using WarehouseManage.Data;
using WarehouseManage.DTOs.GoodsReceipt;
using WarehouseManage.Interfaces;
using WarehouseManage.Model;

namespace WarehouseManage.Repositories;

public class GoodsReceiptRepository : IGoodsReceiptRepository
{
    private readonly WarehouseDbContext _context;

    public GoodsReceiptRepository(WarehouseDbContext context)
    {
        _context = context;
    }

    public async Task<PagedResult<GoodsReceipt>> GetGoodsReceiptsAsync(GoodsReceiptFilterDto filter)
    {
        var query = _context.GoodsReceipts
            .Include(gr => gr.Supplier)
            .Include(gr => gr.CreatedByUser)
            .Include(gr => gr.GoodsReceiptDetails)
                .ThenInclude(grd => grd.Product)
            .AsQueryable();

        // Apply filters
        if (!string.IsNullOrWhiteSpace(filter.ReceiptNumber))
        {
            query = query.Where(gr => gr.ReceiptNumber.Contains(filter.ReceiptNumber));
        }

        if (filter.SupplierId.HasValue)
        {
            query = query.Where(gr => gr.SupplierId == filter.SupplierId.Value);
        }

        if (!string.IsNullOrWhiteSpace(filter.SupplierName))
        {
            query = query.Where(gr => gr.Supplier.SupplierName.Contains(filter.SupplierName));
        }

        if (filter.FromDate.HasValue)
        {
            query = query.Where(gr => gr.ReceiptDate >= filter.FromDate.Value);
        }

        if (filter.ToDate.HasValue)
        {
            query = query.Where(gr => gr.ReceiptDate <= filter.ToDate.Value);
        }

        if (!string.IsNullOrWhiteSpace(filter.Status))
        {
            query = query.Where(gr => gr.Status == filter.Status);
        }

        if (filter.MinAmount.HasValue)
        {
            query = query.Where(gr => gr.TotalAmount >= filter.MinAmount.Value);
        }

        if (filter.MaxAmount.HasValue)
        {
            query = query.Where(gr => gr.TotalAmount <= filter.MaxAmount.Value);
        }

        // Order by latest first
        query = query.OrderByDescending(gr => gr.ReceiptDate);

        var totalCount = await query.CountAsync();
        var items = await query
            .Skip((filter.PageNumber - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .ToListAsync();

        return new PagedResult<GoodsReceipt>(items, totalCount, filter.PageNumber, filter.PageSize);
    }

    public async Task<GoodsReceipt?> GetGoodsReceiptByIdAsync(int id)
    {
        return await _context.GoodsReceipts
            .Include(gr => gr.Supplier)
            .Include(gr => gr.CreatedByUser)
            .Include(gr => gr.GoodsReceiptDetails)
                .ThenInclude(grd => grd.Product)
            .FirstOrDefaultAsync(gr => gr.GoodsReceiptId == id);
    }

    public async Task<GoodsReceipt?> GetGoodsReceiptByNumberAsync(string receiptNumber)
    {
        return await _context.GoodsReceipts
            .Include(gr => gr.Supplier)
            .Include(gr => gr.CreatedByUser)
            .Include(gr => gr.GoodsReceiptDetails)
                .ThenInclude(grd => grd.Product)
            .FirstOrDefaultAsync(gr => gr.ReceiptNumber == receiptNumber);
    }

    public async Task<GoodsReceipt> CreateGoodsReceiptAsync(GoodsReceipt receipt)
    {
        _context.GoodsReceipts.Add(receipt);
        await _context.SaveChangesAsync();
        
        // Reload with includes
        return await GetGoodsReceiptByIdAsync(receipt.GoodsReceiptId) ?? receipt;
    }

    public async Task<GoodsReceipt> UpdateGoodsReceiptAsync(GoodsReceipt receipt)
    {
        _context.GoodsReceipts.Update(receipt);
        await _context.SaveChangesAsync();
        
        // Reload with includes
        return await GetGoodsReceiptByIdAsync(receipt.GoodsReceiptId) ?? receipt;
    }

    public async Task<bool> DeleteGoodsReceiptAsync(int id)
    {
        var receipt = await _context.GoodsReceipts.FindAsync(id);
        if (receipt == null) return false;

        _context.GoodsReceipts.Remove(receipt);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> ExistsAsync(int id)
    {
        return await _context.GoodsReceipts.AnyAsync(gr => gr.GoodsReceiptId == id);
    }

    public async Task<string> GenerateReceiptNumberAsync()
    {
        var today = DateTime.Today;
        var todayString = today.ToString("yyyyMMdd");
        
        var lastReceiptToday = await _context.GoodsReceipts
            .Where(gr => gr.ReceiptNumber.StartsWith($"{GoodsReceiptConstants.ReceiptNumberPrefix.Standard}{todayString}"))
            .OrderByDescending(gr => gr.ReceiptNumber)
            .FirstOrDefaultAsync();

        int sequence = 1;
        if (lastReceiptToday != null)
        {
            var lastNumber = lastReceiptToday.ReceiptNumber;
            if (lastNumber.Length >= 13) // GR + 8 digits + 3 digits
            {
                var lastSequence = lastNumber.Substring(10, 3);
                if (int.TryParse(lastSequence, out int lastSeq))
                {
                    sequence = lastSeq + 1;
                }
            }
        }

        return string.Format(GoodsReceiptConstants.ReceiptNumberPrefix.Format, today, sequence);
    }

    public async Task<List<GoodsReceipt>> GetGoodsReceiptsBySupplierAsync(int supplierId)
    {
        return await _context.GoodsReceipts
            .Include(gr => gr.Supplier)
            .Include(gr => gr.CreatedByUser)
            .Include(gr => gr.GoodsReceiptDetails)
                .ThenInclude(grd => grd.Product)
            .Where(gr => gr.SupplierId == supplierId)
            .OrderByDescending(gr => gr.ReceiptDate)
            .ToListAsync();
    }

    public async Task<List<GoodsReceipt>> GetGoodsReceiptsByDateRangeAsync(DateTime fromDate, DateTime toDate)
    {
        return await _context.GoodsReceipts
            .Include(gr => gr.Supplier)
            .Include(gr => gr.CreatedByUser)
            .Include(gr => gr.GoodsReceiptDetails)
                .ThenInclude(grd => grd.Product)
            .Where(gr => gr.ReceiptDate >= fromDate && gr.ReceiptDate <= toDate)
            .OrderByDescending(gr => gr.ReceiptDate)
            .ToListAsync();
    }
}
