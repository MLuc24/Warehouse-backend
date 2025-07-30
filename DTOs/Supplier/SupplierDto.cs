using System.ComponentModel.DataAnnotations;

namespace WarehouseManage.DTOs.Supplier;

public class SupplierDto
{
    public int SupplierId { get; set; }
    public string SupplierName { get; set; } = null!;
    public string? Address { get; set; }
    public string? PhoneNumber { get; set; }
    public string? Email { get; set; }
    public string? TaxCode { get; set; }
    public DateTime? CreatedAt { get; set; }
    
    // Additional fields for management
    public int TotalProducts { get; set; }
    public int TotalReceipts { get; set; }
    public decimal? TotalPurchaseValue { get; set; }
}

public class CreateSupplierDto
{
    [Required(ErrorMessage = "Tên nhà cung cấp là bắt buộc")]
    [StringLength(200, ErrorMessage = "Tên nhà cung cấp không được vượt quá 200 ký tự")]
    public string SupplierName { get; set; } = null!;

    [StringLength(500, ErrorMessage = "Địa chỉ không được vượt quá 500 ký tự")]
    public string? Address { get; set; }

    [RegularExpression(@"^[\d\s\-\+\(\)]+$", ErrorMessage = "Số điện thoại không hợp lệ")]
    [StringLength(20, ErrorMessage = "Số điện thoại không được vượt quá 20 ký tự")]
    public string? PhoneNumber { get; set; }

    [EmailAddress(ErrorMessage = "Email không hợp lệ")]
    [StringLength(100, ErrorMessage = "Email không được vượt quá 100 ký tự")]
    public string? Email { get; set; }

    [StringLength(20, ErrorMessage = "Mã số thuế không được vượt quá 20 ký tự")]
    public string? TaxCode { get; set; }
}

public class UpdateSupplierDto
{
    [Required(ErrorMessage = "Tên nhà cung cấp là bắt buộc")]
    [StringLength(200, ErrorMessage = "Tên nhà cung cấp không được vượt quá 200 ký tự")]
    public string SupplierName { get; set; } = null!;

    [StringLength(500, ErrorMessage = "Địa chỉ không được vượt quá 500 ký tự")]
    public string? Address { get; set; }

    [RegularExpression(@"^[\d\s\-\+\(\)]+$", ErrorMessage = "Số điện thoại không hợp lệ")]
    [StringLength(20, ErrorMessage = "Số điện thoại không được vượt quá 20 ký tự")]
    public string? PhoneNumber { get; set; }

    [EmailAddress(ErrorMessage = "Email không hợp lệ")]
    [StringLength(100, ErrorMessage = "Email không được vượt quá 100 ký tự")]
    public string? Email { get; set; }

    [StringLength(20, ErrorMessage = "Mã số thuế không được vượt quá 20 ký tự")]
    public string? TaxCode { get; set; }
}

public class SupplierSearchDto
{
    public string? SearchTerm { get; set; }
    public string? Address { get; set; }
    public string? Email { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public string? SortBy { get; set; } = "SupplierName";
    public bool SortDescending { get; set; } = false;
}

public class SupplierListResponseDto
{
    public List<SupplierDto> Suppliers { get; set; } = new();
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
}

public class SupplierStatsDto
{
    public int SupplierId { get; set; }
    public string SupplierName { get; set; } = null!;
    public int TotalProducts { get; set; }
    public int TotalGoodsReceipts { get; set; }
    public decimal TotalPurchaseValue { get; set; }
    public DateTime? LastReceiptDate { get; set; }
    public DateTime? FirstReceiptDate { get; set; }
    public List<MonthlyPurchaseDto> MonthlyPurchases { get; set; } = new();
}

public class MonthlyPurchaseDto
{
    public int Year { get; set; }
    public int Month { get; set; }
    public decimal TotalValue { get; set; }
    public int ReceiptCount { get; set; }
}
