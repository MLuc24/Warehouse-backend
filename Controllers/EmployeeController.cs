using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WarehouseManage.Constants;
using WarehouseManage.DTOs.Employee;
using WarehouseManage.Interfaces;

namespace WarehouseManage.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize] // Require authentication
public class EmployeeController : ControllerBase
{
    private readonly IEmployeeService _employeeService;
    private readonly ILogger<EmployeeController> _logger;

    public EmployeeController(IEmployeeService employeeService, ILogger<EmployeeController> logger)
    {
        _employeeService = employeeService;
        _logger = logger;
    }

    /// <summary>
    /// Lấy danh sách tất cả nhân viên (có phân trang và tìm kiếm)
    /// </summary>
    [HttpGet]
    [Authorize(Roles = "Admin,Manager")] // Only Admin and Manager can view employees
    public async Task<ActionResult<EmployeeListResponseDto>> GetAllEmployees([FromQuery] EmployeeSearchDto searchDto)
    {
        try
        {
            var result = await _employeeService.GetAllEmployeesAsync(searchDto);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting employees");
            return StatusCode(500, new { message = "Đã xảy ra lỗi khi lấy danh sách nhân viên" });
        }
    }

    /// <summary>
    /// Lấy thông tin nhân viên theo ID
    /// </summary>
    [HttpGet("{id}")]
    [Authorize(Roles = "Admin,Manager")] // Only Admin and Manager can view employee details
    public async Task<ActionResult<EmployeeDto>> GetEmployeeById(int id)
    {
        try
        {
            if (id <= 0)
            {
                return BadRequest(new { message = EmployeeConstants.ErrorMessages.INVALID_EMPLOYEE_ID });
            }

            var employee = await _employeeService.GetEmployeeByIdAsync(id);
            if (employee == null)
            {
                return NotFound(new { message = EmployeeConstants.ErrorMessages.EMPLOYEE_NOT_FOUND });
            }

            return Ok(employee);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting employee {EmployeeId}", id);
            return StatusCode(500, new { message = "Đã xảy ra lỗi khi lấy thông tin nhân viên" });
        }
    }

    /// <summary>
    /// Tạo nhân viên mới
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Admin")] // Only Admin can create employees
    public async Task<ActionResult<EmployeeDto>> CreateEmployee([FromBody] CreateEmployeeDto createDto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var employee = await _employeeService.CreateEmployeeAsync(createDto);
            return CreatedAtAction(nameof(GetEmployeeById), new { id = employee.UserId }, employee);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while creating employee");
            return StatusCode(500, new { message = "Đã xảy ra lỗi khi tạo nhân viên" });
        }
    }

    /// <summary>
    /// Cập nhật thông tin nhân viên
    /// </summary>
    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")] // Only Admin can update employees
    public async Task<ActionResult<EmployeeDto>> UpdateEmployee(int id, [FromBody] UpdateEmployeeDto updateDto)
    {
        try
        {
            if (id <= 0)
            {
                return BadRequest(new { message = EmployeeConstants.ErrorMessages.INVALID_EMPLOYEE_ID });
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var employee = await _employeeService.UpdateEmployeeAsync(id, updateDto);
            if (employee == null)
            {
                return NotFound(new { message = EmployeeConstants.ErrorMessages.EMPLOYEE_NOT_FOUND });
            }

            return Ok(employee);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while updating employee {EmployeeId}", id);
            return StatusCode(500, new { message = "Đã xảy ra lỗi khi cập nhật nhân viên" });
        }
    }

    /// <summary>
    /// Xóa nhân viên (soft delete)
    /// </summary>
    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")] // Only Admin can delete employees
    public async Task<ActionResult> DeleteEmployee(int id)
    {
        try
        {
            if (id <= 0)
            {
                return BadRequest(new { message = EmployeeConstants.ErrorMessages.INVALID_EMPLOYEE_ID });
            }

            var exists = await _employeeService.EmployeeExistsAsync(id);
            if (!exists)
            {
                return NotFound(new { message = EmployeeConstants.ErrorMessages.EMPLOYEE_NOT_FOUND });
            }

            var canDelete = await _employeeService.CanDeleteEmployeeAsync(id);
            if (!canDelete)
            {
                return BadRequest(new { message = EmployeeConstants.ErrorMessages.CANNOT_DELETE_EMPLOYEE });
            }

            var deleted = await _employeeService.DeleteEmployeeAsync(id);
            if (!deleted)
            {
                return BadRequest(new { message = EmployeeConstants.ErrorMessages.CANNOT_DELETE_EMPLOYEE });
            }

            return Ok(new { message = EmployeeConstants.SuccessMessages.EMPLOYEE_DELETED });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while deleting employee {EmployeeId}", id);
            return StatusCode(500, new { message = "Đã xảy ra lỗi khi xóa nhân viên" });
        }
    }

    /// <summary>
    /// Khôi phục nhân viên
    /// </summary>
    [HttpPatch("{id}/reactivate")]
    [Authorize(Roles = "Admin")] // Only Admin can reactivate employees
    public async Task<ActionResult> ReactivateEmployee(int id)
    {
        try
        {
            if (id <= 0)
            {
                return BadRequest(new { message = EmployeeConstants.ErrorMessages.INVALID_EMPLOYEE_ID });
            }

            var reactivated = await _employeeService.ReactivateEmployeeAsync(id);
            if (!reactivated)
            {
                return NotFound(new { message = EmployeeConstants.ErrorMessages.EMPLOYEE_NOT_FOUND });
            }

            return Ok(new { message = EmployeeConstants.SuccessMessages.EMPLOYEE_REACTIVATED });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while reactivating employee {EmployeeId}", id);
            return StatusCode(500, new { message = "Đã xảy ra lỗi khi khôi phục nhân viên" });
        }
    }

    /// <summary>
    /// Đặt lại mật khẩu nhân viên
    /// </summary>
    [HttpPatch("{id}/password")]
    [Authorize(Roles = "Admin")] // Only Admin can reset passwords
    public async Task<ActionResult> UpdateEmployeePassword(int id, [FromBody] UpdateEmployeePasswordDto passwordDto)
    {
        try
        {
            if (id <= 0)
            {
                return BadRequest(new { message = EmployeeConstants.ErrorMessages.INVALID_EMPLOYEE_ID });
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var exists = await _employeeService.EmployeeExistsAsync(id);
            if (!exists)
            {
                return NotFound(new { message = EmployeeConstants.ErrorMessages.EMPLOYEE_NOT_FOUND });
            }

            var updated = await _employeeService.UpdatePasswordAsync(id, passwordDto);
            if (!updated)
            {
                return BadRequest(new { message = "Không thể cập nhật mật khẩu" });
            }

            return Ok(new { message = EmployeeConstants.SuccessMessages.PASSWORD_RESET });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while updating password for employee {EmployeeId}", id);
            return StatusCode(500, new { message = "Đã xảy ra lỗi khi đặt lại mật khẩu" });
        }
    }

    /// <summary>
    /// Lấy thống kê chi tiết của nhân viên
    /// </summary>
    [HttpGet("{id}/statistics")]
    [Authorize(Roles = "Admin,Manager")] // Only Admin and Manager can view statistics
    public async Task<ActionResult<EmployeeStatsDto>> GetEmployeeStatistics(int id)
    {
        try
        {
            if (id <= 0)
            {
                return BadRequest(new { message = EmployeeConstants.ErrorMessages.INVALID_EMPLOYEE_ID });
            }

            var stats = await _employeeService.GetEmployeeStatisticsAsync(id);
            if (stats == null)
            {
                return NotFound(new { message = EmployeeConstants.ErrorMessages.EMPLOYEE_NOT_FOUND });
            }

            return Ok(stats);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting statistics for employee {EmployeeId}", id);
            return StatusCode(500, new { message = "Đã xảy ra lỗi khi lấy thống kê nhân viên" });
        }
    }

    /// <summary>
    /// Lấy danh sách nhân viên hiệu suất cao nhất
    /// </summary>
    [HttpGet("top-performers")]
    [Authorize(Roles = "Admin,Manager")] // Only Admin and Manager can view top performers
    public async Task<ActionResult<List<EmployeeDto>>> GetTopPerformers([FromQuery] int count = 5)
    {
        try
        {
            if (count <= 0 || count > 50)
            {
                count = 5;
            }

            var topPerformers = await _employeeService.GetTopPerformersAsync(count);
            return Ok(topPerformers);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting top performers");
            return StatusCode(500, new { message = "Đã xảy ra lỗi khi lấy danh sách nhân viên xuất sắc" });
        }
    }

    /// <summary>
    /// Lấy danh sách nhân viên theo vai trò
    /// </summary>
    [HttpGet("by-role/{role}")]
    [Authorize(Roles = "Admin,Manager")] // Only Admin and Manager can filter by role
    public async Task<ActionResult<List<EmployeeDto>>> GetEmployeesByRole(string role)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(role))
            {
                return BadRequest(new { message = EmployeeConstants.ErrorMessages.INVALID_ROLE });
            }

            // Validate role
            if (role != EmployeeConstants.MANAGER_ROLE && role != EmployeeConstants.EMPLOYEE_ROLE)
            {
                return BadRequest(new { message = EmployeeConstants.ErrorMessages.INVALID_ROLE });
            }

            var employees = await _employeeService.GetEmployeesByRoleAsync(role);
            return Ok(employees);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting employees by role {Role}", role);
            return StatusCode(500, new { message = "Đã xảy ra lỗi khi lấy danh sách nhân viên theo vai trò" });
        }
    }

    /// <summary>
    /// Lấy danh sách nhân viên đang hoạt động (cho dropdown)
    /// </summary>
    [HttpGet("active")]
    [Authorize(Roles = "Admin,Manager,Employee")] // All roles can view active employees for dropdowns
    public async Task<ActionResult<List<EmployeeDto>>> GetActiveEmployees()
    {
        try
        {
            var activeEmployees = await _employeeService.GetActiveEmployeesAsync();
            return Ok(activeEmployees);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting active employees");
            return StatusCode(500, new { message = "Đã xảy ra lỗi khi lấy danh sách nhân viên đang hoạt động" });
        }
    }

    /// <summary>
    /// Lấy danh sách quản lý (cho dropdown)
    /// </summary>
    [HttpGet("managers")]
    [Authorize(Roles = "Admin,Manager,Employee")] // All roles can view managers for assignments
    public async Task<ActionResult<List<EmployeeDto>>> GetManagers()
    {
        try
        {
            var managers = await _employeeService.GetManagersAsync();
            return Ok(managers);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting managers");
            return StatusCode(500, new { message = "Đã xảy ra lỗi khi lấy danh sách quản lý" });
        }
    }

    /// <summary>
    /// Kích hoạt nhân viên
    /// </summary>
    [HttpPatch("{id}/activate")]
    [Authorize(Roles = "Admin")] // Only Admin can activate employees
    public async Task<ActionResult> ActivateEmployee(int id)
    {
        try
        {
            if (id <= 0)
            {
                return BadRequest(new { message = EmployeeConstants.ErrorMessages.INVALID_EMPLOYEE_ID });
            }

            var activated = await _employeeService.ActivateEmployeeAsync(id);
            if (!activated)
            {
                return NotFound(new { message = EmployeeConstants.ErrorMessages.EMPLOYEE_NOT_FOUND });
            }

            return Ok(new { message = EmployeeConstants.SuccessMessages.STATUS_UPDATED });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while activating employee {EmployeeId}", id);
            return StatusCode(500, new { message = "Đã xảy ra lỗi khi kích hoạt nhân viên" });
        }
    }

    /// <summary>
    /// Vô hiệu hóa nhân viên
    /// </summary>
    [HttpPatch("{id}/deactivate")]
    [Authorize(Roles = "Admin")] // Only Admin can deactivate employees
    public async Task<ActionResult> DeactivateEmployee(int id)
    {
        try
        {
            if (id <= 0)
            {
                return BadRequest(new { message = EmployeeConstants.ErrorMessages.INVALID_EMPLOYEE_ID });
            }

            var exists = await _employeeService.EmployeeExistsAsync(id);
            if (!exists)
            {
                return NotFound(new { message = EmployeeConstants.ErrorMessages.EMPLOYEE_NOT_FOUND });
            }

            var canDeactivate = await _employeeService.CanDeleteEmployeeAsync(id);
            if (!canDeactivate)
            {
                return BadRequest(new { message = EmployeeConstants.ErrorMessages.CANNOT_DELETE_EMPLOYEE });
            }

            var deactivated = await _employeeService.DeactivateEmployeeAsync(id);
            if (!deactivated)
            {
                return BadRequest(new { message = "Không thể vô hiệu hóa nhân viên" });
            }

            return Ok(new { message = EmployeeConstants.SuccessMessages.STATUS_UPDATED });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while deactivating employee {EmployeeId}", id);
            return StatusCode(500, new { message = "Đã xảy ra lỗi khi vô hiệu hóa nhân viên" });
        }
    }

    /// <summary>
    /// Kiểm tra username có khả dụng không
    /// </summary>
    [HttpGet("check-username/{username}")]
    [Authorize(Roles = "Admin")] // Only Admin needs this for creating employees
    public async Task<ActionResult<bool>> CheckUsernameAvailability(string username, [FromQuery] int? excludeId = null)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(username))
            {
                return BadRequest(new { message = "Tên đăng nhập không được để trống" });
            }

            var isAvailable = await _employeeService.IsUsernameAvailableAsync(username, excludeId);
            return Ok(new { available = isAvailable });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while checking username availability for {Username}", username);
            return StatusCode(500, new { message = "Đã xảy ra lỗi khi kiểm tra tên đăng nhập" });
        }
    }

    /// <summary>
    /// Kiểm tra email có khả dụng không
    /// </summary>
    [HttpGet("check-email/{email}")]
    [Authorize(Roles = "Admin")] // Only Admin needs this for creating employees
    public async Task<ActionResult<bool>> CheckEmailAvailability(string email, [FromQuery] int? excludeId = null)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                return Ok(new { available = true });
            }

            var isAvailable = await _employeeService.IsEmailAvailableAsync(email, excludeId);
            return Ok(new { available = isAvailable });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while checking email availability for {Email}", email);
            return StatusCode(500, new { message = "Đã xảy ra lỗi khi kiểm tra email" });
        }
    }
}
