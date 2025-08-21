using AutoMapper;
using WarehouseManage.Constants;
using WarehouseManage.DTOs.Employee;
using WarehouseManage.Helpers;
using WarehouseManage.Interfaces;
using WarehouseManage.Model;

namespace WarehouseManage.Services;

public class EmployeeService : IEmployeeService
{
    private readonly IEmployeeRepository _employeeRepository;
    private readonly IMapper _mapper;

    public EmployeeService(IEmployeeRepository employeeRepository, IMapper mapper)
    {
        _employeeRepository = employeeRepository;
        _mapper = mapper;
    }

    public async Task<EmployeeDto?> GetEmployeeByIdAsync(int employeeId)
    {
        var employee = await _employeeRepository.GetByIdAsync(employeeId);
        if (employee == null)
            return null;

        var employeeDto = _mapper.Map<EmployeeDto>(employee);
        
        // Add calculated statistics
        employeeDto.TotalGoodsIssuesCreated = await _employeeRepository.GetTotalGoodsIssuesCreatedAsync(employeeId);
        employeeDto.TotalGoodsReceiptsCreated = await _employeeRepository.GetTotalGoodsReceiptsCreatedAsync(employeeId);
        employeeDto.TotalApprovals = await _employeeRepository.GetTotalApprovalsAsync(employeeId);

        return employeeDto;
    }

    public async Task<EmployeeListResponseDto> GetAllEmployeesAsync(EmployeeSearchDto searchDto)
    {
        return await _employeeRepository.GetAllAsync(searchDto);
    }

    public async Task<EmployeeDto> CreateEmployeeAsync(CreateEmployeeDto createDto)
    {
        // Validate data
        var isValid = await ValidateEmployeeDataAsync(createDto);
        if (!isValid)
        {
            throw new InvalidOperationException(EmployeeConstants.ErrorMessages.USERNAME_ALREADY_EXISTS);
        }

        // Create User entity
        var employee = _mapper.Map<User>(createDto);
        employee.PasswordHash = PasswordHelper.HashPassword(createDto.Password);
        employee.CreatedAt = DateTime.UtcNow;
        employee.Status = createDto.Status;

        var createdEmployee = await _employeeRepository.CreateAsync(employee);
        return _mapper.Map<EmployeeDto>(createdEmployee);
    }

    public async Task<EmployeeDto?> UpdateEmployeeAsync(int employeeId, UpdateEmployeeDto updateDto)
    {
        var existingEmployee = await _employeeRepository.GetByIdAsync(employeeId);
        if (existingEmployee == null)
            return null;

        // Validate data
        var isValid = await ValidateEmployeeDataAsync(employeeId, updateDto);
        if (!isValid)
        {
            throw new InvalidOperationException(EmployeeConstants.ErrorMessages.EMAIL_ALREADY_EXISTS);
        }

        // Update entity
        var employeeToUpdate = _mapper.Map<User>(updateDto);
        employeeToUpdate.UserId = employeeId;

        var updatedEmployee = await _employeeRepository.UpdateAsync(employeeId, employeeToUpdate);
        return updatedEmployee != null ? _mapper.Map<EmployeeDto>(updatedEmployee) : null;
    }

    public async Task<bool> DeleteEmployeeAsync(int employeeId)
    {
        var canDelete = await CanDeleteEmployeeAsync(employeeId);
        if (!canDelete)
            return false;

        return await _employeeRepository.DeleteAsync(employeeId);
    }

    public async Task<bool> ReactivateEmployeeAsync(int employeeId)
    {
        return await _employeeRepository.ReactivateAsync(employeeId);
    }

    public async Task<bool> UpdatePasswordAsync(int employeeId, UpdateEmployeePasswordDto passwordDto)
    {
        var hashedPassword = PasswordHelper.HashPassword(passwordDto.NewPassword);
        return await _employeeRepository.UpdatePasswordAsync(employeeId, hashedPassword);
    }

    public async Task<bool> ResetPasswordAsync(int employeeId, string newPassword)
    {
        var hashedPassword = PasswordHelper.HashPassword(newPassword);
        return await _employeeRepository.UpdatePasswordAsync(employeeId, hashedPassword);
    }

    public async Task<bool> ValidateEmployeeDataAsync(CreateEmployeeDto createDto)
    {
        // Check username uniqueness
        var usernameExists = await _employeeRepository.ExistsByUsernameAsync(createDto.Username);
        if (usernameExists)
            return false;

        // Check email uniqueness if provided
        if (!string.IsNullOrWhiteSpace(createDto.Email))
        {
            var emailExists = await _employeeRepository.ExistsByEmailAsync(createDto.Email);
            if (emailExists)
                return false;
        }

        return true;
    }

    public async Task<bool> ValidateEmployeeDataAsync(int employeeId, UpdateEmployeeDto updateDto)
    {
        // Check email uniqueness if provided
        if (!string.IsNullOrWhiteSpace(updateDto.Email))
        {
            var emailExists = await _employeeRepository.ExistsByEmailAsync(updateDto.Email, employeeId);
            if (emailExists)
                return false;
        }

        return true;
    }

    public async Task<bool> CanDeleteEmployeeAsync(int employeeId)
    {
        var employee = await _employeeRepository.GetByIdAsync(employeeId);
        if (employee == null || employee.Role == RoleConstants.Admin)
            return false;

        // Check if employee has active tasks
        var hasActiveTasks = await _employeeRepository.HasActiveTasksAsync(employeeId);
        if (hasActiveTasks)
            return false;

        return true;
    }

    public async Task<bool> CanModifyEmployeeAsync(int employeeId, int currentUserId, string currentUserRole)
    {
        var employee = await _employeeRepository.GetByIdAsync(employeeId);
        if (employee == null)
            return false;

        // Admin cannot be modified
        if (employee.Role == RoleConstants.Admin)
            return false;

        // Only Admin can modify employees
        if (currentUserRole != RoleConstants.Admin)
            return false;

        // Cannot modify self if it's the only admin (but this is employee context so should be fine)
        return true;
    }

    public async Task<EmployeeStatsDto?> GetEmployeeStatisticsAsync(int employeeId)
    {
        return await _employeeRepository.GetEmployeeStatsAsync(employeeId);
    }

    public async Task<List<EmployeeDto>> GetTopPerformersAsync(int count = 5)
    {
        return await _employeeRepository.GetTopPerformersAsync(count);
    }

    public async Task<List<EmployeeDto>> GetEmployeesByRoleAsync(string role)
    {
        var employees = await _employeeRepository.GetEmployeesByRoleAsync(role);
        return _mapper.Map<List<EmployeeDto>>(employees);
    }

    public async Task<bool> EmployeeExistsAsync(int employeeId)
    {
        return await _employeeRepository.ExistsAsync(employeeId);
    }

    public async Task<bool> IsUsernameAvailableAsync(string username, int? excludeId = null)
    {
        var exists = await _employeeRepository.ExistsByUsernameAsync(username, excludeId);
        return !exists;
    }

    public async Task<bool> IsEmailAvailableAsync(string email, int? excludeId = null)
    {
        if (string.IsNullOrWhiteSpace(email))
            return true;

        var exists = await _employeeRepository.ExistsByEmailAsync(email, excludeId);
        return !exists;
    }

    public async Task<List<EmployeeDto>> GetActiveEmployeesAsync()
    {
        var employees = await _employeeRepository.GetActiveEmployeesAsync();
        return _mapper.Map<List<EmployeeDto>>(employees);
    }

    public async Task<List<EmployeeDto>> GetManagersAsync()
    {
        var managers = await _employeeRepository.GetEmployeesByRoleAsync(EmployeeConstants.MANAGER_ROLE);
        return _mapper.Map<List<EmployeeDto>>(managers);
    }

    public async Task<bool> ActivateEmployeeAsync(int employeeId)
    {
        return await _employeeRepository.ReactivateAsync(employeeId);
    }

    public async Task<bool> DeactivateEmployeeAsync(int employeeId)
    {
        return await _employeeRepository.DeleteAsync(employeeId);
    }
}
