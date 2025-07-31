using AutoMapper;
using WarehouseManage.DTOs.Supplier;
using WarehouseManage.Interfaces;
using WarehouseManage.Model;

namespace WarehouseManage.Services;

public class SupplierService : ISupplierService
{
    private readonly ISupplierRepository _supplierRepository;
    private readonly IMapper _mapper;

    public SupplierService(ISupplierRepository supplierRepository, IMapper mapper)
    {
        _supplierRepository = supplierRepository;
        _mapper = mapper;
    }

    public async Task<SupplierDto?> GetSupplierByIdAsync(int supplierId)
    {
        var supplier = await _supplierRepository.GetByIdAsync(supplierId);
        if (supplier == null)
            return null;

        var supplierDto = _mapper.Map<SupplierDto>(supplier);
        supplierDto.TotalProducts = supplier.Products.Count;
        supplierDto.TotalReceipts = supplier.GoodsReceipts.Count;
        supplierDto.TotalPurchaseValue = supplier.GoodsReceipts.Sum(gr => gr.TotalAmount ?? 0);

        return supplierDto;
    }

    public async Task<SupplierListResponseDto> GetAllSuppliersAsync(SupplierSearchDto searchDto)
    {
        // Validate pagination parameters
        if (searchDto.Page < 1) searchDto.Page = 1;
        if (searchDto.PageSize < 1) searchDto.PageSize = 10;
        if (searchDto.PageSize > 100) searchDto.PageSize = 100; // Limit max page size

        return await _supplierRepository.GetAllAsync(searchDto);
    }

    public async Task<SupplierDto> CreateSupplierAsync(CreateSupplierDto createDto)
    {
        // Validate supplier data
        await ValidateSupplierDataAsync(createDto);

        var supplier = _mapper.Map<Supplier>(createDto);
        var createdSupplier = await _supplierRepository.CreateAsync(supplier);

        return _mapper.Map<SupplierDto>(createdSupplier);
    }

    public async Task<SupplierDto?> UpdateSupplierAsync(int supplierId, UpdateSupplierDto updateDto)
    {
        // Check if supplier exists
        if (!await _supplierRepository.ExistsAsync(supplierId))
            return null;

        // Validate supplier data
        await ValidateSupplierDataAsync(supplierId, updateDto);

        var supplier = _mapper.Map<Supplier>(updateDto);
        var updatedSupplier = await _supplierRepository.UpdateAsync(supplierId, supplier);

        return updatedSupplier != null ? _mapper.Map<SupplierDto>(updatedSupplier) : null;
    }

    public async Task<bool> DeleteSupplierAsync(int supplierId)
    {
        // Check if supplier exists
        if (!await _supplierRepository.ExistsAsync(supplierId))
            return false;

        // Change supplier status to Expired instead of deleting
        return await _supplierRepository.DeleteAsync(supplierId);
    }

    public async Task<bool> ReactivateSupplierAsync(int supplierId)
    {
        // Check if supplier exists and is expired
        var supplier = await _supplierRepository.GetByIdAsync(supplierId);
        if (supplier == null || supplier.Status != "Expired")
            return false;

        // Change supplier status back to Active
        return await _supplierRepository.ReactivateAsync(supplierId);
    }

    public async Task<bool> ValidateSupplierDataAsync(CreateSupplierDto createDto)
    {
        var errors = new List<string>();

        // Check duplicate supplier name
        if (await _supplierRepository.ExistsByNameAsync(createDto.SupplierName))
        {
            errors.Add("Tên nhà cung cấp đã tồn tại.");
        }

        // Check duplicate tax code if provided
        if (!string.IsNullOrWhiteSpace(createDto.TaxCode) && 
            await _supplierRepository.ExistsByTaxCodeAsync(createDto.TaxCode))
        {
            errors.Add("Mã số thuế đã tồn tại.");
        }

        // Check duplicate email if provided
        if (!string.IsNullOrWhiteSpace(createDto.Email) && 
            await _supplierRepository.ExistsByEmailAsync(createDto.Email))
        {
            errors.Add("Email đã tồn tại.");
        }

        if (errors.Any())
        {
            throw new InvalidOperationException(string.Join(" ", errors));
        }

        return true;
    }

    public async Task<bool> ValidateSupplierDataAsync(int supplierId, UpdateSupplierDto updateDto)
    {
        var errors = new List<string>();

        // Check duplicate supplier name (excluding current supplier)
        if (await _supplierRepository.ExistsByNameAsync(updateDto.SupplierName, supplierId))
        {
            errors.Add("Tên nhà cung cấp đã tồn tại.");
        }

        // Check duplicate tax code if provided (excluding current supplier)
        if (!string.IsNullOrWhiteSpace(updateDto.TaxCode) && 
            await _supplierRepository.ExistsByTaxCodeAsync(updateDto.TaxCode, supplierId))
        {
            errors.Add("Mã số thuế đã tồn tại.");
        }

        // Check duplicate email if provided (excluding current supplier)
        if (!string.IsNullOrWhiteSpace(updateDto.Email) && 
            await _supplierRepository.ExistsByEmailAsync(updateDto.Email, supplierId))
        {
            errors.Add("Email đã tồn tại.");
        }

        if (errors.Any())
        {
            throw new InvalidOperationException(string.Join(" ", errors));
        }

        return true;
    }

    public async Task<bool> CanDeleteSupplierAsync(int supplierId)
    {
        // Cannot delete if supplier has active products
        if (await _supplierRepository.HasActiveProductsAsync(supplierId))
            return false;

        // Cannot delete if supplier has goods receipts (historical data)
        if (await _supplierRepository.HasGoodsReceiptsAsync(supplierId))
            return false;

        return true;
    }

    public async Task<SupplierStatsDto?> GetSupplierStatisticsAsync(int supplierId)
    {
        return await _supplierRepository.GetSupplierStatsAsync(supplierId);
    }

    public async Task<List<SupplierDto>> GetTopSuppliersAsync(int count = 5)
    {
        if (count <= 0) count = 5;
        if (count > 20) count = 20; // Limit max count

        return await _supplierRepository.GetTopSuppliersAsync(count);
    }

    public async Task<bool> SupplierExistsAsync(int supplierId)
    {
        return await _supplierRepository.ExistsAsync(supplierId);
    }

    public async Task<List<SupplierDto>> GetActiveSuppliersAsync()
    {
        return await _supplierRepository.GetActiveSuppliersAsync();
    }
}
