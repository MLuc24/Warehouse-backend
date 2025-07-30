using System.ComponentModel.DataAnnotations;

namespace WarehouseManage.DTOs.Product;

public class ProductDto
{
    public int ProductId { get; set; }
    public string Sku { get; set; } = null!;
    public string ProductName { get; set; } = null!;
    public string? Description { get; set; }
    public int? SupplierId { get; set; }
    public string? SupplierName { get; set; } // For display purposes
    public string? Unit { get; set; }
    public decimal? PurchasePrice { get; set; }
    public decimal? SellingPrice { get; set; }
    public string? ImageUrl { get; set; }
    public bool? Status { get; set; }
    public DateTime? CreatedAt { get; set; }
    
    // Additional fields for management
    public int CurrentStock { get; set; }
    public int TotalIssued { get; set; }
    public int TotalReceived { get; set; }
    public decimal? TotalValue { get; set; }
}

public class CreateProductDto
{
    [Required(ErrorMessage = "Mã SKU là bắt buộc")]
    [StringLength(50, ErrorMessage = "Mã SKU không được vượt quá 50 ký tự")]
    public string Sku { get; set; } = null!;

    [Required(ErrorMessage = "Tên sản phẩm là bắt buộc")]
    [StringLength(200, ErrorMessage = "Tên sản phẩm không được vượt quá 200 ký tự")]
    public string ProductName { get; set; } = null!;

    [StringLength(1000, ErrorMessage = "Mô tả không được vượt quá 1000 ký tự")]
    public string? Description { get; set; }

    public int? SupplierId { get; set; }

    [StringLength(50, ErrorMessage = "Đơn vị tính không được vượt quá 50 ký tự")]
    public string? Unit { get; set; }

    [Range(0, double.MaxValue, ErrorMessage = "Giá mua phải lớn hơn hoặc bằng 0")]
    public decimal? PurchasePrice { get; set; }

    [Range(0, double.MaxValue, ErrorMessage = "Giá bán phải lớn hơn hoặc bằng 0")]
    public decimal? SellingPrice { get; set; }

    [Url(ErrorMessage = "URL hình ảnh không hợp lệ")]
    [StringLength(500, ErrorMessage = "URL hình ảnh không được vượt quá 500 ký tự")]
    public string? ImageUrl { get; set; }

    public bool? Status { get; set; } = true;
}

public class UpdateProductDto
{
    [Required(ErrorMessage = "Mã SKU là bắt buộc")]
    [StringLength(50, ErrorMessage = "Mã SKU không được vượt quá 50 ký tự")]
    public string Sku { get; set; } = null!;

    [Required(ErrorMessage = "Tên sản phẩm là bắt buộc")]
    [StringLength(200, ErrorMessage = "Tên sản phẩm không được vượt quá 200 ký tự")]
    public string ProductName { get; set; } = null!;

    [StringLength(1000, ErrorMessage = "Mô tả không được vượt quá 1000 ký tự")]
    public string? Description { get; set; }

    public int? SupplierId { get; set; }

    [StringLength(50, ErrorMessage = "Đơn vị tính không được vượt quá 50 ký tự")]
    public string? Unit { get; set; }

    [Range(0, double.MaxValue, ErrorMessage = "Giá mua phải lớn hơn hoặc bằng 0")]
    public decimal? PurchasePrice { get; set; }

    [Range(0, double.MaxValue, ErrorMessage = "Giá bán phải lớn hơn hoặc bằng 0")]
    public decimal? SellingPrice { get; set; }

    [Url(ErrorMessage = "URL hình ảnh không hợp lệ")]
    [StringLength(500, ErrorMessage = "URL hình ảnh không được vượt quá 500 ký tự")]
    public string? ImageUrl { get; set; }

    public bool? Status { get; set; }
}

public class ProductSearchDto
{
    public string? SearchTerm { get; set; }
    public string? Sku { get; set; }
    public int? SupplierId { get; set; }
    public string? Unit { get; set; }
    public decimal? MinPrice { get; set; }
    public decimal? MaxPrice { get; set; }
    public bool? Status { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public string? SortBy { get; set; } = "ProductName";
    public bool SortDescending { get; set; } = false;
}

public class ProductListResponseDto
{
    public List<ProductDto> Products { get; set; } = new();
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
}

public class ProductStatsDto
{
    public int ProductId { get; set; }
    public string ProductName { get; set; } = null!;
    public string Sku { get; set; } = null!;
    public int CurrentStock { get; set; }
    public int TotalReceived { get; set; }
    public int TotalIssued { get; set; }
    public decimal TotalPurchaseValue { get; set; }
    public decimal TotalSaleValue { get; set; }
    public DateTime? LastReceiptDate { get; set; }
    public DateTime? LastIssueDate { get; set; }
    public List<MonthlyMovementDto> MonthlyMovements { get; set; } = new();
}

public class MonthlyMovementDto
{
    public int Year { get; set; }
    public int Month { get; set; }
    public int ReceivedQuantity { get; set; }
    public int IssuedQuantity { get; set; }
    public decimal ReceivedValue { get; set; }
    public decimal IssuedValue { get; set; }
}

public class ProductInventoryDto
{
    public int ProductId { get; set; }
    public string ProductName { get; set; } = null!;
    public string Sku { get; set; } = null!;
    public string? Unit { get; set; }
    public int WarehouseId { get; set; }
    public string WarehouseName { get; set; } = null!;
    public int CurrentStock { get; set; }
    public int MinStock { get; set; }
    public int MaxStock { get; set; }
    public bool IsLowStock { get; set; }
    public DateTime? LastUpdated { get; set; }
}
