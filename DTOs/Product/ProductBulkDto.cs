using System.ComponentModel.DataAnnotations;

namespace WarehouseManage.DTOs.Product;

/// <summary>
/// DTO cho thao tác hàng loạt với sản phẩm
/// </summary>
public class BulkProductOperationDto
{
    [Required(ErrorMessage = "Danh sách ID sản phẩm là bắt buộc")]
    [MinLength(1, ErrorMessage = "Phải chọn ít nhất 1 sản phẩm")]
    public List<int> ProductIds { get; set; } = new();

    [Required(ErrorMessage = "Loại thao tác là bắt buộc")]
    public BulkOperationType Operation { get; set; }

    public Dictionary<string, object> Parameters { get; set; } = new();
}

/// <summary>
/// Enum cho các loại thao tác hàng loạt
/// </summary>
public enum BulkOperationType
{
    UpdateStatus,           // Cập nhật trạng thái
    UpdateCategory,         // Cập nhật danh mục
    UpdateSupplier,         // Cập nhật nhà cung cấp
    UpdatePricing,          // Cập nhật giá
    UpdateStockLevels,      // Cập nhật mức tồn kho
    Delete,                 // Xóa
    Export,                 // Xuất dữ liệu
    GenerateBarcode        // Tạo mã vạch
}

/// <summary>
/// DTO cho kết quả thao tác hàng loạt
/// </summary>
public class BulkOperationResultDto
{
    public BulkOperationType Operation { get; set; }
    public int TotalItems { get; set; }
    public int SuccessCount { get; set; }
    public int FailureCount { get; set; }
    public List<BulkOperationError> Errors { get; set; } = new();
    public DateTime ProcessedAt { get; set; }
    public TimeSpan ProcessingTime { get; set; }
    public string? AdditionalInfo { get; set; }
}

/// <summary>
/// DTO cho lỗi trong thao tác hàng loạt
/// </summary>
public class BulkOperationError
{
    public int ProductId { get; set; }
    public string ProductName { get; set; } = null!;
    public string Sku { get; set; } = null!;
    public string ErrorMessage { get; set; } = null!;
    public string? ErrorCode { get; set; }
}

/// <summary>
/// DTO cho import sản phẩm từ file
/// </summary>
public class ProductImportDto
{
    [Required(ErrorMessage = "File import là bắt buộc")]
    public IFormFile ImportFile { get; set; } = null!;

    public bool SkipDuplicates { get; set; } = true;
    public bool UpdateExisting { get; set; } = false;
    public bool ValidateOnly { get; set; } = false;
}

/// <summary>
/// DTO cho kết quả import
/// </summary>
public class ProductImportResultDto
{
    public int TotalRows { get; set; }
    public int SuccessCount { get; set; }
    public int SkippedCount { get; set; }
    public int ErrorCount { get; set; }
    public List<ProductImportError> Errors { get; set; } = new();
    public List<ProductDto> ImportedProducts { get; set; } = new();
    public DateTime ImportedAt { get; set; }
    public string ImportedBy { get; set; } = null!;
}

/// <summary>
/// DTO cho lỗi import
/// </summary>
public class ProductImportError
{
    public int RowNumber { get; set; }
    public string? Sku { get; set; }
    public string? ProductName { get; set; }
    public string ErrorMessage { get; set; } = null!;
    public string? ErrorField { get; set; }
}

/// <summary>
/// DTO cho export sản phẩm
/// </summary>
public class ProductExportDto
{
    public List<int>? ProductIds { get; set; }
    public ProductSearchDto? SearchCriteria { get; set; }
    public ExportFormat Format { get; set; } = ExportFormat.Excel;
    public List<string> IncludeFields { get; set; } = new();
    public bool IncludeStockInfo { get; set; } = true;
    public bool IncludePricingInfo { get; set; } = true;
    public bool IncludeSupplierInfo { get; set; } = true;
}

/// <summary>
/// Enum cho định dạng export
/// </summary>
public enum ExportFormat
{
    Excel,
    Csv,
    Pdf
}

/// <summary>
/// DTO cho tạo mã vạch hàng loạt
/// </summary>
public class BulkBarcodeGenerationDto
{
    [Required(ErrorMessage = "Danh sách ID sản phẩm là bắt buộc")]
    public List<int> ProductIds { get; set; } = new();

    public BarcodeFormat Format { get; set; } = BarcodeFormat.Code128;
    public BarcodeSize Size { get; set; } = BarcodeSize.Medium;
    public bool IncludeProductName { get; set; } = true;
    public bool IncludePrice { get; set; } = false;
}

/// <summary>
/// Enum cho định dạng mã vạch
/// </summary>
public enum BarcodeFormat
{
    Code128,
    Code39,
    EAN13,
    QRCode
}

/// <summary>
/// Enum cho kích thước mã vạch
/// </summary>
public enum BarcodeSize
{
    Small,
    Medium,
    Large
}
