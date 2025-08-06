using System.ComponentModel.DataAnnotations;

namespace WarehouseManage.DTOs.Product;

/// <summary>
/// DTO cho thông tin tồn kho sản phẩm
/// </summary>
public class ProductStockDto
{
    public int ProductId { get; set; }
    public string Sku { get; set; } = null!;
    public string ProductName { get; set; } = null!;
    public string? ImageUrl { get; set; }
    public string? Category { get; set; }
    public int CurrentStock { get; set; }
    public int? MinStockLevel { get; set; }
    public int? MaxStockLevel { get; set; }
    public bool IsLowStock => MinStockLevel.HasValue && CurrentStock <= MinStockLevel.Value;
    public bool IsOutOfStock => CurrentStock <= 0;
    public string StockStatus => IsOutOfStock ? "Hết hàng" : IsLowStock ? "Sắp hết" : "Đủ hàng";
    public decimal? PurchasePrice { get; set; }
    public decimal TotalValue => (PurchasePrice ?? 0) * CurrentStock;
}

/// <summary>
/// DTO cho cập nhật thông tin tồn kho
/// </summary>
public class UpdateStockLevelsDto
{
    [Required(ErrorMessage = "ID sản phẩm là bắt buộc")]
    public int ProductId { get; set; }

    [Range(0, int.MaxValue, ErrorMessage = "Mức tồn kho tối thiểu phải >= 0")]
    public int? MinStockLevel { get; set; }

    [Range(0, int.MaxValue, ErrorMessage = "Mức tồn kho tối đa phải >= 0")]
    public int? MaxStockLevel { get; set; }
}

/// <summary>
/// DTO cho điều chỉnh tồn kho
/// </summary>
public class StockAdjustmentDto
{
    [Required(ErrorMessage = "ID sản phẩm là bắt buộc")]
    public int ProductId { get; set; }

    [Required(ErrorMessage = "Loại điều chỉnh là bắt buộc")]
    public StockAdjustmentType AdjustmentType { get; set; }

    [Required(ErrorMessage = "Số lượng điều chỉnh là bắt buộc")]
    [Range(1, int.MaxValue, ErrorMessage = "Số lượng điều chỉnh phải > 0")]
    public int Quantity { get; set; }

    [Required(ErrorMessage = "Lý do điều chỉnh là bắt buộc")]
    [StringLength(500, ErrorMessage = "Lý do không được vượt quá 500 ký tự")]
    public string Reason { get; set; } = null!;

    public string? Notes { get; set; }
}

/// <summary>
/// Enum cho loại điều chỉnh tồn kho
/// </summary>
public enum StockAdjustmentType
{
    Increase,  // Tăng
    Decrease   // Giảm
}

/// <summary>
/// DTO cho báo cáo tồn kho
/// </summary>
public class StockReportDto
{
    public int TotalProducts { get; set; }
    public int LowStockProducts { get; set; }
    public int OutOfStockProducts { get; set; }
    public int OverStockProducts { get; set; }
    public decimal TotalInventoryValue { get; set; }
    public List<ProductStockDto> LowStockItems { get; set; } = new();
    public List<ProductStockDto> OutOfStockItems { get; set; } = new();
}

/// <summary>
/// DTO cho lịch sử thay đổi tồn kho
/// </summary>
public class StockHistoryDto
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public string ProductName { get; set; } = null!;
    public string Sku { get; set; } = null!;
    public StockAdjustmentType AdjustmentType { get; set; }
    public int Quantity { get; set; }
    public int StockBefore { get; set; }
    public int StockAfter { get; set; }
    public string Reason { get; set; } = null!;
    public string? Notes { get; set; }
    public string CreatedBy { get; set; } = null!;
    public DateTime CreatedAt { get; set; }
}
