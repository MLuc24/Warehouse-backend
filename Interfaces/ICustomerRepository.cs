using WarehouseManage.DTOs.Customer;
using WarehouseManage.Model;

namespace WarehouseManage.Interfaces;

public interface ICustomerRepository : IBaseRepository<Customer, CustomerSearchDto, CustomerListResponseDto>
{
    // Statistics
    Task<CustomerStatsDto?> GetCustomerStatsAsync(int customerId);
    Task<List<CustomerDto>> GetTopCustomersAsync(int count = 5);
    
    // Relationships
    Task<bool> HasActiveOrdersAsync(int customerId);
    Task<bool> HasGoodsIssuesAsync(int customerId);
    Task<List<GoodsIssue>> GetCustomerOrdersAsync(int customerId);
    
    // Active customers for dropdowns/selection
    Task<List<CustomerDto>> GetActiveCustomersAsync();
    
    // Customer type specific queries
    Task<List<CustomerDto>> GetCustomersByTypeAsync(string customerType);
}
