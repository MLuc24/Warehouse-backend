using System.ComponentModel.DataAnnotations;

namespace WarehouseManage.DTOs.GoodsIssue;

public class CreateGoodsIssueDto
{
    public int? CustomerId { get; set; }
    public DateTime? RequestedDeliveryDate { get; set; }
    public string? Notes { get; set; }

    [Required(ErrorMessage = "Chi tiết phiếu xuất không được để trống")]
    public List<CreateGoodsIssueDetailDto> Details { get; set; } = new();
}

public class CreateGoodsIssueDetailDto
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

public class UpdateGoodsIssueDto
{
    public int GoodsIssueId { get; set; }
    public int? CustomerId { get; set; }
    public DateTime? RequestedDeliveryDate { get; set; }
    public string? Notes { get; set; }
    public List<CreateGoodsIssueDetailDto> Details { get; set; } = new();
}

public class GoodsIssueFilterDto
{
    public string? IssueNumber { get; set; }
    public string? Status { get; set; }
    public int? CustomerId { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}
