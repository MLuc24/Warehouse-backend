using System.ComponentModel.DataAnnotations;

namespace WarehouseManage.DTOs.Product;

/// <summary>
/// DTO cho quản lý giá sản phẩm
/// </summary>
public class ProductPricingDto
{
    public int ProductId { get; set; }
    public string Sku { get; set; } = null!;
    public string ProductName { get; set; } = null!;
    public string? ImageUrl { get; set; }
    public decimal? PurchasePrice { get; set; }
    public decimal? SellingPrice { get; set; }
    public decimal? ProfitMargin => SellingPrice.HasValue && PurchasePrice.HasValue && PurchasePrice > 0 
        ? ((SellingPrice.Value - PurchasePrice.Value) / PurchasePrice.Value) * 100 
        : null;
    public decimal? ProfitAmount => SellingPrice.HasValue && PurchasePrice.HasValue 
        ? SellingPrice.Value - PurchasePrice.Value 
        : null;
    public DateTime? LastPriceUpdate { get; set; }
}

/// <summary>
/// DTO cho cập nhật giá sản phẩm
/// </summary>
public class UpdateProductPricingDto
{
    [Required(ErrorMessage = "ID sản phẩm là bắt buộc")]
    public int ProductId { get; set; }

    [Range(0, double.MaxValue, ErrorMessage = "Giá mua phải >= 0")]
    public decimal? PurchasePrice { get; set; }

    [Range(0, double.MaxValue, ErrorMessage = "Giá bán phải >= 0")]
    public decimal? SellingPrice { get; set; }

    [StringLength(500, ErrorMessage = "Lý do thay đổi giá không được vượt quá 500 ký tự")]
    public string? PriceChangeReason { get; set; }
}

/// <summary>
/// DTO cho cập nhật giá hàng loạt
/// </summary>
public class BulkUpdatePricingDto
{
    [Required(ErrorMessage = "Danh sách ID sản phẩm là bắt buộc")]
    public List<int> ProductIds { get; set; } = new();

    public PriceUpdateType UpdateType { get; set; }

    [Range(0.01, double.MaxValue, ErrorMessage = "Giá trị phải > 0")]
    public decimal Value { get; set; }

    [StringLength(500, ErrorMessage = "Lý do thay đổi giá không được vượt quá 500 ký tự")]
    public string? PriceChangeReason { get; set; }
}

/// <summary>
/// Enum cho loại cập nhật giá
/// </summary>
public enum PriceUpdateType
{
    SetPurchasePrice,           // Đặt giá mua cố định
    SetSellingPrice,            // Đặt giá bán cố định
    IncreasePurchasePercent,    // Tăng giá mua theo %
    DecreasePurchasePercent,    // Giảm giá mua theo %
    IncreaseSellingPercent,     // Tăng giá bán theo %
    DecreaseSellingPercent,     // Giảm giá bán theo %
    SetMarginPercent            // Đặt tỷ suất lợi nhuận
}

/// <summary>
/// DTO cho lịch sử thay đổi giá
/// </summary>
public class PriceHistoryDto
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public string ProductName { get; set; } = null!;
    public string Sku { get; set; } = null!;
    public decimal? OldPurchasePrice { get; set; }
    public decimal? NewPurchasePrice { get; set; }
    public decimal? OldSellingPrice { get; set; }
    public decimal? NewSellingPrice { get; set; }
    public string? Reason { get; set; }
    public string ChangedBy { get; set; } = null!;
    public DateTime ChangedAt { get; set; }
}

/// <summary>
/// DTO cho báo cáo phân tích giá
/// </summary>
public class PricingAnalysisDto
{
    public int TotalProducts { get; set; }
    public int ProductsWithoutPurchasePrice { get; set; }
    public int ProductsWithoutSellingPrice { get; set; }
    public int ProductsWithNegativeMargin { get; set; }
    public int ProductsWithHighMargin { get; set; } // > 50%
    public decimal AveragePurchasePrice { get; set; }
    public decimal AverageSellingPrice { get; set; }
    public decimal AverageMarginPercent { get; set; }
    public List<ProductPricingDto> TopProfitableProducts { get; set; } = new();
    public List<ProductPricingDto> LowMarginProducts { get; set; } = new();
}
