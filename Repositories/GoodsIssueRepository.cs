using Microsoft.EntityFrameworkCore;
using WarehouseManage.Constants;
using WarehouseManage.Data;
using WarehouseManage.DTOs.GoodsIssue;
using WarehouseManage.DTOs.GoodsReceipt;
using WarehouseManage.Interfaces;
using WarehouseManage.Model;

namespace WarehouseManage.Repositories;

public class GoodsIssueRepository : IGoodsIssueRepository
{
    private readonly WarehouseDbContext _context;

    public GoodsIssueRepository(WarehouseDbContext context)
    {
        _context = context;
    }

    public async Task<PagedResult<GoodsIssue>> GetGoodsIssuesAsync(GoodsIssueFilterDto filter)
    {
        // For list view, we only need basic information, not all related data
        var query = _context.GoodsIssues
            .Include(gi => gi.Customer) // Only customer name needed for list
            .Include(gi => gi.CreatedByUser) // Only creator name needed for list
            .AsQueryable();

        // Apply filters
        if (!string.IsNullOrEmpty(filter.IssueNumber))
        {
            query = query.Where(gi => gi.IssueNumber.Contains(filter.IssueNumber));
        }

        if (!string.IsNullOrEmpty(filter.Status))
        {
            query = query.Where(gi => gi.Status == filter.Status);
        }

        if (filter.CustomerId.HasValue)
        {
            query = query.Where(gi => gi.CustomerId == filter.CustomerId);
        }

        if (filter.FromDate.HasValue)
        {
            query = query.Where(gi => gi.IssueDate >= filter.FromDate);
        }

        if (filter.ToDate.HasValue)
        {
            query = query.Where(gi => gi.IssueDate <= filter.ToDate);
        }

        // Order by CreatedAt descending
        query = query.OrderByDescending(gi => gi.CreatedAt);

        // Get total count
        var totalCount = await query.CountAsync();

        // Apply pagination
        var items = await query
            .Skip((filter.PageNumber - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .ToListAsync();

        return new PagedResult<GoodsIssue>(items, totalCount, filter.PageNumber, filter.PageSize);
    }

    public async Task<GoodsIssue?> GetGoodsIssueByIdAsync(int id)
    {
        return await _context.GoodsIssues
            .Include(gi => gi.Customer)
            .Include(gi => gi.CreatedByUser)
            .Include(gi => gi.ApprovedByUser)
            .Include(gi => gi.PreparedByUser)
            .Include(gi => gi.DeliveredByUser)
            .Include(gi => gi.CompletedByUser)
            .Include(gi => gi.GoodsIssueDetails)
                .ThenInclude(gid => gid.Product)
            .FirstOrDefaultAsync(gi => gi.GoodsIssueId == id);
    }

    public async Task<GoodsIssue?> GetGoodsIssueByNumberAsync(string issueNumber)
    {
        return await _context.GoodsIssues
            .Include(gi => gi.Customer)
            .Include(gi => gi.CreatedByUser)
            .Include(gi => gi.ApprovedByUser)
            .Include(gi => gi.PreparedByUser)
            .Include(gi => gi.DeliveredByUser)
            .Include(gi => gi.CompletedByUser)
            .Include(gi => gi.GoodsIssueDetails)
                .ThenInclude(gid => gid.Product)
            .FirstOrDefaultAsync(gi => gi.IssueNumber == issueNumber);
    }

    public async Task<GoodsIssue> CreateGoodsIssueAsync(GoodsIssue goodsIssue)
    {
        _context.GoodsIssues.Add(goodsIssue);
        await _context.SaveChangesAsync();
        return goodsIssue;
    }

    public async Task<GoodsIssue> UpdateGoodsIssueAsync(GoodsIssue goodsIssue)
    {
        goodsIssue.UpdatedAt = DateTime.UtcNow;
        _context.GoodsIssues.Update(goodsIssue);
        await _context.SaveChangesAsync();
        return goodsIssue;
    }

    public async Task<bool> DeleteGoodsIssueAsync(int id)
    {
        var goodsIssue = await _context.GoodsIssues.FindAsync(id);
        if (goodsIssue == null)
            return false;

        _context.GoodsIssues.Remove(goodsIssue);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<List<GoodsIssue>> GetGoodsIssuesByCustomerAsync(int customerId)
    {
        return await _context.GoodsIssues
            .Include(gi => gi.Customer)
            .Include(gi => gi.CreatedByUser)
            .Include(gi => gi.GoodsIssueDetails)
                .ThenInclude(gid => gid.Product)
            .Where(gi => gi.CustomerId == customerId)
            .OrderByDescending(gi => gi.CreatedAt)
            .ToListAsync();
    }

    public async Task<bool> ExistsAsync(int id)
    {
        return await _context.GoodsIssues.AnyAsync(gi => gi.GoodsIssueId == id);
    }

    public async Task<bool> IssueNumberExistsAsync(string issueNumber)
    {
        return await _context.GoodsIssues.AnyAsync(gi => gi.IssueNumber == issueNumber);
    }

    public async Task<string> GenerateNextIssueNumberAsync()
    {
        var today = DateTime.Today;
        var datePrefix = today.ToString("yyyyMMdd");
        var pattern = $"{GoodsIssueConstants.IssueNumberPrefix.Standard}{datePrefix}%";

        var lastIssueNumber = await _context.GoodsIssues
            .Where(gi => gi.IssueNumber.StartsWith($"{GoodsIssueConstants.IssueNumberPrefix.Standard}{datePrefix}"))
            .OrderByDescending(gi => gi.IssueNumber)
            .Select(gi => gi.IssueNumber)
            .FirstOrDefaultAsync();

        if (lastIssueNumber == null)
        {
            return string.Format(GoodsIssueConstants.IssueNumberPrefix.Format, today, 1);
        }

        // Extract sequence number from the last issue number
        var sequencePart = lastIssueNumber.Substring(10); // Skip "GI" + "YYYYMMDD" = 10 characters
        if (int.TryParse(sequencePart, out int lastSequence))
        {
            return string.Format(GoodsIssueConstants.IssueNumberPrefix.Format, today, lastSequence + 1);
        }

        return string.Format(GoodsIssueConstants.IssueNumberPrefix.Format, today, 1);
    }
}
