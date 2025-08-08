using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WarehouseManage.Constants;
using WarehouseManage.DTOs.Customer;
using WarehouseManage.Interfaces;

namespace WarehouseManage.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize] // Require authentication
public class CustomerController : ControllerBase
{
    private readonly ICustomerService _customerService;
    private readonly ILogger<CustomerController> _logger;

    public CustomerController(ICustomerService customerService, ILogger<CustomerController> logger)
    {
        _customerService = customerService;
        _logger = logger;
    }

    /// <summary>
    /// Lấy danh sách tất cả khách hàng (có phân trang và tìm kiếm)
    /// </summary>
    [HttpGet]
    [Authorize(Roles = "Admin,Manager,Employee")] // All roles can view
    public async Task<ActionResult<CustomerListResponseDto>> GetAllCustomers([FromQuery] CustomerSearchDto searchDto)
    {
        try
        {
            var result = await _customerService.GetAllAsync(searchDto);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting customers");
            return StatusCode(500, new { message = "Đã xảy ra lỗi khi lấy danh sách khách hàng" });
        }
    }

    /// <summary>
    /// Lấy thông tin khách hàng theo ID
    /// </summary>
    [HttpGet("{id}")]
    [Authorize(Roles = "Admin,Manager,Employee")] // All roles can view
    public async Task<ActionResult<CustomerDto>> GetCustomerById(int id)
    {
        try
        {
            var customer = await _customerService.GetByIdAsync(id);
            if (customer == null)
            {
                return NotFound(new { message = CustomerConstants.ERROR_CUSTOMER_NOT_FOUND });
            }

            return Ok(customer);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting customer {CustomerId}", id);
            return StatusCode(500, new { message = "Đã xảy ra lỗi khi lấy thông tin khách hàng" });
        }
    }

    /// <summary>
    /// Tạo khách hàng mới
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Admin,Manager")] // Admin and Manager can create
    public async Task<ActionResult<CustomerDto>> CreateCustomer([FromBody] CreateCustomerDto createDto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var customer = await _customerService.CreateAsync(createDto);
            return CreatedAtAction(nameof(GetCustomerById), new { id = customer.CustomerId }, customer);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while creating customer");
            return StatusCode(500, new { message = "Đã xảy ra lỗi khi tạo khách hàng" });
        }
    }

    /// <summary>
    /// Cập nhật thông tin khách hàng
    /// </summary>
    [HttpPut("{id}")]
    [Authorize(Roles = "Admin,Manager")] // Admin and Manager can update
    public async Task<ActionResult<CustomerDto>> UpdateCustomer(int id, [FromBody] UpdateCustomerDto updateDto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var customer = await _customerService.UpdateAsync(id, updateDto);
            if (customer == null)
            {
                return NotFound(new { message = CustomerConstants.ERROR_CUSTOMER_NOT_FOUND });
            }

            return Ok(customer);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while updating customer {CustomerId}", id);
            return StatusCode(500, new { message = "Đã xảy ra lỗi khi cập nhật khách hàng" });
        }
    }

    /// <summary>
    /// Xóa khách hàng (soft delete - chuyển trạng thái thành Inactive)
    /// </summary>
    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")] // Only Admin can delete
    public async Task<ActionResult> DeleteCustomer(int id)
    {
        try
        {
            var canDelete = await _customerService.CanDeleteCustomerAsync(id);
            if (!canDelete)
            {
                return BadRequest(new { message = CustomerConstants.ERROR_CANNOT_DELETE_CUSTOMER_WITH_ORDERS });
            }

            var result = await _customerService.DeleteAsync(id);
            if (!result)
            {
                return NotFound(new { message = CustomerConstants.ERROR_CUSTOMER_NOT_FOUND });
            }

            return Ok(new { message = CustomerConstants.SUCCESS_CUSTOMER_DELETED });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while deleting customer {CustomerId}", id);
            return StatusCode(500, new { message = "Đã xảy ra lỗi khi xóa khách hàng" });
        }
    }

    /// <summary>
    /// Kích hoạt lại khách hàng
    /// </summary>
    [HttpPost("{id}/reactivate")]
    [Authorize(Roles = "Admin")] // Only Admin can reactivate
    public async Task<ActionResult> ReactivateCustomer(int id)
    {
        try
        {
            var result = await _customerService.ReactivateAsync(id);
            if (!result)
            {
                return NotFound(new { message = CustomerConstants.ERROR_CUSTOMER_NOT_FOUND });
            }

            return Ok(new { message = CustomerConstants.SUCCESS_CUSTOMER_REACTIVATED });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while reactivating customer {CustomerId}", id);
            return StatusCode(500, new { message = "Đã xảy ra lỗi khi kích hoạt lại khách hàng" });
        }
    }

    /// <summary>
    /// Lấy thống kê của khách hàng
    /// </summary>
    [HttpGet("{id}/statistics")]
    [Authorize(Roles = "Admin,Manager")] // Admin and Manager can view statistics
    public async Task<ActionResult<CustomerStatsDto>> GetCustomerStatistics(int id)
    {
        try
        {
            var stats = await _customerService.GetCustomerStatisticsAsync(id);
            if (stats == null)
            {
                return NotFound(new { message = CustomerConstants.ERROR_CUSTOMER_NOT_FOUND });
            }

            return Ok(stats);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting customer statistics {CustomerId}", id);
            return StatusCode(500, new { message = "Đã xảy ra lỗi khi lấy thống kê khách hàng" });
        }
    }

    /// <summary>
    /// Lấy danh sách khách hàng hàng đầu (theo tổng giá trị mua hàng)
    /// </summary>
    [HttpGet("top")]
    [Authorize(Roles = "Admin,Manager")] // Admin and Manager can view
    public async Task<ActionResult<List<CustomerDto>>> GetTopCustomers([FromQuery] int count = 5)
    {
        try
        {
            if (count < 1) count = CustomerConstants.DEFAULT_TOP_CUSTOMERS;
            if (count > CustomerConstants.MAX_TOP_CUSTOMERS) count = CustomerConstants.MAX_TOP_CUSTOMERS;

            var topCustomers = await _customerService.GetTopCustomersAsync(count);
            return Ok(topCustomers);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting top customers");
            return StatusCode(500, new { message = "Đã xảy ra lỗi khi lấy danh sách khách hàng hàng đầu" });
        }
    }

    /// <summary>
    /// Lấy danh sách khách hàng đang hoạt động (cho dropdown)
    /// </summary>
    [HttpGet("active")]
    [Authorize(Roles = "Admin,Manager,Employee")] // All roles can access
    public async Task<ActionResult<List<CustomerDto>>> GetActiveCustomers()
    {
        try
        {
            var activeCustomers = await _customerService.GetActiveCustomersAsync();
            return Ok(activeCustomers);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting active customers");
            return StatusCode(500, new { message = "Đã xảy ra lỗi khi lấy danh sách khách hàng hoạt động" });
        }
    }

    /// <summary>
    /// Lấy khách hàng theo loại
    /// </summary>
    [HttpGet("by-type/{customerType}")]
    [Authorize(Roles = "Admin,Manager,Employee")] // All roles can view
    public async Task<ActionResult<List<CustomerDto>>> GetCustomersByType(string customerType)
    {
        try
        {
            var customers = await _customerService.GetCustomersByTypeAsync(customerType);
            return Ok(customers);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting customers by type {CustomerType}", customerType);
            return StatusCode(500, new { message = "Đã xảy ra lỗi khi lấy khách hàng theo loại" });
        }
    }

    /// <summary>
    /// Kiểm tra khách hàng có thể xóa hay không
    /// </summary>
    [HttpGet("{id}/can-delete")]
    [Authorize(Roles = "Admin")] // Only Admin needs this check
    public async Task<ActionResult<bool>> CanDeleteCustomer(int id)
    {
        try
        {
            var canDelete = await _customerService.CanDeleteCustomerAsync(id);
            return Ok(new { canDelete });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while checking if customer can be deleted {CustomerId}", id);
            return StatusCode(500, new { message = "Đã xảy ra lỗi khi kiểm tra khả năng xóa khách hàng" });
        }
    }
}
