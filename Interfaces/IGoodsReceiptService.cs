using WarehouseManage.DTOs.GoodsReceipt;

namespace WarehouseManage.Interfaces;

public interface IGoodsReceiptService
{
    Task<PagedResult<GoodsReceiptDto>> GetGoodsReceiptsAsync(GoodsReceiptFilterDto filter);
    Task<GoodsReceiptDto?> GetGoodsReceiptByIdAsync(int id);
    Task<GoodsReceiptDto?> GetGoodsReceiptByNumberAsync(string receiptNumber);
    Task<GoodsReceiptDto> CreateGoodsReceiptAsync(CreateGoodsReceiptDto dto, int createdByUserId);
    Task<GoodsReceiptDto> UpdateGoodsReceiptAsync(UpdateGoodsReceiptDto dto);
    Task<bool> DeleteGoodsReceiptAsync(int id);
    Task<List<GoodsReceiptDto>> GetGoodsReceiptsBySupplierAsync(int supplierId);
    Task<bool> ValidateGoodsReceiptAsync(CreateGoodsReceiptDto dto);
    Task<bool> CanDeleteGoodsReceiptAsync(int id);
}
