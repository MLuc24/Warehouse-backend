using WarehouseManage.DTOs.Customer;

namespace WarehouseManage.Interfaces;

public interface ICustomerService : IBaseService<CustomerDto, CreateCustomerDto, UpdateCustomerDto, CustomerSearchDto, CustomerListResponseDto>
{
    // Validation
    Task<bool> ValidateCustomerDataAsync(CreateCustomerDto createDto);
    Task<bool> ValidateCustomerDataAsync(int customerId, UpdateCustomerDto updateDto);
    Task<bool> CanDeleteCustomerAsync(int customerId);
    
    // Statistics & Analytics
    Task<CustomerStatsDto?> GetCustomerStatisticsAsync(int customerId);
    Task<List<CustomerDto>> GetTopCustomersAsync(int count = 5);
    
    // Customer type specific
    Task<List<CustomerDto>> GetCustomersByTypeAsync(string customerType);
    
    // Active customers for dropdowns
    Task<List<CustomerDto>> GetActiveCustomersAsync();
}
