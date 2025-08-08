using Microsoft.EntityFrameworkCore;
using WarehouseManage.Data;
using WarehouseManage.DTOs.Customer;
using WarehouseManage.Interfaces;
using WarehouseManage.Model;

namespace WarehouseManage.Repositories;

public class CustomerRepository : ICustomerRepository
{
    private readonly WarehouseDbContext _context;

    public CustomerRepository(WarehouseDbContext context)
    {
        _context = context;
    }

    public async Task<Customer?> GetByIdAsync(int customerId)
    {
        return await _context.Customers
            .Include(c => c.GoodsIssues)
            .FirstOrDefaultAsync(c => c.CustomerId == customerId);
    }

    public async Task<CustomerListResponseDto> GetAllAsync(CustomerSearchDto searchDto)
    {
        var query = _context.Customers.AsQueryable(); // Show all customers (Active and Inactive)

        // Apply search filters
        if (!string.IsNullOrWhiteSpace(searchDto.SearchTerm))
        {
            var searchTerm = searchDto.SearchTerm.ToLower();
            query = query.Where(c => 
                c.CustomerName.ToLower().Contains(searchTerm) ||
                (c.Email != null && c.Email.ToLower().Contains(searchTerm)) ||
                (c.PhoneNumber != null && c.PhoneNumber.Contains(searchTerm)));
        }

        if (!string.IsNullOrWhiteSpace(searchDto.Address))
        {
            query = query.Where(c => c.Address != null && c.Address.ToLower().Contains(searchDto.Address.ToLower()));
        }

        if (!string.IsNullOrWhiteSpace(searchDto.Email))
        {
            query = query.Where(c => c.Email != null && c.Email.ToLower().Contains(searchDto.Email.ToLower()));
        }

        if (!string.IsNullOrWhiteSpace(searchDto.CustomerType))
        {
            query = query.Where(c => c.CustomerType == searchDto.CustomerType);
        }

        if (!string.IsNullOrWhiteSpace(searchDto.Status))
        {
            query = query.Where(c => c.Status == searchDto.Status);
        }

        // Apply sorting
        query = searchDto.SortBy?.ToLower() switch
        {
            "customername" => searchDto.SortDescending ? query.OrderByDescending(c => c.CustomerName) : query.OrderBy(c => c.CustomerName),
            "email" => searchDto.SortDescending ? query.OrderByDescending(c => c.Email) : query.OrderBy(c => c.Email),
            "customertype" => searchDto.SortDescending ? query.OrderByDescending(c => c.CustomerType) : query.OrderBy(c => c.CustomerType),
            "status" => searchDto.SortDescending ? query.OrderByDescending(c => c.Status) : query.OrderBy(c => c.Status),
            "createdat" => searchDto.SortDescending ? query.OrderByDescending(c => c.CreatedAt) : query.OrderBy(c => c.CreatedAt),
            _ => query.OrderBy(c => c.CustomerName)
        };

        var totalCount = await query.CountAsync();
        var totalPages = (int)Math.Ceiling((double)totalCount / searchDto.PageSize);

        var customers = await query
            .Include(c => c.GoodsIssues)
            .Skip((searchDto.Page - 1) * searchDto.PageSize)
            .Take(searchDto.PageSize)
            .Select(c => new CustomerDto
            {
                CustomerId = c.CustomerId,
                CustomerName = c.CustomerName,
                Address = c.Address,
                PhoneNumber = c.PhoneNumber,
                Email = c.Email,
                CustomerType = c.CustomerType,
                Status = c.Status,
                CreatedAt = c.CreatedAt,
                TotalOrders = c.GoodsIssues.Count,
                TotalPurchaseValue = c.GoodsIssues.Sum(gi => gi.TotalAmount ?? 0)
            })
            .ToListAsync();

        return new CustomerListResponseDto
        {
            Customers = customers,
            TotalCount = totalCount,
            Page = searchDto.Page,
            PageSize = searchDto.PageSize,
            TotalPages = totalPages
        };
    }

    public async Task<Customer> CreateAsync(Customer customer)
    {
        customer.CreatedAt = DateTime.Now;
        _context.Customers.Add(customer);
        await _context.SaveChangesAsync();
        return customer;
    }

    public async Task<Customer?> UpdateAsync(int customerId, Customer customer)
    {
        var existingCustomer = await _context.Customers.FindAsync(customerId);
        if (existingCustomer == null)
            return null;

        existingCustomer.CustomerName = customer.CustomerName;
        existingCustomer.Address = customer.Address;
        existingCustomer.PhoneNumber = customer.PhoneNumber;
        existingCustomer.Email = customer.Email;
        existingCustomer.CustomerType = customer.CustomerType;
        existingCustomer.Status = customer.Status;

        await _context.SaveChangesAsync();
        return existingCustomer;
    }

    public async Task<bool> DeleteAsync(int customerId)
    {
        var customer = await _context.Customers.FindAsync(customerId);
        if (customer == null)
            return false;

        // Soft delete: Change status to Inactive instead of deleting
        customer.Status = "Inactive";
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> ReactivateAsync(int customerId)
    {
        var customer = await _context.Customers.FindAsync(customerId);
        if (customer == null)
            return false;

        customer.Status = "Active";
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> ExistsAsync(int customerId)
    {
        return await _context.Customers.AnyAsync(c => c.CustomerId == customerId);
    }

    public async Task<bool> ExistsByNameAsync(string customerName, int? excludeId = null)
    {
        var query = _context.Customers.Where(c => c.CustomerName.ToLower() == customerName.ToLower());
        
        if (excludeId.HasValue)
        {
            query = query.Where(c => c.CustomerId != excludeId.Value);
        }

        return await query.AnyAsync();
    }

    public async Task<bool> ExistsByEmailAsync(string email, int? excludeId = null)
    {
        if (string.IsNullOrWhiteSpace(email))
            return false;

        var query = _context.Customers.Where(c => c.Email != null && c.Email.ToLower() == email.ToLower());
        
        if (excludeId.HasValue)
        {
            query = query.Where(c => c.CustomerId != excludeId.Value);
        }

        return await query.AnyAsync();
    }

    public async Task<CustomerStatsDto?> GetCustomerStatsAsync(int customerId)
    {
        var customer = await _context.Customers
            .Include(c => c.GoodsIssues)
            .FirstOrDefaultAsync(c => c.CustomerId == customerId);

        if (customer == null)
            return null;

        var monthlyOrders = customer.GoodsIssues
            .Where(gi => gi.IssueDate.HasValue)
            .GroupBy(gi => new { gi.IssueDate!.Value.Year, gi.IssueDate.Value.Month })
            .Select(g => new MonthlyOrderDto
            {
                Year = g.Key.Year,
                Month = g.Key.Month,
                OrderCount = g.Count(),
                TotalAmount = g.Sum(gi => gi.TotalAmount ?? 0)
            })
            .OrderBy(m => m.Year).ThenBy(m => m.Month)
            .ToList();

        return new CustomerStatsDto
        {
            CustomerId = customer.CustomerId,
            CustomerName = customer.CustomerName,
            TotalOrders = customer.GoodsIssues.Count,
            TotalPurchaseValue = customer.GoodsIssues.Sum(gi => gi.TotalAmount ?? 0),
            FirstOrderDate = customer.GoodsIssues.Where(gi => gi.IssueDate.HasValue)
                .OrderBy(gi => gi.IssueDate).FirstOrDefault()?.IssueDate,
            LastOrderDate = customer.GoodsIssues.Where(gi => gi.IssueDate.HasValue)
                .OrderByDescending(gi => gi.IssueDate).FirstOrDefault()?.IssueDate,
            MonthlyOrders = monthlyOrders
        };
    }

    public async Task<List<CustomerDto>> GetTopCustomersAsync(int count = 5)
    {
        return await _context.Customers
            .Where(c => c.Status == "Active") // Only active customers
            .Include(c => c.GoodsIssues)
            .Select(c => new CustomerDto
            {
                CustomerId = c.CustomerId,
                CustomerName = c.CustomerName,
                Address = c.Address,
                PhoneNumber = c.PhoneNumber,
                Email = c.Email,
                CustomerType = c.CustomerType,
                Status = c.Status,
                CreatedAt = c.CreatedAt,
                TotalOrders = c.GoodsIssues.Count,
                TotalPurchaseValue = c.GoodsIssues.Sum(gi => gi.TotalAmount ?? 0)
            })
            .OrderByDescending(c => c.TotalPurchaseValue)
            .Take(count)
            .ToListAsync();
    }

    public async Task<bool> HasActiveOrdersAsync(int customerId)
    {
        return await _context.GoodsIssues
            .AnyAsync(gi => gi.CustomerId == customerId);
    }

    public async Task<bool> HasGoodsIssuesAsync(int customerId)
    {
        return await _context.GoodsIssues
            .AnyAsync(gi => gi.CustomerId == customerId);
    }

    public async Task<List<GoodsIssue>> GetCustomerOrdersAsync(int customerId)
    {
        return await _context.GoodsIssues
            .Where(gi => gi.CustomerId == customerId)
            .Include(gi => gi.GoodsIssueDetails)
            .OrderByDescending(gi => gi.IssueDate)
            .ToListAsync();
    }

    public async Task<List<CustomerDto>> GetActiveCustomersAsync()
    {
        return await _context.Customers
            .Where(c => c.Status == "Active")
            .Select(c => new CustomerDto
            {
                CustomerId = c.CustomerId,
                CustomerName = c.CustomerName,
                Address = c.Address,
                PhoneNumber = c.PhoneNumber,
                Email = c.Email,
                CustomerType = c.CustomerType,
                Status = c.Status,
                CreatedAt = c.CreatedAt
            })
            .OrderBy(c => c.CustomerName)
            .ToListAsync();
    }

    public async Task<List<CustomerDto>> GetCustomersByTypeAsync(string customerType)
    {
        return await _context.Customers
            .Where(c => c.CustomerType == customerType && c.Status == "Active")
            .Include(c => c.GoodsIssues)
            .Select(c => new CustomerDto
            {
                CustomerId = c.CustomerId,
                CustomerName = c.CustomerName,
                Address = c.Address,
                PhoneNumber = c.PhoneNumber,
                Email = c.Email,
                CustomerType = c.CustomerType,
                Status = c.Status,
                CreatedAt = c.CreatedAt,
                TotalOrders = c.GoodsIssues.Count,
                TotalPurchaseValue = c.GoodsIssues.Sum(gi => gi.TotalAmount ?? 0)
            })
            .OrderBy(c => c.CustomerName)
            .ToListAsync();
    }
}
