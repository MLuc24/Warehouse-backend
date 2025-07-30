using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WarehouseManage.DTOs.Product;
using WarehouseManage.Interfaces;

namespace WarehouseManage.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize] // Require authentication
public class ProductController : ControllerBase
{
    private readonly IProductService _productService;
    private readonly ILogger<ProductController> _logger;

    public ProductController(IProductService productService, ILogger<ProductController> logger)
    {
        _productService = productService;
        _logger = logger;
    }

    /// <summary>
    /// Lấy danh sách tất cả sản phẩm (có phân trang và tìm kiếm)
    /// </summary>
    [HttpGet]
    [Authorize(Roles = "Admin")] // Only Admin can access
    public async Task<ActionResult<ProductListResponseDto>> GetAllProducts([FromQuery] ProductSearchDto searchDto)
    {
        try
        {
            var result = await _productService.GetAllProductsAsync(searchDto);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting products");
            return StatusCode(500, new { message = "Đã xảy ra lỗi khi lấy danh sách sản phẩm" });
        }
    }

    /// <summary>
    /// Lấy thông tin sản phẩm theo ID
    /// </summary>
    [HttpGet("{id}")]
    [Authorize(Roles = "Admin")] // Only Admin can access
    public async Task<ActionResult<ProductDto>> GetProductById(int id)
    {
        try
        {
            var product = await _productService.GetProductByIdAsync(id);
            if (product == null)
            {
                return NotFound(new { message = "Sản phẩm không tồn tại" });
            }

            return Ok(product);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting product {ProductId}", id);
            return StatusCode(500, new { message = "Đã xảy ra lỗi khi lấy thông tin sản phẩm" });
        }
    }

    /// <summary>
    /// Lấy thông tin sản phẩm theo SKU
    /// </summary>
    [HttpGet("sku/{sku}")]
    [Authorize(Roles = "Admin")] // Only Admin can access
    public async Task<ActionResult<ProductDto>> GetProductBySku(string sku)
    {
        try
        {
            var product = await _productService.GetProductBySkuAsync(sku);
            if (product == null)
            {
                return NotFound(new { message = "Sản phẩm không tồn tại" });
            }

            return Ok(product);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting product by SKU {SKU}", sku);
            return StatusCode(500, new { message = "Đã xảy ra lỗi khi lấy thông tin sản phẩm" });
        }
    }

    /// <summary>
    /// Tạo sản phẩm mới
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Admin")] // Only Admin can create
    public async Task<ActionResult<ProductDto>> CreateProduct([FromBody] CreateProductDto createDto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var product = await _productService.CreateProductAsync(createDto);
            return CreatedAtAction(nameof(GetProductById), new { id = product.ProductId }, product);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while creating product");
            return StatusCode(500, new { message = "Đã xảy ra lỗi khi tạo sản phẩm" });
        }
    }

    /// <summary>
    /// Cập nhật thông tin sản phẩm
    /// </summary>
    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")] // Only Admin can update
    public async Task<ActionResult<ProductDto>> UpdateProduct(int id, [FromBody] UpdateProductDto updateDto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var product = await _productService.UpdateProductAsync(id, updateDto);
            if (product == null)
            {
                return NotFound(new { message = "Sản phẩm không tồn tại" });
            }

            return Ok(product);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while updating product {ProductId}", id);
            return StatusCode(500, new { message = "Đã xảy ra lỗi khi cập nhật sản phẩm" });
        }
    }

    /// <summary>
    /// Xóa sản phẩm
    /// </summary>
    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")] // Only Admin can delete
    public async Task<ActionResult> DeleteProduct(int id)
    {
        try
        {
            var result = await _productService.DeleteProductAsync(id);
            if (!result)
            {
                return NotFound(new { message = "Sản phẩm không tồn tại" });
            }

            return Ok(new { message = "Đã xóa sản phẩm thành công" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while deleting product {ProductId}", id);
            return StatusCode(500, new { message = "Đã xảy ra lỗi khi xóa sản phẩm" });
        }
    }

    /// <summary>
    /// Lấy thống kê chi tiết của sản phẩm
    /// </summary>
    [HttpGet("{id}/stats")]
    [Authorize(Roles = "Admin")] // Only Admin can access
    public async Task<ActionResult<ProductStatsDto>> GetProductStats(int id)
    {
        try
        {
            var stats = await _productService.GetProductStatsAsync(id);
            if (stats == null)
            {
                return NotFound(new { message = "Sản phẩm không tồn tại" });
            }

            return Ok(stats);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting product stats {ProductId}", id);
            return StatusCode(500, new { message = "Đã xảy ra lỗi khi lấy thống kê sản phẩm" });
        }
    }

    /// <summary>
    /// Lấy danh sách top sản phẩm bán chạy
    /// </summary>
    [HttpGet("top-products")]
    [Authorize(Roles = "Admin")] // Only Admin can access
    public async Task<ActionResult<List<ProductDto>>> GetTopProducts([FromQuery] int count = 5)
    {
        try
        {
            var products = await _productService.GetTopProductsAsync(count);
            return Ok(products);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting top products");
            return StatusCode(500, new { message = "Đã xảy ra lỗi khi lấy danh sách sản phẩm bán chạy" });
        }
    }

    /// <summary>
    /// Lấy danh sách sản phẩm sắp hết hàng
    /// </summary>
    [HttpGet("low-stock")]
    [Authorize(Roles = "Admin")] // Only Admin can access
    public async Task<ActionResult<List<ProductInventoryDto>>> GetLowStockProducts()
    {
        try
        {
            var products = await _productService.GetLowStockProductsAsync();
            return Ok(products);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting low stock products");
            return StatusCode(500, new { message = "Đã xảy ra lỗi khi lấy danh sách sản phẩm sắp hết hàng" });
        }
    }

    /// <summary>
    /// Lấy danh sách sản phẩm theo nhà cung cấp
    /// </summary>
    [HttpGet("by-supplier/{supplierId}")]
    [Authorize(Roles = "Admin")] // Only Admin can access
    public async Task<ActionResult<List<ProductDto>>> GetProductsBySupplier(int supplierId)
    {
        try
        {
            var products = await _productService.GetProductsBySupplierAsync(supplierId);
            return Ok(products);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting products by supplier {SupplierId}", supplierId);
            return StatusCode(500, new { message = "Đã xảy ra lỗi khi lấy danh sách sản phẩm theo nhà cung cấp" });
        }
    }
}
