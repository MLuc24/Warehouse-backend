using WarehouseManage.DTOs.Employee;
using WarehouseManage.Model;

namespace WarehouseManage.Interfaces;

public interface IEmployeeRepository
{
    // CRUD Operations
    Task<User?> GetByIdAsync(int employeeId);
    Task<EmployeeListResponseDto> GetAllAsync(EmployeeSearchDto searchDto);
    Task<User> CreateAsync(User employee);
    Task<User?> UpdateAsync(int employeeId, User employee);
    Task<bool> DeleteAsync(int employeeId);
    Task<bool> ReactivateAsync(int employeeId);
    
    // Additional queries
    Task<bool> ExistsAsync(int employeeId);
    Task<bool> ExistsByUsernameAsync(string username, int? excludeId = null);
    Task<bool> ExistsByEmailAsync(string email, int? excludeId = null);
    
    // Employee specific queries
    Task<List<User>> GetEmployeesByRoleAsync(string role);
    Task<List<User>> GetActiveEmployeesAsync();
    Task<User?> GetByUsernameAsync(string username);
    
    // Statistics
    Task<EmployeeStatsDto?> GetEmployeeStatsAsync(int employeeId);
    Task<List<EmployeeDto>> GetTopPerformersAsync(int count = 5);
    
    // Relationships and Activity
    Task<bool> HasActiveTasksAsync(int employeeId);
    Task<bool> HasCreatedGoodsIssuesAsync(int employeeId);
    Task<bool> HasCreatedGoodsReceiptsAsync(int employeeId);
    Task<int> GetTotalGoodsIssuesCreatedAsync(int employeeId);
    Task<int> GetTotalGoodsReceiptsCreatedAsync(int employeeId);
    Task<int> GetTotalApprovalsAsync(int employeeId);
    
    // Password management
    Task<bool> UpdatePasswordAsync(int employeeId, string hashedPassword);
}
