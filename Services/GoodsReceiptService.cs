using Microsoft.EntityFrameworkCore;
using WarehouseManage.Constants;
using WarehouseManage.Data;
using WarehouseManage.DTOs.GoodsReceipt;
using WarehouseManage.Interfaces;
using WarehouseManage.Model;

namespace WarehouseManage.Services;

public class GoodsReceiptService : IGoodsReceiptService
{
    private readonly IGoodsReceiptRepository _goodsReceiptRepository;
    private readonly ISupplierRepository _supplierRepository;
    private readonly IProductRepository _productRepository;
    private readonly IGoodsReceiptWorkflowService _workflowService;
    private readonly WarehouseDbContext _context;

    public GoodsReceiptService(
        IGoodsReceiptRepository goodsReceiptRepository,
        ISupplierRepository supplierRepository,
        IProductRepository productRepository,
        IGoodsReceiptWorkflowService workflowService,
        WarehouseDbContext context)
    {
        _goodsReceiptRepository = goodsReceiptRepository;
        _supplierRepository = supplierRepository;
        _productRepository = productRepository;
        _workflowService = workflowService;
        _context = context;
    }

    public async Task<PagedResult<GoodsReceiptDto>> GetGoodsReceiptsAsync(GoodsReceiptFilterDto filter)
    {
        var result = await _goodsReceiptRepository.GetGoodsReceiptsAsync(filter);
        
        var dtos = result.Items.Select(MapToDto).ToList();
        
        return new PagedResult<GoodsReceiptDto>(dtos, result.TotalCount, result.PageNumber, result.PageSize);
    }

    public async Task<GoodsReceiptDto?> GetGoodsReceiptByIdAsync(int id)
    {
        var receipt = await _goodsReceiptRepository.GetGoodsReceiptByIdAsync(id);
        return receipt != null ? MapToDto(receipt) : null;
    }

    public async Task<GoodsReceiptDto?> GetGoodsReceiptByNumberAsync(string receiptNumber)
    {
        var receipt = await _goodsReceiptRepository.GetGoodsReceiptByNumberAsync(receiptNumber);
        return receipt != null ? MapToDto(receipt) : null;
    }

    public async Task<GoodsReceiptDto> CreateGoodsReceiptAsync(CreateGoodsReceiptDto dto, int createdByUserId)
    {
        // Validate input
        if (!await ValidateGoodsReceiptAsync(dto))
        {
            throw new ArgumentException("Dữ liệu phiếu nhập không hợp lệ");
        }

        // Check if supplier exists
        if (!await _supplierRepository.ExistsAsync(dto.SupplierId))
        {
            throw new ArgumentException(GoodsReceiptConstants.ErrorMessages.SupplierNotFound);
        }

        // Get current user to determine initial status
        var currentUser = await _context.Users.FindAsync(createdByUserId);
        if (currentUser == null)
        {
            throw new ArgumentException("User not found");
        }

        // Determine initial status based on user role
        var initialStatus = _workflowService.GetInitialStatusByRole(currentUser.Role!);

        // Generate receipt number
        var receiptNumber = await _goodsReceiptRepository.GenerateReceiptNumberAsync();

        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            // Create goods receipt
            var receipt = new GoodsReceipt
            {
                ReceiptNumber = receiptNumber,
                SupplierId = dto.SupplierId,
                CreatedByUserId = createdByUserId,
                ReceiptDate = DateTime.Now,
                Notes = dto.Notes,
                Status = initialStatus, // Use workflow-determined status
                TotalAmount = 0
            };

            // Create details and calculate total
            decimal totalAmount = 0;
            var details = new List<GoodsReceiptDetail>();

            foreach (var detailDto in dto.Details)
            {
                // Validate product exists
                if (!await _productRepository.ExistsAsync(detailDto.ProductId))
                {
                    throw new ArgumentException($"Sản phẩm ID {detailDto.ProductId} không tồn tại");
                }

                var detail = new GoodsReceiptDetail
                {
                    ProductId = detailDto.ProductId,
                    Quantity = detailDto.Quantity,
                    UnitPrice = detailDto.UnitPrice
                };

                details.Add(detail);
                totalAmount += detailDto.Quantity * detailDto.UnitPrice;
            }

            receipt.TotalAmount = totalAmount;
            receipt.GoodsReceiptDetails = details;

            // Save receipt
            var createdReceipt = await _goodsReceiptRepository.CreateGoodsReceiptAsync(receipt);

            // Send supplier email if status is Pending (Admin/Manager created)
            if (initialStatus == GoodsReceiptConstants.Status.Pending)
            {
                await _workflowService.SendSupplierConfirmationEmailAsync(createdReceipt.GoodsReceiptId);
            }

            await transaction.CommitAsync();

            return MapToDto(createdReceipt);
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task<GoodsReceiptDto> UpdateGoodsReceiptAsync(UpdateGoodsReceiptDto dto)
    {
        var existingReceipt = await _goodsReceiptRepository.GetGoodsReceiptByIdAsync(dto.GoodsReceiptId);
        if (existingReceipt == null)
        {
            throw new ArgumentException(GoodsReceiptConstants.ErrorMessages.GoodsReceiptNotFound);
        }

        // Check if receipt can be updated (only draft or pending receipts)
        if (existingReceipt.Status == GoodsReceiptConstants.Status.Completed)
        {
            throw new InvalidOperationException("Không thể cập nhật phiếu nhập đã hoàn thành");
        }

        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            // Update basic info
            if (dto.SupplierId.HasValue)
            {
                if (!await _supplierRepository.ExistsAsync(dto.SupplierId.Value))
                {
                    throw new ArgumentException(GoodsReceiptConstants.ErrorMessages.SupplierNotFound);
                }
                existingReceipt.SupplierId = dto.SupplierId.Value;
            }

            if (!string.IsNullOrWhiteSpace(dto.Notes))
            {
                existingReceipt.Notes = dto.Notes;
            }

            if (!string.IsNullOrWhiteSpace(dto.Status))
            {
                existingReceipt.Status = dto.Status;
            }

            // Update details if provided
            if (dto.Details?.Any() == true)
            {
                // Remove existing details
                _context.GoodsReceiptDetails.RemoveRange(existingReceipt.GoodsReceiptDetails);

                // Add new details
                decimal totalAmount = 0;
                foreach (var detailDto in dto.Details)
                {
                    if (!await _productRepository.ExistsAsync(detailDto.ProductId))
                    {
                        throw new ArgumentException($"Sản phẩm ID {detailDto.ProductId} không tồn tại");
                    }

                    var detail = new GoodsReceiptDetail
                    {
                        GoodsReceiptId = existingReceipt.GoodsReceiptId,
                        ProductId = detailDto.ProductId,
                        Quantity = detailDto.Quantity,
                        UnitPrice = detailDto.UnitPrice
                    };

                    existingReceipt.GoodsReceiptDetails.Add(detail);
                    totalAmount += detailDto.Quantity * detailDto.UnitPrice;
                }

                existingReceipt.TotalAmount = totalAmount;
            }

            var updatedReceipt = await _goodsReceiptRepository.UpdateGoodsReceiptAsync(existingReceipt);

            // Update inventory if status changed to completed
            if (dto.Status == GoodsReceiptConstants.Status.Completed)
            {
                await UpdateInventoryAfterReceiptAsync(updatedReceipt);
            }

            await transaction.CommitAsync();

            return MapToDto(updatedReceipt);
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task<bool> DeleteGoodsReceiptAsync(int id)
    {
        if (!await CanDeleteGoodsReceiptAsync(id))
        {
            throw new InvalidOperationException(GoodsReceiptConstants.ErrorMessages.CannotDeleteCompleted);
        }

        return await _goodsReceiptRepository.DeleteGoodsReceiptAsync(id);
    }

    public async Task<List<GoodsReceiptDto>> GetGoodsReceiptsBySupplierAsync(int supplierId)
    {
        var receipts = await _goodsReceiptRepository.GetGoodsReceiptsBySupplierAsync(supplierId);
        return receipts.Select(MapToDto).ToList();
    }

    public Task<bool> ValidateGoodsReceiptAsync(CreateGoodsReceiptDto dto)
    {
        if (dto.Details == null || !dto.Details.Any())
        {
            return Task.FromResult(false);
        }

        foreach (var detail in dto.Details)
        {
            if (detail.Quantity < GoodsReceiptConstants.Validation.MinQuantity ||
                detail.Quantity > GoodsReceiptConstants.Validation.MaxQuantity)
            {
                return Task.FromResult(false);
            }

            if (detail.UnitPrice < GoodsReceiptConstants.Validation.MinUnitPrice ||
                detail.UnitPrice > GoodsReceiptConstants.Validation.MaxUnitPrice)
            {
                return Task.FromResult(false);
            }
        }

        return Task.FromResult(true);
    }

    public async Task<bool> CanDeleteGoodsReceiptAsync(int id)
    {
        var receipt = await _goodsReceiptRepository.GetGoodsReceiptByIdAsync(id);
        if (receipt == null) return false;

        // Only allow deletion of draft or cancelled receipts
        return receipt.Status == GoodsReceiptConstants.Status.Draft ||
               receipt.Status == GoodsReceiptConstants.Status.Cancelled;
    }

    private async Task UpdateInventoryAfterReceiptAsync(GoodsReceipt receipt)
    {
        foreach (var detail in receipt.GoodsReceiptDetails)
        {
            var inventory = await _context.Inventories
                .FirstOrDefaultAsync(i => i.ProductId == detail.ProductId);

            if (inventory != null)
            {
                // Update existing inventory
                inventory.Quantity += detail.Quantity;
                inventory.LastUpdatedAt = DateTime.Now;
            }
            else
            {
                // Create new inventory record
                inventory = new Inventory
                {
                    ProductId = detail.ProductId,
                    Quantity = detail.Quantity,
                    Status = "Good",
                    LastUpdatedAt = DateTime.Now
                };
                _context.Inventories.Add(inventory);
            }
        }

        await _context.SaveChangesAsync();
    }

    private static GoodsReceiptDto MapToDto(GoodsReceipt receipt)
    {
        return new GoodsReceiptDto
        {
            GoodsReceiptId = receipt.GoodsReceiptId,
            ReceiptNumber = receipt.ReceiptNumber,
            SupplierId = receipt.SupplierId,
            SupplierName = receipt.Supplier?.SupplierName ?? "",
            CreatedByUserId = receipt.CreatedByUserId,
            CreatedByUserName = receipt.CreatedByUser?.FullName ?? "",
            ReceiptDate = receipt.ReceiptDate,
            TotalAmount = receipt.TotalAmount,
            Notes = receipt.Notes,
            Status = receipt.Status,
            Details = receipt.GoodsReceiptDetails?.Select(d => new GoodsReceiptDetailDto
            {
                ProductId = d.ProductId,
                ProductName = d.Product?.ProductName ?? "",
                ProductSku = d.Product?.Sku,
                Quantity = d.Quantity,
                UnitPrice = d.UnitPrice,
                Subtotal = d.Subtotal
            }).ToList() ?? new List<GoodsReceiptDetailDto>()
        };
    }
}
