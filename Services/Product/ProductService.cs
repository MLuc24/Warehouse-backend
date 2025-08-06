using AutoMapper;
using WarehouseManage.DTOs.Product;
using WarehouseManage.Interfaces;
using WarehouseManage.Model;

namespace WarehouseManage.Services;

public class ProductService : IProductService
{
    private readonly IProductRepository _productRepository;
    private readonly ISupplierRepository _supplierRepository;
    private readonly IMapper _mapper;

    public ProductService(
        IProductRepository productRepository, 
        ISupplierRepository supplierRepository,
        IMapper mapper)
    {
        _productRepository = productRepository;
        _supplierRepository = supplierRepository;
        _mapper = mapper;
    }

    public async Task<ProductDto?> GetProductByIdAsync(int productId)
    {
        var product = await _productRepository.GetByIdAsync(productId);
        if (product == null)
            return null;

        var productDto = _mapper.Map<ProductDto>(product);
        
        // Add calculated fields
        productDto.SupplierName = product.Supplier?.SupplierName;
        productDto.CategoryName = product.Category?.Name; // Add missing CategoryName mapping
        productDto.CurrentStock = product.Inventories.Sum(i => i.Quantity);
        productDto.TotalReceived = product.GoodsReceiptDetails.Sum(grd => grd.Quantity);
        productDto.TotalIssued = product.GoodsIssueDetails.Sum(gid => gid.Quantity);
        productDto.TotalValue = (product.PurchasePrice ?? 0) * productDto.CurrentStock;

        return productDto;
    }

    public async Task<ProductListResponseDto> GetAllProductsAsync(ProductSearchDto searchDto)
    {
        // Validate pagination parameters
        if (searchDto.Page < 1) searchDto.Page = 1;
        if (searchDto.PageSize < 1) searchDto.PageSize = 10;
        if (searchDto.PageSize > 100) searchDto.PageSize = 100; // Limit max page size

        return await _productRepository.GetAllAsync(searchDto);
    }

    public async Task<ProductDto> CreateProductAsync(CreateProductDto createDto)
    {
        // Validate product data
        await ValidateProductDataAsync(createDto);

        var product = _mapper.Map<Product>(createDto);
        var createdProduct = await _productRepository.CreateAsync(product);

        return _mapper.Map<ProductDto>(createdProduct);
    }

    public async Task<ProductDto?> UpdateProductAsync(int productId, UpdateProductDto updateDto)
    {
        // Check if product exists
        if (!await _productRepository.ExistsAsync(productId))
            return null;

        // Validate product data
        await ValidateProductDataAsync(updateDto, productId);

        var product = _mapper.Map<Product>(updateDto);
        var updatedProduct = await _productRepository.UpdateAsync(productId, product);

        if (updatedProduct == null)
            return null;

        return _mapper.Map<ProductDto>(updatedProduct);
    }

    public async Task<bool> DeleteProductAsync(int productId)
    {
        if (!await _productRepository.ExistsAsync(productId))
            return false;

        return await _productRepository.DeleteAsync(productId);
    }

    public async Task<bool> ReactivateProductAsync(int productId)
    {
        // Check if product exists and is inactive
        var product = await _productRepository.GetByIdAsync(productId);
        if (product == null || product.Status == true)
            return false;

        // Reactivate the product
        return await _productRepository.ReactivateAsync(productId);
    }

    public async Task<ProductDto?> GetProductBySkuAsync(string sku)
    {
        if (string.IsNullOrWhiteSpace(sku))
            return null;

        var product = await _productRepository.GetBySkuAsync(sku);
        if (product == null)
            return null;

        return _mapper.Map<ProductDto>(product);
    }

    public async Task<ProductStatsDto?> GetProductStatsAsync(int productId)
    {
        return await _productRepository.GetProductStatsAsync(productId);
    }

    public async Task<List<ProductDto>> GetTopProductsAsync(int count = 5)
    {
        if (count <= 0) count = 5;
        if (count > 50) count = 50; // Limit max count

        return await _productRepository.GetTopProductsAsync(count);
    }

    public async Task<List<ProductInventoryDto>> GetLowStockProductsAsync()
    {
        return await _productRepository.GetLowStockProductsAsync();
    }

    public async Task<List<ProductDto>> GetProductsBySupplierAsync(int supplierId)
    {
        // Validate supplier exists
        if (!await _supplierRepository.ExistsAsync(supplierId))
            throw new ArgumentException("Nhà cung cấp không tồn tại", nameof(supplierId));

        return await _productRepository.GetProductsBySupplierId(supplierId);
    }

    public async Task<List<ProductDto>> GetActiveProductsAsync()
    {
        return await _productRepository.GetActiveProductsAsync();
    }

    public async Task<bool> HasInventoryMovementsAsync(int productId)
    {
        return await _productRepository.HasInventoryMovementsAsync(productId);
    }

    #region Private Methods

    private async Task ValidateProductDataAsync(CreateProductDto createDto)
    {
        // Check if SKU already exists
        if (await _productRepository.ExistsBySkuAsync(createDto.Sku))
        {
            throw new InvalidOperationException($"Mã SKU '{createDto.Sku}' đã tồn tại");
        }

        // Validate supplier if provided
        if (createDto.SupplierId.HasValue)
        {
            if (!await _supplierRepository.ExistsAsync(createDto.SupplierId.Value))
            {
                throw new ArgumentException("Nhà cung cấp không tồn tại", nameof(createDto.SupplierId));
            }
        }

        // Validate price logic
        if (createDto.PurchasePrice.HasValue && createDto.SellingPrice.HasValue)
        {
            if (createDto.SellingPrice < createDto.PurchasePrice)
            {
                throw new InvalidOperationException("Giá bán không thể thấp hơn giá mua");
            }
        }
    }

    private async Task ValidateProductDataAsync(UpdateProductDto updateDto, int productId)
    {
        // Check if SKU already exists (excluding current product)
        if (await _productRepository.ExistsBySkuAsync(updateDto.Sku, productId))
        {
            throw new InvalidOperationException($"Mã SKU '{updateDto.Sku}' đã tồn tại");
        }

        // Validate supplier if provided
        if (updateDto.SupplierId.HasValue)
        {
            if (!await _supplierRepository.ExistsAsync(updateDto.SupplierId.Value))
            {
                throw new ArgumentException("Nhà cung cấp không tồn tại", nameof(updateDto.SupplierId));
            }
        }

        // Validate price logic
        if (updateDto.PurchasePrice.HasValue && updateDto.SellingPrice.HasValue)
        {
            if (updateDto.SellingPrice < updateDto.PurchasePrice)
            {
                throw new InvalidOperationException("Giá bán không thể thấp hơn giá mua");
            }
        }
    }

    #endregion
}
