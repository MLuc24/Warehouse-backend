using Microsoft.EntityFrameworkCore;
using WarehouseManage.Constants;
using WarehouseManage.Data;
using WarehouseManage.DTOs.Employee;
using WarehouseManage.Interfaces;
using WarehouseManage.Model;

namespace WarehouseManage.Repositories;

public class EmployeeRepository : IEmployeeRepository
{
    private readonly WarehouseDbContext _context;

    public EmployeeRepository(WarehouseDbContext context)
    {
        _context = context;
    }

    public async Task<User?> GetByIdAsync(int employeeId)
    {
        return await _context.Users
            .Include(u => u.GoodsIssues)
            .Include(u => u.GoodsReceipts)
            .Where(u => u.Role != RoleConstants.Admin)
            .FirstOrDefaultAsync(u => u.UserId == employeeId);
    }

    public async Task<EmployeeListResponseDto> GetAllAsync(EmployeeSearchDto searchDto)
    {
        var query = _context.Users
            .Where(u => u.Role != RoleConstants.Admin)
            .Include(u => u.GoodsIssues)
            .Include(u => u.GoodsReceipts)
            .AsQueryable();

        // Apply filters
        if (!string.IsNullOrWhiteSpace(searchDto.SearchTerm))
        {
            var searchTerm = searchDto.SearchTerm.ToLower();
            query = query.Where(u => 
                u.FullName.ToLower().Contains(searchTerm) ||
                u.Username.ToLower().Contains(searchTerm) ||
                (u.Email != null && u.Email.ToLower().Contains(searchTerm)));
        }

        if (!string.IsNullOrWhiteSpace(searchDto.Role))
        {
            query = query.Where(u => u.Role == searchDto.Role);
        }

        if (searchDto.Status.HasValue)
        {
            query = query.Where(u => u.Status == searchDto.Status);
        }

        if (searchDto.IsEmailVerified.HasValue)
        {
            query = query.Where(u => u.IsEmailVerified == searchDto.IsEmailVerified);
        }

        if (searchDto.IsPhoneVerified.HasValue)
        {
            query = query.Where(u => u.IsPhoneVerified == searchDto.IsPhoneVerified);
        }

        if (searchDto.CreatedFrom.HasValue)
        {
            query = query.Where(u => u.CreatedAt >= searchDto.CreatedFrom);
        }

        if (searchDto.CreatedTo.HasValue)
        {
            query = query.Where(u => u.CreatedAt <= searchDto.CreatedTo);
        }

        // Apply sorting
        query = ApplySorting(query, searchDto.SortBy, searchDto.SortDirection);

        // Get total count before pagination
        var totalCount = await query.CountAsync();

        // Apply pagination
        var pageSize = Math.Min(searchDto.PageSize, EmployeeConstants.MAX_PAGE_SIZE);
        pageSize = Math.Max(pageSize, EmployeeConstants.MIN_PAGE_SIZE);
        var skip = (searchDto.Page - 1) * pageSize;

        var employees = await query
            .Skip(skip)
            .Take(pageSize)
            .Select(u => new EmployeeDto
            {
                UserId = u.UserId,
                Username = u.Username,
                FullName = u.FullName,
                Email = u.Email,
                PhoneNumber = u.PhoneNumber,
                Address = u.Address,
                Image = u.Image,
                IsEmailVerified = u.IsEmailVerified,
                IsPhoneVerified = u.IsPhoneVerified,
                Role = u.Role,
                Status = u.Status,
                CreatedAt = u.CreatedAt,
                TotalGoodsIssuesCreated = u.GoodsIssues.Count,
                TotalGoodsReceiptsCreated = u.GoodsReceipts.Count,
                TotalApprovals = 0 // Will be calculated in service
            })
            .ToListAsync();

        var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);

        return new EmployeeListResponseDto
        {
            Employees = employees,
            TotalCount = totalCount,
            TotalPages = totalPages,
            CurrentPage = searchDto.Page,
            PageSize = pageSize,
            HasPreviousPage = searchDto.Page > 1,
            HasNextPage = searchDto.Page < totalPages
        };
    }

    public async Task<User> CreateAsync(User employee)
    {
        _context.Users.Add(employee);
        await _context.SaveChangesAsync();
        return employee;
    }

    public async Task<User?> UpdateAsync(int employeeId, User employee)
    {
        var existingEmployee = await GetByIdAsync(employeeId);
        if (existingEmployee == null)
            return null;

        // Update properties
        existingEmployee.FullName = employee.FullName;
        existingEmployee.Email = employee.Email;
        existingEmployee.PhoneNumber = employee.PhoneNumber;
        existingEmployee.Address = employee.Address;
        existingEmployee.Role = employee.Role;
        existingEmployee.Status = employee.Status;

        await _context.SaveChangesAsync();
        return existingEmployee;
    }

    public async Task<bool> DeleteAsync(int employeeId)
    {
        var employee = await GetByIdAsync(employeeId);
        if (employee == null)
            return false;

        // Soft delete - set status to inactive
        employee.Status = false;
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> ReactivateAsync(int employeeId)
    {
        var employee = await GetByIdAsync(employeeId);
        if (employee == null)
            return false;

        employee.Status = true;
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> ExistsAsync(int employeeId)
    {
        return await _context.Users
            .Where(u => u.Role != RoleConstants.Admin)
            .AnyAsync(u => u.UserId == employeeId);
    }

    public async Task<bool> ExistsByUsernameAsync(string username, int? excludeId = null)
    {
        var query = _context.Users
            .Where(u => u.Username == username);

        if (excludeId.HasValue)
        {
            query = query.Where(u => u.UserId != excludeId.Value);
        }

        return await query.AnyAsync();
    }

    public async Task<bool> ExistsByEmailAsync(string email, int? excludeId = null)
    {
        if (string.IsNullOrWhiteSpace(email))
            return false;

        var query = _context.Users
            .Where(u => u.Email == email);

        if (excludeId.HasValue)
        {
            query = query.Where(u => u.UserId != excludeId.Value);
        }

        return await query.AnyAsync();
    }

    public async Task<List<User>> GetEmployeesByRoleAsync(string role)
    {
        return await _context.Users
            .Where(u => u.Role == role && u.Status == true)
            .OrderBy(u => u.FullName)
            .ToListAsync();
    }

    public async Task<List<User>> GetActiveEmployeesAsync()
    {
        return await _context.Users
            .Where(u => u.Role != RoleConstants.Admin && u.Status == true)
            .OrderBy(u => u.FullName)
            .ToListAsync();
    }

    public async Task<User?> GetByUsernameAsync(string username)
    {
        return await _context.Users
            .Where(u => u.Role != RoleConstants.Admin)
            .FirstOrDefaultAsync(u => u.Username == username);
    }

    public async Task<EmployeeStatsDto?> GetEmployeeStatsAsync(int employeeId)
    {
        var employee = await _context.Users
            .Include(u => u.GoodsIssues)
            .Include(u => u.GoodsReceipts)
            .Where(u => u.Role != RoleConstants.Admin)
            .FirstOrDefaultAsync(u => u.UserId == employeeId);

        if (employee == null)
            return null;

        var goodsIssuesCreated = employee.GoodsIssues.Count;
        var goodsReceiptsCreated = employee.GoodsReceipts.Count;

        // Count approvals (as ApprovedByUser in both GoodsIssue and GoodsReceipt)
        var approvalsCount = await _context.GoodsIssues
            .CountAsync(gi => gi.ApprovedByUserId == employeeId) +
            await _context.GoodsReceipts
            .CountAsync(gr => gr.ApprovedByUserId == employeeId);

        // Calculate completed tasks
        var completedTasks = await _context.GoodsIssues
            .CountAsync(gi => gi.CompletedByUserId == employeeId) +
            await _context.GoodsReceipts
            .CountAsync(gr => gr.CompletedByUserId == employeeId);

        // Get monthly activities for the last 12 months
        var monthlyActivities = await GetMonthlyActivitiesAsync(employeeId);

        return new EmployeeStatsDto
        {
            EmployeeId = employee.UserId,
            FullName = employee.FullName,
            Role = employee.Role,
            TotalGoodsIssuesCreated = goodsIssuesCreated,
            TotalGoodsReceiptsCreated = goodsReceiptsCreated,
            TotalApprovals = approvalsCount,
            TotalCompletedTasks = completedTasks,
            LastActivityDate = GetLastActivityDate(employee),
            DaysActive = CalculateDaysActive(employee.CreatedAt),
            TasksThisMonth = GetTasksThisMonth(employee),
            MonthlyActivities = monthlyActivities
        };
    }

    public async Task<List<EmployeeDto>> GetTopPerformersAsync(int count = 5)
    {
        return await _context.Users
            .Where(u => u.Role != RoleConstants.Admin && u.Status == true)
            .Include(u => u.GoodsIssues)
            .Include(u => u.GoodsReceipts)
            .Select(u => new EmployeeDto
            {
                UserId = u.UserId,
                Username = u.Username,
                FullName = u.FullName,
                Email = u.Email,
                PhoneNumber = u.PhoneNumber,
                Role = u.Role,
                Status = u.Status,
                CreatedAt = u.CreatedAt,
                TotalGoodsIssuesCreated = u.GoodsIssues.Count,
                TotalGoodsReceiptsCreated = u.GoodsReceipts.Count
            })
            .OrderByDescending(e => e.TotalGoodsIssuesCreated + e.TotalGoodsReceiptsCreated)
            .Take(count)
            .ToListAsync();
    }

    public async Task<bool> HasActiveTasksAsync(int employeeId)
    {
        var hasActiveGoodsIssues = await _context.GoodsIssues
            .AnyAsync(gi => gi.CreatedByUserId == employeeId && gi.Status != "Completed");

        var hasActiveGoodsReceipts = await _context.GoodsReceipts
            .AnyAsync(gr => gr.CreatedByUserId == employeeId && gr.Status != "Completed");

        return hasActiveGoodsIssues || hasActiveGoodsReceipts;
    }

    public async Task<bool> HasCreatedGoodsIssuesAsync(int employeeId)
    {
        return await _context.GoodsIssues
            .AnyAsync(gi => gi.CreatedByUserId == employeeId);
    }

    public async Task<bool> HasCreatedGoodsReceiptsAsync(int employeeId)
    {
        return await _context.GoodsReceipts
            .AnyAsync(gr => gr.CreatedByUserId == employeeId);
    }

    public async Task<int> GetTotalGoodsIssuesCreatedAsync(int employeeId)
    {
        return await _context.GoodsIssues
            .CountAsync(gi => gi.CreatedByUserId == employeeId);
    }

    public async Task<int> GetTotalGoodsReceiptsCreatedAsync(int employeeId)
    {
        return await _context.GoodsReceipts
            .CountAsync(gr => gr.CreatedByUserId == employeeId);
    }

    public async Task<int> GetTotalApprovalsAsync(int employeeId)
    {
        var goodsIssueApprovals = await _context.GoodsIssues
            .CountAsync(gi => gi.ApprovedByUserId == employeeId);

        var goodsReceiptApprovals = await _context.GoodsReceipts
            .CountAsync(gr => gr.ApprovedByUserId == employeeId);

        return goodsIssueApprovals + goodsReceiptApprovals;
    }

    public async Task<bool> UpdatePasswordAsync(int employeeId, string hashedPassword)
    {
        var employee = await _context.Users.FindAsync(employeeId);
        if (employee == null || employee.Role == RoleConstants.Admin)
            return false;

        employee.PasswordHash = hashedPassword;
        await _context.SaveChangesAsync();
        return true;
    }

    private IQueryable<User> ApplySorting(IQueryable<User> query, string? sortBy, string? sortDirection)
    {
        var validSortBy = EmployeeConstants.VALID_SORT_FIELDS.Contains(sortBy) 
            ? sortBy : EmployeeConstants.Defaults.DEFAULT_SORT_BY;
        
        var validSortDirection = EmployeeConstants.VALID_SORT_DIRECTIONS.Contains(sortDirection?.ToLower()) 
            ? sortDirection!.ToLower() : EmployeeConstants.Defaults.DEFAULT_SORT_DIRECTION;

        return (validSortBy, validSortDirection) switch
        {
            ("Username", "asc") => query.OrderBy(u => u.Username),
            ("Username", "desc") => query.OrderByDescending(u => u.Username),
            ("FullName", "asc") => query.OrderBy(u => u.FullName),
            ("FullName", "desc") => query.OrderByDescending(u => u.FullName),
            ("Email", "asc") => query.OrderBy(u => u.Email),
            ("Email", "desc") => query.OrderByDescending(u => u.Email),
            ("Role", "asc") => query.OrderBy(u => u.Role),
            ("Role", "desc") => query.OrderByDescending(u => u.Role),
            ("Status", "asc") => query.OrderBy(u => u.Status),
            ("Status", "desc") => query.OrderByDescending(u => u.Status),
            ("CreatedAt", "asc") => query.OrderBy(u => u.CreatedAt),
            _ => query.OrderByDescending(u => u.CreatedAt)
        };
    }

    private async Task<List<MonthlyActivityDto>> GetMonthlyActivitiesAsync(int employeeId)
    {
        var startDate = DateTime.Now.AddMonths(-11).Date;
        var endDate = DateTime.Now.Date.AddDays(1);

        var goodsIssues = await _context.GoodsIssues
            .Where(gi => gi.CreatedByUserId == employeeId && gi.CreatedAt >= startDate && gi.CreatedAt < endDate)
            .GroupBy(gi => new { gi.CreatedAt.Year, gi.CreatedAt.Month })
            .Select(g => new { Year = g.Key.Year, Month = g.Key.Month, Count = g.Count() })
            .ToListAsync();

        var goodsReceipts = await _context.GoodsReceipts
            .Where(gr => gr.CreatedByUserId == employeeId && gr.ReceiptDate >= startDate && gr.ReceiptDate < endDate)
            .GroupBy(gr => new { gr.ReceiptDate!.Value.Year, gr.ReceiptDate.Value.Month })
            .Select(g => new { Year = g.Key.Year, Month = g.Key.Month, Count = g.Count() })
            .ToListAsync();

        var approvals = await _context.GoodsIssues
            .Where(gi => gi.ApprovedByUserId == employeeId && gi.ApprovedDate.HasValue && gi.ApprovedDate >= startDate && gi.ApprovedDate < endDate)
            .GroupBy(gi => new { gi.ApprovedDate!.Value.Year, gi.ApprovedDate.Value.Month })
            .Select(g => new { Year = g.Key.Year, Month = g.Key.Month, Count = g.Count() })
            .ToListAsync();

        var activities = new List<MonthlyActivityDto>();
        for (var i = 0; i < 12; i++)
        {
            var date = DateTime.Now.AddMonths(-11 + i);
            var year = date.Year;
            var month = date.Month;

            var goodsIssueCount = goodsIssues.FirstOrDefault(gi => gi.Year == year && gi.Month == month)?.Count ?? 0;
            var goodsReceiptCount = goodsReceipts.FirstOrDefault(gr => gr.Year == year && gr.Month == month)?.Count ?? 0;
            var approvalCount = approvals.FirstOrDefault(a => a.Year == year && a.Month == month)?.Count ?? 0;

            activities.Add(new MonthlyActivityDto
            {
                Year = year,
                Month = month,
                MonthName = date.ToString("MMM yyyy"),
                GoodsIssuesCreated = goodsIssueCount,
                GoodsReceiptsCreated = goodsReceiptCount,
                Approvals = approvalCount,
                TotalActivities = goodsIssueCount + goodsReceiptCount + approvalCount
            });
        }

        return activities;
    }

    private DateTime? GetLastActivityDate(User employee)
    {
        var lastGoodsIssue = employee.GoodsIssues.OrderByDescending(gi => gi.CreatedAt).FirstOrDefault()?.CreatedAt;
        var lastGoodsReceipt = employee.GoodsReceipts.OrderByDescending(gr => gr.ReceiptDate).FirstOrDefault()?.ReceiptDate;

        if (lastGoodsIssue.HasValue && lastGoodsReceipt.HasValue)
        {
            return lastGoodsIssue > lastGoodsReceipt ? lastGoodsIssue : lastGoodsReceipt;
        }

        return lastGoodsIssue ?? lastGoodsReceipt;
    }

    private int CalculateDaysActive(DateTime? createdAt)
    {
        if (!createdAt.HasValue)
            return 0;

        return (DateTime.Now - createdAt.Value).Days;
    }

    private int GetTasksThisMonth(User employee)
    {
        var startOfMonth = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
        var endOfMonth = startOfMonth.AddMonths(1);

        var goodsIssuesThisMonth = employee.GoodsIssues.Count(gi => gi.CreatedAt >= startOfMonth && gi.CreatedAt < endOfMonth);
        var goodsReceiptsThisMonth = employee.GoodsReceipts.Count(gr => gr.ReceiptDate >= startOfMonth && gr.ReceiptDate < endOfMonth);

        return goodsIssuesThisMonth + goodsReceiptsThisMonth;
    }
}
