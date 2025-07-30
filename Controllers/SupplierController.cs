using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WarehouseManage.DTOs.Supplier;
using WarehouseManage.Interfaces;

namespace WarehouseManage.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize] // Require authentication
public class SupplierController : ControllerBase
{
    private readonly ISupplierService _supplierService;
    private readonly ILogger<SupplierController> _logger;

    public SupplierController(ISupplierService supplierService, ILogger<SupplierController> logger)
    {
        _supplierService = supplierService;
        _logger = logger;
    }

    /// <summary>
    /// Lấy danh sách tất cả nhà cung cấp (có phân trang và tìm kiếm)
    /// </summary>
    [HttpGet]
    [Authorize(Roles = "Admin")] // Only Admin can access
    public async Task<ActionResult<SupplierListResponseDto>> GetAllSuppliers([FromQuery] SupplierSearchDto searchDto)
    {
        try
        {
            var result = await _supplierService.GetAllSuppliersAsync(searchDto);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting suppliers");
            return StatusCode(500, new { message = "Đã xảy ra lỗi khi lấy danh sách nhà cung cấp" });
        }
    }

    /// <summary>
    /// Lấy thông tin nhà cung cấp theo ID
    /// </summary>
    [HttpGet("{id}")]
    [Authorize(Roles = "Admin")] // Only Admin can access
    public async Task<ActionResult<SupplierDto>> GetSupplierById(int id)
    {
        try
        {
            var supplier = await _supplierService.GetSupplierByIdAsync(id);
            if (supplier == null)
            {
                return NotFound(new { message = "Không tìm thấy nhà cung cấp" });
            }

            return Ok(supplier);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting supplier {SupplierId}", id);
            return StatusCode(500, new { message = "Đã xảy ra lỗi khi lấy thông tin nhà cung cấp" });
        }
    }

    /// <summary>
    /// Tạo nhà cung cấp mới
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Admin")] // Only Admin can create
    public async Task<ActionResult<SupplierDto>> CreateSupplier([FromBody] CreateSupplierDto createDto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var supplier = await _supplierService.CreateSupplierAsync(createDto);
            return CreatedAtAction(nameof(GetSupplierById), new { id = supplier.SupplierId }, supplier);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while creating supplier");
            return StatusCode(500, new { message = "Đã xảy ra lỗi khi tạo nhà cung cấp" });
        }
    }

    /// <summary>
    /// Cập nhật thông tin nhà cung cấp
    /// </summary>
    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")] // Only Admin can update
    public async Task<ActionResult<SupplierDto>> UpdateSupplier(int id, [FromBody] UpdateSupplierDto updateDto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var supplier = await _supplierService.UpdateSupplierAsync(id, updateDto);
            if (supplier == null)
            {
                return NotFound(new { message = "Không tìm thấy nhà cung cấp" });
            }

            return Ok(supplier);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while updating supplier {SupplierId}", id);
            return StatusCode(500, new { message = "Đã xảy ra lỗi khi cập nhật nhà cung cấp" });
        }
    }

    /// <summary>
    /// Xóa nhà cung cấp
    /// </summary>
    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")] // Only Admin can delete
    public async Task<ActionResult> DeleteSupplier(int id)
    {
        try
        {
            var result = await _supplierService.DeleteSupplierAsync(id);
            if (!result)
            {
                return NotFound(new { message = "Không tìm thấy nhà cung cấp" });
            }

            return Ok(new { message = "Xóa nhà cung cấp thành công" });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while deleting supplier {SupplierId}", id);
            return StatusCode(500, new { message = "Đã xảy ra lỗi khi xóa nhà cung cấp" });
        }
    }

    /// <summary>
    /// Lấy thống kê chi tiết của nhà cung cấp
    /// </summary>
    [HttpGet("{id}/statistics")]
    [Authorize(Roles = "Admin")] // Only Admin can access
    public async Task<ActionResult<SupplierStatsDto>> GetSupplierStatistics(int id)
    {
        try
        {
            var stats = await _supplierService.GetSupplierStatisticsAsync(id);
            if (stats == null)
            {
                return NotFound(new { message = "Không tìm thấy nhà cung cấp" });
            }

            return Ok(stats);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting supplier statistics {SupplierId}", id);
            return StatusCode(500, new { message = "Đã xảy ra lỗi khi lấy thống kê nhà cung cấp" });
        }
    }

    /// <summary>
    /// Lấy danh sách top nhà cung cấp (theo giá trị mua hàng)
    /// </summary>
    [HttpGet("top")]
    [Authorize(Roles = "Admin")] // Only Admin can access
    public async Task<ActionResult<List<SupplierDto>>> GetTopSuppliers([FromQuery] int count = 5)
    {
        try
        {
            var topSuppliers = await _supplierService.GetTopSuppliersAsync(count);
            return Ok(topSuppliers);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting top suppliers");
            return StatusCode(500, new { message = "Đã xảy ra lỗi khi lấy danh sách nhà cung cấp hàng đầu" });
        }
    }

    /// <summary>
    /// Kiểm tra nhà cung cấp có thể xóa được không
    /// </summary>
    [HttpGet("{id}/can-delete")]
    [Authorize(Roles = "Admin")] // Only Admin can access
    public async Task<ActionResult<bool>> CanDeleteSupplier(int id)
    {
        try
        {
            if (!await _supplierService.SupplierExistsAsync(id))
            {
                return NotFound(new { message = "Không tìm thấy nhà cung cấp" });
            }

            var canDelete = await _supplierService.CanDeleteSupplierAsync(id);
            return Ok(new { canDelete, message = canDelete ? "Có thể xóa nhà cung cấp" : "Không thể xóa nhà cung cấp do có dữ liệu liên quan" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while checking if supplier can be deleted {SupplierId}", id);
            return StatusCode(500, new { message = "Đã xảy ra lỗi khi kiểm tra nhà cung cấp" });
        }
    }
}
