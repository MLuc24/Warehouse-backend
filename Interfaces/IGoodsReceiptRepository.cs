using WarehouseManage.DTOs.GoodsReceipt;
using WarehouseManage.Model;

namespace WarehouseManage.Interfaces;

public interface IGoodsReceiptRepository
{
    Task<PagedResult<GoodsReceipt>> GetGoodsReceiptsAsync(GoodsReceiptFilterDto filter);
    Task<GoodsReceipt?> GetGoodsReceiptByIdAsync(int id);
    Task<GoodsReceipt?> GetGoodsReceiptByNumberAsync(string receiptNumber);
    Task<GoodsReceipt> CreateGoodsReceiptAsync(GoodsReceipt receipt);
    Task<GoodsReceipt> UpdateGoodsReceiptAsync(GoodsReceipt receipt);
    Task<bool> DeleteGoodsReceiptAsync(int id);
    Task<bool> ExistsAsync(int id);
    Task<string> GenerateReceiptNumberAsync();
    Task<List<GoodsReceipt>> GetGoodsReceiptsBySupplierAsync(int supplierId);
    Task<List<GoodsReceipt>> GetGoodsReceiptsByDateRangeAsync(DateTime fromDate, DateTime toDate);
}
