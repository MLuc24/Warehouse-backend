using System.ComponentModel.DataAnnotations;

namespace WarehouseManage.DTOs.Product;

/// <summary>
/// DTO cho quản lý hạn sử dụng sản phẩm
/// </summary>
public class ProductExpiryDto
{
    public int ProductId { get; set; }
    public string Sku { get; set; } = null!;
    public string ProductName { get; set; } = null!;
    public string? Category { get; set; }
    public DateTime? ExpiryDate { get; set; }
    public bool IsPerishable { get; set; }
    public string? StorageType { get; set; }
    public int CurrentStock { get; set; }
    public string? Unit { get; set; }  // Added Unit field
    public int DaysUntilExpiry => ExpiryDate.HasValue ? (int)(ExpiryDate.Value - DateTime.Now).TotalDays : int.MaxValue;
    public ExpiryStatus Status => GetExpiryStatus();
    
    private ExpiryStatus GetExpiryStatus()
    {
        if (!ExpiryDate.HasValue) return ExpiryStatus.NoExpiryDate;
        
        var daysLeft = DaysUntilExpiry;
        if (daysLeft < 0) return ExpiryStatus.Expired;
        if (daysLeft <= 7) return ExpiryStatus.ExpiringSoon;
        if (daysLeft <= 30) return ExpiryStatus.ExpiringWithinMonth;
        
        return ExpiryStatus.Fresh;
    }
}

/// <summary>
/// Enum cho trạng thái hạn sử dụng
/// </summary>
public enum ExpiryStatus
{
    NoExpiryDate,         // Không có hạn sử dụng
    Fresh,                // Còn mới (> 30 ngày)
    ExpiringWithinMonth,  // Sắp hết hạn trong tháng
    ExpiringSoon,         // Sắp hết hạn (≤ 7 ngày)
    Expired               // Đã hết hạn
}

/// <summary>
/// DTO cho cập nhật thông tin hạn sử dụng
/// </summary>
public class UpdateProductExpiryDto
{
    [Required(ErrorMessage = "ID sản phẩm là bắt buộc")]
    public int ProductId { get; set; }

    public DateTime? ExpiryDate { get; set; }

    public bool IsPerishable { get; set; }

    [StringLength(50, ErrorMessage = "Loại lưu trữ không được vượt quá 50 ký tự")]
    public string? StorageType { get; set; }
}

/// <summary>
/// DTO cho báo cáo hạn sử dụng
/// </summary>
public class ExpiryReportDto
{
    public int TotalPerishableProducts { get; set; }
    public int ExpiredProducts { get; set; }
    public int ExpiringSoonProducts { get; set; }
    public int ExpiringWithinMonthProducts { get; set; }
    public decimal TotalExpiredValue { get; set; }
    public decimal TotalExpiringSoonValue { get; set; }
    public List<ProductExpiryDto> ExpiredItems { get; set; } = new();
    public List<ProductExpiryDto> ExpiringSoonItems { get; set; } = new();
    public List<ProductExpiryDto> ExpiringWithinMonthItems { get; set; } = new();
}

/// <summary>
/// DTO cho tìm kiếm sản phẩm theo hạn sử dụng
/// </summary>
public class ExpirySearchDto
{
    public ExpiryStatus? Status { get; set; }
    public DateTime? ExpiryFromDate { get; set; }
    public DateTime? ExpiryToDate { get; set; }
    public string? Category { get; set; }
    public string? StorageType { get; set; }
    public bool? IsPerishable { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public string SortBy { get; set; } = "ExpiryDate";
    public bool SortDescending { get; set; } = false;
}

/// <summary>
/// DTO cho cảnh báo hạn sử dụng
/// </summary>
public class ExpiryAlertDto
{
    public int ProductId { get; set; }
    public string Sku { get; set; } = null!;
    public string ProductName { get; set; } = null!;
    public DateTime? ExpiryDate { get; set; }
    public int CurrentStock { get; set; }
    public string? Unit { get; set; }  // Added Unit field
    public decimal? TotalValue { get; set; }
    public ExpiryStatus Status { get; set; }
    public int DaysUntilExpiry { get; set; }
    public string AlertLevel { get; set; } = null!;
    public DateTime AlertCreatedAt { get; set; }
}

/// <summary>
/// DTO cho cài đặt cảnh báo hạn sử dụng
/// </summary>
public class ExpiryAlertSettingsDto
{
    [Range(1, 365, ErrorMessage = "Số ngày cảnh báo phải từ 1 đến 365")]
    public int DaysBeforeExpiry { get; set; } = 7;

    [Range(1, 365, ErrorMessage = "Số ngày cảnh báo sớm phải từ 1 đến 365")]
    public int EarlyWarningDays { get; set; } = 30;

    public bool EnableEmailNotification { get; set; } = true;
    
    public bool EnableSystemNotification { get; set; } = true;
    
    public List<string> NotificationEmails { get; set; } = new();
}
