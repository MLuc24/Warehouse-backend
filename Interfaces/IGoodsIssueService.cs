using WarehouseManage.DTOs.GoodsIssue;
using WarehouseManage.DTOs.GoodsReceipt;

namespace WarehouseManage.Interfaces;

public interface IGoodsIssueService
{
    Task<PagedResult<GoodsIssueDto>> GetGoodsIssuesAsync(GoodsIssueFilterDto filter);
    Task<GoodsIssueDto?> GetGoodsIssueByIdAsync(int id);
    Task<GoodsIssueDto?> GetGoodsIssueByNumberAsync(string issueNumber);
    Task<GoodsIssueDto> CreateGoodsIssueAsync(CreateGoodsIssueDto dto, int createdByUserId);
    Task<GoodsIssueDto> UpdateGoodsIssueAsync(UpdateGoodsIssueDto dto);
    Task<bool> DeleteGoodsIssueAsync(int id);
    Task<List<GoodsIssueDto>> GetGoodsIssuesByCustomerAsync(int customerId);
    Task<bool> ValidateGoodsIssueAsync(CreateGoodsIssueDto dto);
    Task<bool> CanDeleteGoodsIssueAsync(int id);
}
