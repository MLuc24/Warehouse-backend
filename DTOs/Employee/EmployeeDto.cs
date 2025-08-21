using System.ComponentModel.DataAnnotations;

namespace WarehouseManage.DTOs.Employee;

public class EmployeeDto
{
    public int UserId { get; set; }
    public string Username { get; set; } = null!;
    public string FullName { get; set; } = null!;
    public string? Email { get; set; }
    public string? PhoneNumber { get; set; }
    public string? Address { get; set; }
    public string? Image { get; set; }
    public bool IsEmailVerified { get; set; }
    public bool IsPhoneVerified { get; set; }
    public string Role { get; set; } = null!;
    public bool? Status { get; set; }
    public DateTime? CreatedAt { get; set; }
    
    // Employee specific statistics
    public int TotalGoodsIssuesCreated { get; set; }
    public int TotalGoodsReceiptsCreated { get; set; }
    public int TotalApprovals { get; set; }
    public DateTime? LastLoginDate { get; set; }
}

public class CreateEmployeeDto
{
    [Required(ErrorMessage = "Tên đăng nhập là bắt buộc")]
    [StringLength(50, MinimumLength = 3, ErrorMessage = "Tên đăng nhập phải từ 3-50 ký tự")]
    [RegularExpression(@"^[a-zA-Z0-9_]+$", ErrorMessage = "Tên đăng nhập chỉ chứa chữ cái, số và dấu gạch dưới")]
    public string Username { get; set; } = null!;

    [Required(ErrorMessage = "Mật khẩu là bắt buộc")]
    [StringLength(100, MinimumLength = 6, ErrorMessage = "Mật khẩu phải từ 6-100 ký tự")]
    public string Password { get; set; } = null!;

    [Required(ErrorMessage = "Họ tên là bắt buộc")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "Họ tên phải từ 2-100 ký tự")]
    public string FullName { get; set; } = null!;

    [EmailAddress(ErrorMessage = "Email không hợp lệ")]
    [StringLength(255, ErrorMessage = "Email không được vượt quá 255 ký tự")]
    public string? Email { get; set; }

    [Phone(ErrorMessage = "Số điện thoại không hợp lệ")]
    [StringLength(20, ErrorMessage = "Số điện thoại không được vượt quá 20 ký tự")]
    public string? PhoneNumber { get; set; }

    [StringLength(500, ErrorMessage = "Địa chỉ không được vượt quá 500 ký tự")]
    public string? Address { get; set; }

    [Required(ErrorMessage = "Vai trò là bắt buộc")]
    [RegularExpression("^(Manager|Employee)$", ErrorMessage = "Vai trò phải là Manager hoặc Employee")]
    public string Role { get; set; } = null!;

    public bool Status { get; set; } = true;
}

public class UpdateEmployeeDto
{
    [Required(ErrorMessage = "Họ tên là bắt buộc")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "Họ tên phải từ 2-100 ký tự")]
    public string FullName { get; set; } = null!;

    [EmailAddress(ErrorMessage = "Email không hợp lệ")]
    [StringLength(255, ErrorMessage = "Email không được vượt quá 255 ký tự")]
    public string? Email { get; set; }

    [Phone(ErrorMessage = "Số điện thoại không hợp lệ")]
    [StringLength(20, ErrorMessage = "Số điện thoại không được vượt quá 20 ký tự")]
    public string? PhoneNumber { get; set; }

    [StringLength(500, ErrorMessage = "Địa chỉ không được vượt quá 500 ký tự")]
    public string? Address { get; set; }

    [Required(ErrorMessage = "Vai trò là bắt buộc")]
    [RegularExpression("^(Manager|Employee)$", ErrorMessage = "Vai trò phải là Manager hoặc Employee")]
    public string Role { get; set; } = null!;

    public bool Status { get; set; } = true;
}

public class UpdateEmployeePasswordDto
{
    [Required(ErrorMessage = "Mật khẩu mới là bắt buộc")]
    [StringLength(100, MinimumLength = 6, ErrorMessage = "Mật khẩu phải từ 6-100 ký tự")]
    public string NewPassword { get; set; } = null!;

    [Compare("NewPassword", ErrorMessage = "Mật khẩu xác nhận không khớp")]
    public string ConfirmPassword { get; set; } = null!;
}

public class EmployeeSearchDto
{
    public string? SearchTerm { get; set; }
    public string? Role { get; set; }
    public bool? Status { get; set; }
    public bool? IsEmailVerified { get; set; }
    public bool? IsPhoneVerified { get; set; }
    public DateTime? CreatedFrom { get; set; }
    public DateTime? CreatedTo { get; set; }
    public string? SortBy { get; set; } = "CreatedAt";
    public string? SortDirection { get; set; } = "desc";
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}

public class EmployeeListResponseDto
{
    public List<EmployeeDto> Employees { get; set; } = new List<EmployeeDto>();
    public int TotalCount { get; set; }
    public int TotalPages { get; set; }
    public int CurrentPage { get; set; }
    public int PageSize { get; set; }
    public bool HasPreviousPage { get; set; }
    public bool HasNextPage { get; set; }
}

public class EmployeeStatsDto
{
    public int EmployeeId { get; set; }
    public string FullName { get; set; } = null!;
    public string Role { get; set; } = null!;
    
    // Activity Statistics
    public int TotalGoodsIssuesCreated { get; set; }
    public int TotalGoodsReceiptsCreated { get; set; }
    public int TotalApprovals { get; set; }
    public int TotalCompletedTasks { get; set; }
    
    // Time-based statistics
    public DateTime? LastLoginDate { get; set; }
    public DateTime? LastActivityDate { get; set; }
    public int DaysActive { get; set; }
    
    // Performance metrics
    public decimal AverageProcessingTime { get; set; }
    public int TasksThisMonth { get; set; }
    public List<MonthlyActivityDto> MonthlyActivities { get; set; } = new List<MonthlyActivityDto>();
}

public class MonthlyActivityDto
{
    public int Year { get; set; }
    public int Month { get; set; }
    public string MonthName { get; set; } = null!;
    public int GoodsIssuesCreated { get; set; }
    public int GoodsReceiptsCreated { get; set; }
    public int Approvals { get; set; }
    public int TotalActivities { get; set; }
}
