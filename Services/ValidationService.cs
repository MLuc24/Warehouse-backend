using WarehouseManage.Data;
using WarehouseManage.Extensions;
using Microsoft.EntityFrameworkCore;

namespace WarehouseManage.Services;

public class ValidationService : IValidationService
{
    private readonly WarehouseDbContext _context;
    private readonly ILogger<ValidationService> _logger;

    public ValidationService(WarehouseDbContext context, ILogger<ValidationService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<ValidationResult> ValidateUserRegistrationAsync(string username, string email, string? phoneNumber)
    {
        var errors = new List<string>();

        // Chỉ validate logic nghiệp vụ - format validation để frontend xử lý
        
        // Check username availability
        if (await _context.Users.AnyAsync(u => u.Username == username))
        {
            errors.Add("Tên đăng nhập đã được sử dụng");
        }

        // Check email availability
        if (await _context.Users.AnyAsync(u => u.Email == email))
        {
            errors.Add("Email đã được sử dụng");
        }

        // Check phone number availability nếu có
        if (!string.IsNullOrEmpty(phoneNumber))
        {
            if (await _context.Users.AnyAsync(u => u.PhoneNumber == phoneNumber))
            {
                errors.Add("Số điện thoại đã được sử dụng");
            }
        }

        return new ValidationResult
        {
            IsValid = !errors.Any(),
            Errors = errors
        };
    }

    public ValidationResult ValidateUserLogin(string identifier)
    {
        var errors = new List<string>();

        // Chỉ validate business logic - frontend đã validate format
        if (string.IsNullOrWhiteSpace(identifier))
        {
            errors.Add("Vui lòng nhập email hoặc tên đăng nhập");
        }

        return new ValidationResult
        {
            IsValid = !errors.Any(),
            Errors = errors
        };
    }
}

public class ValidationResult
{
    public bool IsValid { get; set; }
    public List<string> Errors { get; set; } = new();
}

public interface IValidationService
{
    Task<ValidationResult> ValidateUserRegistrationAsync(string username, string email, string? phoneNumber);
    ValidationResult ValidateUserLogin(string identifier);
}
