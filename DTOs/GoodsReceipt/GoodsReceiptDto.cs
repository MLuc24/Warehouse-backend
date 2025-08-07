using System.ComponentModel.DataAnnotations;

namespace WarehouseManage.DTOs.GoodsReceipt;

public class GoodsReceiptDto
{
    public int GoodsReceiptId { get; set; }
    public string ReceiptNumber { get; set; } = null!;
    public int SupplierId { get; set; }
    public string SupplierName { get; set; } = null!;
    public int CreatedByUserId { get; set; }
    public string CreatedByUserName { get; set; } = null!;
    public DateTime? ReceiptDate { get; set; }
    public decimal? TotalAmount { get; set; }
    public string? Notes { get; set; }
    public string? Status { get; set; }
    public List<GoodsReceiptDetailDto> Details { get; set; } = new();
}

public class GoodsReceiptDetailDto
{
    public int ProductId { get; set; }
    public string ProductName { get; set; } = null!;
    public string? ProductSku { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal? Subtotal { get; set; }
    public string? Unit { get; set; }
    public string? ImageUrl { get; set; }
}
