using System.ComponentModel.DataAnnotations;

namespace WarehouseManage.DTOs.Warehouse;

public class WarehouseDto
{
    // ID - chỉ có khi đọc dữ liệu hoặc cập nhật
    public int? WarehouseId { get; set; }

    [Required(ErrorMessage = "Tên kho hàng không được để trống")]
    [StringLength(100, ErrorMessage = "Tên kho hàng không được vượt quá 100 ký tự")]
    public string WarehouseName { get; set; } = null!;

    [Required(ErrorMessage = "Địa chỉ không được để trống")]
    [StringLength(500, ErrorMessage = "Địa chỉ không được vượt quá 500 ký tự")]
    public string Address { get; set; } = null!;

    [StringLength(20, ErrorMessage = "Số điện thoại không được vượt quá 20 ký tự")]
    [RegularExpression(@"^(\+84|84|0)[3|5|7|8|9]([0-9]{8})$", ErrorMessage = "Số điện thoại không đúng định dạng")]
    public string? ContactPhone { get; set; }

    // Chỉ đọc - không dùng khi tạo/cập nhật
    public DateTime? CreatedAt { get; set; }
    public int TotalInventoryItems { get; set; }

    // Helper methods để xác định action
    public bool IsCreate => !WarehouseId.HasValue || WarehouseId.Value == 0;
    public bool IsUpdate => WarehouseId.HasValue && WarehouseId.Value > 0;
}
