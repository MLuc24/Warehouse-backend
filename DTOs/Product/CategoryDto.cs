using System.ComponentModel.DataAnnotations;

namespace WarehouseManage.DTOs.Product;

/// <summary>
/// DTO cho danh mục sản phẩm
/// </summary>
public class CategoryDto
{
    public int CategoryId { get; set; }
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public string? Icon { get; set; }
    public string? Color { get; set; }
    public string? StorageType { get; set; }
    public bool IsPerishable { get; set; }
    public int? DefaultMinStock { get; set; }
    public int? DefaultMaxStock { get; set; }
    public bool Status { get; set; }
    public DateTime? CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public int ProductCount { get; set; } // Số sản phẩm trong danh mục
}

/// <summary>
/// DTO để tạo danh mục mới
/// </summary>
public class CreateCategoryDto
{
    [Required(ErrorMessage = "Tên danh mục là bắt buộc")]
    [StringLength(100, ErrorMessage = "Tên danh mục không được vượt quá 100 ký tự")]
    public string Name { get; set; } = null!;

    [StringLength(500, ErrorMessage = "Mô tả không được vượt quá 500 ký tự")]
    public string? Description { get; set; }

    [StringLength(50, ErrorMessage = "Icon không được vượt quá 50 ký tự")]
    public string? Icon { get; set; }

    [StringLength(20, ErrorMessage = "Màu sắc không được vượt quá 20 ký tự")]
    public string? Color { get; set; }

    [StringLength(50, ErrorMessage = "Loại bảo quản không được vượt quá 50 ký tự")]
    public string? StorageType { get; set; }

    public bool IsPerishable { get; set; } = false;

    [Range(0, int.MaxValue, ErrorMessage = "Mức tồn kho tối thiểu phải >= 0")]
    public int? DefaultMinStock { get; set; }

    [Range(0, int.MaxValue, ErrorMessage = "Mức tồn kho tối đa phải >= 0")]
    public int? DefaultMaxStock { get; set; }

    public bool Status { get; set; } = true;
}

/// <summary>
/// DTO để cập nhật danh mục
/// </summary>
public class UpdateCategoryDto
{
    [Required(ErrorMessage = "Tên danh mục là bắt buộc")]
    [StringLength(100, ErrorMessage = "Tên danh mục không được vượt quá 100 ký tự")]
    public string Name { get; set; } = null!;

    [StringLength(500, ErrorMessage = "Mô tả không được vượt quá 500 ký tự")]
    public string? Description { get; set; }

    [StringLength(50, ErrorMessage = "Icon không được vượt quá 50 ký tự")]
    public string? Icon { get; set; }

    [StringLength(20, ErrorMessage = "Màu sắc không được vượt quá 20 ký tự")]
    public string? Color { get; set; }

    [StringLength(50, ErrorMessage = "Loại bảo quản không được vượt quá 50 ký tự")]
    public string? StorageType { get; set; }

    public bool IsPerishable { get; set; } = false;

    [Range(0, int.MaxValue, ErrorMessage = "Mức tồn kho tối thiểu phải >= 0")]
    public int? DefaultMinStock { get; set; }

    [Range(0, int.MaxValue, ErrorMessage = "Mức tồn kho tối đa phải >= 0")]
    public int? DefaultMaxStock { get; set; }

    public bool Status { get; set; } = true;
}

/// <summary>
/// DTO cho danh mục mặc định TocoToco với dữ liệu seed
/// </summary>
public class DefaultCategoryDto
{
    public string Name { get; set; } = null!;
    public string Description { get; set; } = null!;
    public string Icon { get; set; } = null!;
    public string Color { get; set; } = null!;
    public List<string> ExampleProducts { get; set; } = new();
    public string StorageType { get; set; } = null!;
    public bool IsPerishable { get; set; }
    public int DefaultMinStock { get; set; }
    public int DefaultMaxStock { get; set; }
}
