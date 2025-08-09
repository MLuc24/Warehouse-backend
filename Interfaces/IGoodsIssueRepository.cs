using WarehouseManage.DTOs.GoodsIssue;
using WarehouseManage.DTOs.GoodsReceipt;
using WarehouseManage.Model;

namespace WarehouseManage.Interfaces;

public interface IGoodsIssueRepository
{
    Task<PagedResult<GoodsIssue>> GetGoodsIssuesAsync(GoodsIssueFilterDto filter);
    Task<GoodsIssue?> GetGoodsIssueByIdAsync(int id);
    Task<GoodsIssue?> GetGoodsIssueByNumberAsync(string issueNumber);
    Task<GoodsIssue> CreateGoodsIssueAsync(GoodsIssue goodsIssue);
    Task<GoodsIssue> UpdateGoodsIssueAsync(GoodsIssue goodsIssue);
    Task<bool> DeleteGoodsIssueAsync(int id);
    Task<List<GoodsIssue>> GetGoodsIssuesByCustomerAsync(int customerId);
    Task<bool> ExistsAsync(int id);
    Task<bool> IssueNumberExistsAsync(string issueNumber);
    Task<string> GenerateNextIssueNumberAsync();
}
