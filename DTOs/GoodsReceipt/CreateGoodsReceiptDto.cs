using System.ComponentModel.DataAnnotations;

namespace WarehouseManage.DTOs.GoodsReceipt;

public class CreateGoodsReceiptDto
{
    [Required(ErrorMessage = "ID nhà cung cấp không được để trống")]
    public int SupplierId { get; set; }

    public string? Notes { get; set; }

    [Required(ErrorMessage = "Chi tiết phiếu nhập không được để trống")]
    public List<CreateGoodsReceiptDetailDto> Details { get; set; } = new();
}

public class CreateGoodsReceiptDetailDto
{
    [Required(ErrorMessage = "ID sản phẩm không được để trống")]
    public int ProductId { get; set; }

    [Required(ErrorMessage = "Số lượng không được để trống")]
    [Range(1, int.MaxValue, ErrorMessage = "Số lượng phải lớn hơn 0")]
    public int Quantity { get; set; }

    [Required(ErrorMessage = "Đơn giá không được để trống")]
    [Range(0.01, double.MaxValue, ErrorMessage = "Đơn giá phải lớn hơn 0")]
    public decimal UnitPrice { get; set; }
}
