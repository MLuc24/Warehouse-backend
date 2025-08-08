using System.ComponentModel.DataAnnotations;

namespace WarehouseManage.DTOs.Customer;

public class CustomerDto
{
    public int CustomerId { get; set; }
    public string CustomerName { get; set; } = null!;
    public string? Address { get; set; }
    public string? PhoneNumber { get; set; }
    public string? Email { get; set; }
    public string CustomerType { get; set; } = "Regular";
    public string Status { get; set; } = "Active";
    public DateTime? CreatedAt { get; set; }
    
    // Additional fields for management
    public int TotalOrders { get; set; }
    public decimal? TotalPurchaseValue { get; set; }
}

public class CreateCustomerDto
{
    [Required(ErrorMessage = "Tên khách hàng là bắt buộc")]
    [StringLength(200, ErrorMessage = "Tên khách hàng không được vượt quá 200 ký tự")]
    public string CustomerName { get; set; } = null!;

    [StringLength(500, ErrorMessage = "Địa chỉ không được vượt quá 500 ký tự")]
    public string? Address { get; set; }

    [Phone(ErrorMessage = "Số điện thoại không hợp lệ")]
    [StringLength(20, ErrorMessage = "Số điện thoại không được vượt quá 20 ký tự")]
    public string? PhoneNumber { get; set; }

    [EmailAddress(ErrorMessage = "Địa chỉ email không hợp lệ")]
    [StringLength(100, ErrorMessage = "Email không được vượt quá 100 ký tự")]
    public string? Email { get; set; }

    [Required(ErrorMessage = "Loại khách hàng là bắt buộc")]
    [RegularExpression("^(Regular|VIP|Wholesale)$", ErrorMessage = "Loại khách hàng phải là Regular, VIP hoặc Wholesale")]
    public string CustomerType { get; set; } = "Regular";
}

public class UpdateCustomerDto
{
    [Required(ErrorMessage = "Tên khách hàng là bắt buộc")]
    [StringLength(200, ErrorMessage = "Tên khách hàng không được vượt quá 200 ký tự")]
    public string CustomerName { get; set; } = null!;

    [StringLength(500, ErrorMessage = "Địa chỉ không được vượt quá 500 ký tự")]
    public string? Address { get; set; }

    [Phone(ErrorMessage = "Số điện thoại không hợp lệ")]
    [StringLength(20, ErrorMessage = "Số điện thoại không được vượt quá 20 ký tự")]
    public string? PhoneNumber { get; set; }

    [EmailAddress(ErrorMessage = "Địa chỉ email không hợp lệ")]
    [StringLength(100, ErrorMessage = "Email không được vượt quá 100 ký tự")]
    public string? Email { get; set; }

    [Required(ErrorMessage = "Loại khách hàng là bắt buộc")]
    [RegularExpression("^(Regular|VIP|Wholesale)$", ErrorMessage = "Loại khách hàng phải là Regular, VIP hoặc Wholesale")]
    public string CustomerType { get; set; } = "Regular";

    [Required(ErrorMessage = "Trạng thái là bắt buộc")]
    [RegularExpression("^(Active|Inactive)$", ErrorMessage = "Trạng thái phải là Active hoặc Inactive")]
    public string Status { get; set; } = "Active";
}

public class CustomerSearchDto
{
    public string? SearchTerm { get; set; }
    public string? Address { get; set; }
    public string? Email { get; set; }
    public string? CustomerType { get; set; }
    public string? Status { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public string? SortBy { get; set; } = "CustomerName";
    public bool SortDescending { get; set; } = false;
}

public class CustomerListResponseDto
{
    public List<CustomerDto> Customers { get; set; } = new();
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
}

public class CustomerStatsDto
{
    public int CustomerId { get; set; }
    public string CustomerName { get; set; } = null!;
    public int TotalOrders { get; set; }
    public decimal TotalPurchaseValue { get; set; }
    public DateTime? LastOrderDate { get; set; }
    public DateTime? FirstOrderDate { get; set; }
    public List<MonthlyOrderDto> MonthlyOrders { get; set; } = new();
}

public class MonthlyOrderDto
{
    public int Year { get; set; }
    public int Month { get; set; }
    public int OrderCount { get; set; }
    public decimal TotalAmount { get; set; }
}
