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
    [Authorize(Roles = "Admin,Manager,Employee")] // All roles can access
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
    [Authorize(Roles = "Admin,Manager,Employee")] // All roles can access
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
    [Authorize(Roles = "Admin,Manager,Employee")] // All roles can access
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
    [Authorize(Roles = "Admin,Manager,Employee")] // All roles can create
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
    [Authorize(Roles = "Admin,Manager,Employee")] // All roles can update
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
    /// Xóa sản phẩm (chuyển trạng thái thành không hoạt động hoặc xóa vĩnh viễn)
    /// </summary>
    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin,Manager,Employee")] // All roles can delete
    public async Task<ActionResult> DeleteProduct(int id)
    {
        try
        {
            var result = await _productService.DeleteProductAsync(id);
            if (!result)
            {
                return NotFound(new { message = "Sản phẩm không tồn tại" });
            }

            return Ok(new { message = "Sản phẩm đã được ngừng kinh doanh thành công" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while deleting product {ProductId}", id);
            return StatusCode(500, new { message = "Đã xảy ra lỗi khi ngừng kinh doanh sản phẩm" });
        }
    }

    /// <summary>
    /// Khôi phục sản phẩm (chuyển từ trạng thái không hoạt động về hoạt động)
    /// </summary>
    [HttpPatch("{id}/reactivate")]
    [Authorize(Roles = "Admin,Manager,Employee")] // All roles can reactivate
    public async Task<ActionResult> ReactivateProduct(int id)
    {
        try
        {
            var result = await _productService.ReactivateProductAsync(id);
            if (!result)
            {
                return NotFound(new { message = "Không tìm thấy sản phẩm hoặc sản phẩm đã hoạt động" });
            }

            return Ok(new { message = "Sản phẩm đã được khôi phục và chuyển về trạng thái hoạt động" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while reactivating product {ProductId}", id);
            return StatusCode(500, new { message = "Đã xảy ra lỗi khi khôi phục sản phẩm" });
        }
    }

    /// <summary>
    /// Lấy thống kê chi tiết của sản phẩm
    /// </summary>
    [HttpGet("{id}/stats")]
    [Authorize(Roles = "Admin,Manager,Employee")] // All roles can access
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
    [Authorize(Roles = "Admin,Manager,Employee")] // All roles can access
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
    [Authorize(Roles = "Admin,Manager,Employee")] // All roles can access
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
    [Authorize(Roles = "Admin,Manager,Employee")] // All roles can access
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

    /// <summary>
    /// Kiểm tra trạng thái sản phẩm
    /// </summary>
    [HttpGet("{id}/can-delete")]
    [Authorize(Roles = "Admin,Manager,Employee")] // All roles can access
    public async Task<ActionResult<bool>> CanDeleteProduct(int id)
    {
        try
        {
            var product = await _productService.GetProductByIdAsync(id);
            if (product == null)
            {
                return NotFound(new { message = "Không tìm thấy sản phẩm" });
            }

            // Always soft delete (same as supplier logic)
            return Ok(new { 
                canDelete = true, 
                willSoftDelete = true,
                message = "Sản phẩm sẽ được chuyển sang trạng thái ngừng kinh doanh" 
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while checking if product can be deleted {ProductId}", id);
            return StatusCode(500, new { message = "Đã xảy ra lỗi khi kiểm tra sản phẩm" });
        }
    }

    /// <summary>
    /// Lấy danh sách sản phẩm đang hoạt động (cho dropdown, selection)
    /// </summary>
    [HttpGet("active")]
    [Authorize(Roles = "Admin,Manager,Employee")] // All roles can access
    public async Task<ActionResult<List<ProductDto>>> GetActiveProducts()
    {
        try
        {
            var activeProducts = await _productService.GetActiveProductsAsync();
            return Ok(activeProducts);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting active products");
            return StatusCode(500, new { message = "Đã xảy ra lỗi khi lấy danh sách sản phẩm đang hoạt động" });
        }
    }

    /// <summary>
    /// Kiểm tra xem sản phẩm có chuyển động kho không
    /// </summary>
    [HttpGet("{id}/has-inventory-movements")]
    [Authorize(Roles = "Admin,Manager,Employee")] // All roles can access
    public async Task<ActionResult<bool>> HasInventoryMovements(int id)
    {
        try
        {
            var hasMovements = await _productService.HasInventoryMovementsAsync(id);
            return Ok(hasMovements);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while checking inventory movements for product {ProductId}", id);
            return StatusCode(500, new { message = "Đã xảy ra lỗi khi kiểm tra chuyển động kho của sản phẩm" });
        }
    }
}
