using System;
using System.Collections.Generic;

namespace WarehouseManage.Model;

public partial class Inventory
{
    public int ProductId { get; set; }

    public int Quantity { get; set; }

    // 🧋 THÊM MỚI cho tracking theo lô hàng
    public string? BatchNumber { get; set; }              // Số lô (cho nguyên liệu)
    public DateTime? ExpiryDate { get; set; }             // Hạn sử dụng cụ thể
    public string? StoredLocation { get; set; }           // Vị trí lưu trữ
    public string Status { get; set; } = "Good";          // "Good", "Near Expiry", "Expired"

    public DateTime? LastUpdatedAt { get; set; }

    public virtual Product Product { get; set; } = null!;
}
