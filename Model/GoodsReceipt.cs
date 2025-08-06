using System;
using System.Collections.Generic;

namespace WarehouseManage.Model;

public partial class GoodsReceipt
{
    public int GoodsReceiptId { get; set; }

    // 🧋 THÊM MỚI - Số phiếu nhập cho dễ tracking
    public string ReceiptNumber { get; set; } = null!;   // VD: NK20250803001

    public int SupplierId { get; set; }

    public int CreatedByUserId { get; set; }

    public DateTime? ReceiptDate { get; set; }

    public decimal? TotalAmount { get; set; }

    public string? Notes { get; set; }

    public string? Status { get; set; }

    // Workflow tracking fields
    public int? ApprovedByUserId { get; set; }
    public DateTime? ApprovedDate { get; set; }
    public string? ApprovalNotes { get; set; }
    
    public bool? SupplierConfirmed { get; set; }
    public DateTime? SupplierConfirmedDate { get; set; }
    public string? SupplierConfirmationToken { get; set; } // For email confirmation link
    
    public int? CompletedByUserId { get; set; }
    public DateTime? CompletedDate { get; set; }

    public virtual User CreatedByUser { get; set; } = null!;
    public virtual User? ApprovedByUser { get; set; }
    public virtual User? CompletedByUser { get; set; }

    public virtual ICollection<GoodsReceiptDetail> GoodsReceiptDetails { get; set; } = new List<GoodsReceiptDetail>();

    public virtual Supplier Supplier { get; set; } = null!;
}
