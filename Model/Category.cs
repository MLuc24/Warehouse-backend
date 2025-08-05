using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace WarehouseManage.Model;

/// <summary>
/// Model cho danh mục sản phẩm TocoToco
/// </summary>
public partial class Category
{
    public int CategoryId { get; set; }

    [Required]
    [StringLength(100)]
    public string Name { get; set; } = null!;

    [StringLength(500)]
    public string? Description { get; set; }

    [StringLength(50)]
    public string? Icon { get; set; }

    [StringLength(20)]
    public string? Color { get; set; }

    /// <summary>
    /// Loại bảo quản: "Khô", "Lạnh", "Đông lạnh"
    /// </summary>
    [StringLength(50)]
    public string? StorageType { get; set; }

    /// <summary>
    /// Có phải hàng dễ hỏng không
    /// </summary>
    public bool IsPerishable { get; set; } = false;

    /// <summary>
    /// Mức tồn kho tối thiểu mặc định
    /// </summary>
    public int? DefaultMinStock { get; set; }

    /// <summary>
    /// Mức tồn kho tối đa mặc định
    /// </summary>
    public int? DefaultMaxStock { get; set; }

    /// <summary>
    /// Trạng thái hoạt động
    /// </summary>
    public bool Status { get; set; } = true;

    public DateTime? CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    /// <summary>
    /// Danh sách sản phẩm thuộc danh mục này
    /// </summary>
    public virtual ICollection<Product> Products { get; set; } = new List<Product>();
}
