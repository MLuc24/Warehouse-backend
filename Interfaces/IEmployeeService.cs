using WarehouseManage.DTOs.Employee;

namespace WarehouseManage.Interfaces;

public interface IEmployeeService
{
    // CRUD Operations
    Task<EmployeeDto?> GetEmployeeByIdAsync(int employeeId);
    Task<EmployeeListResponseDto> GetAllEmployeesAsync(EmployeeSearchDto searchDto);
    Task<EmployeeDto> CreateEmployeeAsync(CreateEmployeeDto createDto);
    Task<EmployeeDto?> UpdateEmployeeAsync(int employeeId, UpdateEmployeeDto updateDto);
    Task<bool> DeleteEmployeeAsync(int employeeId);
    Task<bool> ReactivateEmployeeAsync(int employeeId);
    
    // Password Management
    Task<bool> UpdatePasswordAsync(int employeeId, UpdateEmployeePasswordDto passwordDto);
    Task<bool> ResetPasswordAsync(int employeeId, string newPassword);
    
    // Validation
    Task<bool> ValidateEmployeeDataAsync(CreateEmployeeDto createDto);
    Task<bool> ValidateEmployeeDataAsync(int employeeId, UpdateEmployeeDto updateDto);
    Task<bool> CanDeleteEmployeeAsync(int employeeId);
    Task<bool> CanModifyEmployeeAsync(int employeeId, int currentUserId, string currentUserRole);
    
    // Statistics & Analytics
    Task<EmployeeStatsDto?> GetEmployeeStatisticsAsync(int employeeId);
    Task<List<EmployeeDto>> GetTopPerformersAsync(int count = 5);
    Task<List<EmployeeDto>> GetEmployeesByRoleAsync(string role);
    
    // Business Logic
    Task<bool> EmployeeExistsAsync(int employeeId);
    Task<bool> IsUsernameAvailableAsync(string username, int? excludeId = null);
    Task<bool> IsEmailAvailableAsync(string email, int? excludeId = null);
    
    // Active employees for dropdowns
    Task<List<EmployeeDto>> GetActiveEmployeesAsync();
    Task<List<EmployeeDto>> GetManagersAsync();
    
    // Status management
    Task<bool> ActivateEmployeeAsync(int employeeId);
    Task<bool> DeactivateEmployeeAsync(int employeeId);
}
