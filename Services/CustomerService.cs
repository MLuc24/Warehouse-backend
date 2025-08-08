using AutoMapper;
using WarehouseManage.Constants;
using WarehouseManage.DTOs.Customer;
using WarehouseManage.Interfaces;
using WarehouseManage.Model;

namespace WarehouseManage.Services;

public class CustomerService : ICustomerService
{
    private readonly ICustomerRepository _customerRepository;
    private readonly IMapper _mapper;

    public CustomerService(ICustomerRepository customerRepository, IMapper mapper)
    {
        _customerRepository = customerRepository;
        _mapper = mapper;
    }

    public async Task<CustomerDto?> GetByIdAsync(int customerId)
    {
        var customer = await _customerRepository.GetByIdAsync(customerId);
        if (customer == null)
            return null;

        var customerDto = _mapper.Map<CustomerDto>(customer);
        customerDto.TotalOrders = customer.GoodsIssues.Count;
        customerDto.TotalPurchaseValue = customer.GoodsIssues.Sum(gi => gi.TotalAmount ?? 0);

        return customerDto;
    }

    public async Task<CustomerListResponseDto> GetAllAsync(CustomerSearchDto searchDto)
    {
        // Validate pagination parameters
        if (searchDto.Page < 1) searchDto.Page = 1;
        if (searchDto.PageSize < 1) searchDto.PageSize = CustomerConstants.DEFAULT_PAGE_SIZE;
        if (searchDto.PageSize > CustomerConstants.MAX_PAGE_SIZE) searchDto.PageSize = CustomerConstants.MAX_PAGE_SIZE;

        return await _customerRepository.GetAllAsync(searchDto);
    }

    public async Task<CustomerDto> CreateAsync(CreateCustomerDto createDto)
    {
        // Validate customer data
        await ValidateCustomerDataAsync(createDto);

        var customer = _mapper.Map<Customer>(createDto);
        var createdCustomer = await _customerRepository.CreateAsync(customer);

        return _mapper.Map<CustomerDto>(createdCustomer);
    }

    public async Task<CustomerDto?> UpdateAsync(int customerId, UpdateCustomerDto updateDto)
    {
        // Check if customer exists
        if (!await _customerRepository.ExistsAsync(customerId))
            return null;

        // Validate customer data
        await ValidateCustomerDataAsync(customerId, updateDto);

        var customer = _mapper.Map<Customer>(updateDto);
        var updatedCustomer = await _customerRepository.UpdateAsync(customerId, customer);

        return updatedCustomer != null ? _mapper.Map<CustomerDto>(updatedCustomer) : null;
    }

    public async Task<bool> DeleteAsync(int customerId)
    {
        // Check if customer exists
        if (!await _customerRepository.ExistsAsync(customerId))
            return false;

        // Change customer status to Inactive instead of deleting
        return await _customerRepository.DeleteAsync(customerId);
    }

    public async Task<bool> ReactivateAsync(int customerId)
    {
        // Check if customer exists
        if (!await _customerRepository.ExistsAsync(customerId))
            return false;

        return await _customerRepository.ReactivateAsync(customerId);
    }

    public async Task<bool> ExistsAsync(int customerId)
    {
        return await _customerRepository.ExistsAsync(customerId);
    }

    public async Task<bool> ValidateCustomerDataAsync(CreateCustomerDto createDto)
    {
        // Check for duplicate customer name
        if (await _customerRepository.ExistsByNameAsync(createDto.CustomerName))
        {
            throw new InvalidOperationException(CustomerConstants.ERROR_CUSTOMER_NAME_EXISTS);
        }

        // Check for duplicate email if provided
        if (!string.IsNullOrWhiteSpace(createDto.Email))
        {
            if (await _customerRepository.ExistsByEmailAsync(createDto.Email))
            {
                throw new InvalidOperationException(CustomerConstants.ERROR_CUSTOMER_EMAIL_EXISTS);
            }
        }

        return true;
    }

    public async Task<bool> ValidateCustomerDataAsync(int customerId, UpdateCustomerDto updateDto)
    {
        // Check for duplicate customer name (excluding current customer)
        if (await _customerRepository.ExistsByNameAsync(updateDto.CustomerName, customerId))
        {
            throw new InvalidOperationException(CustomerConstants.ERROR_CUSTOMER_NAME_EXISTS);
        }

        // Check for duplicate email if provided (excluding current customer)
        if (!string.IsNullOrWhiteSpace(updateDto.Email))
        {
            if (await _customerRepository.ExistsByEmailAsync(updateDto.Email, customerId))
            {
                throw new InvalidOperationException(CustomerConstants.ERROR_CUSTOMER_EMAIL_EXISTS);
            }
        }

        return true;
    }

    public async Task<bool> CanDeleteCustomerAsync(int customerId)
    {
        return !await _customerRepository.HasActiveOrdersAsync(customerId);
    }

    public async Task<CustomerStatsDto?> GetCustomerStatisticsAsync(int customerId)
    {
        return await _customerRepository.GetCustomerStatsAsync(customerId);
    }

    public async Task<List<CustomerDto>> GetTopCustomersAsync(int count = CustomerConstants.DEFAULT_TOP_CUSTOMERS)
    {
        return await _customerRepository.GetTopCustomersAsync(count);
    }

    public async Task<List<CustomerDto>> GetCustomersByTypeAsync(string customerType)
    {
        return await _customerRepository.GetCustomersByTypeAsync(customerType);
    }

    public async Task<List<CustomerDto>> GetActiveCustomersAsync()
    {
        return await _customerRepository.GetActiveCustomersAsync();
    }
}
