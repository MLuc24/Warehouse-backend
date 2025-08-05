using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WarehouseManage.DTOs.Product;
using WarehouseManage.Interfaces;

namespace WarehouseManage.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CategoryController : ControllerBase
{
    private readonly ICategoryService _categoryService;
    private readonly ILogger<CategoryController> _logger;

    public CategoryController(ICategoryService categoryService, ILogger<CategoryController> logger)
    {
        _categoryService = categoryService;
        _logger = logger;
    }

    /// <summary>
    /// Lấy danh sách tất cả danh mục
    /// </summary>
    [HttpGet]
    [Authorize(Roles = "Admin,Manager,Employee")]
    public async Task<ActionResult<List<CategoryDto>>> GetAllCategories()
    {
        try
        {
            var categories = await _categoryService.GetAllCategoriesAsync();
            return Ok(categories);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all categories");
            return StatusCode(500, new { message = "Đã xảy ra lỗi khi lấy danh sách danh mục" });
        }
    }

    /// <summary>
    /// Lấy danh sách danh mục đang hoạt động
    /// </summary>
    [HttpGet("active")]
    [Authorize(Roles = "Admin,Manager,Employee")]
    public async Task<ActionResult<List<CategoryDto>>> GetActiveCategories()
    {
        try
        {
            var categories = await _categoryService.GetActiveCategoriesAsync();
            return Ok(categories);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting active categories");
            return StatusCode(500, new { message = "Đã xảy ra lỗi khi lấy danh sách danh mục hoạt động" });
        }
    }

    /// <summary>
    /// Lấy danh mục theo ID
    /// </summary>
    [HttpGet("{id}")]
    [Authorize(Roles = "Admin,Manager,Employee")]
    public async Task<ActionResult<CategoryDto>> GetCategoryById(int id)
    {
        try
        {
            var category = await _categoryService.GetCategoryByIdAsync(id);
            if (category == null)
                return NotFound(new { message = "Không tìm thấy danh mục" });

            return Ok(category);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting category by id {CategoryId}", id);
            return StatusCode(500, new { message = "Đã xảy ra lỗi khi lấy thông tin danh mục" });
        }
    }

    /// <summary>
    /// Tạo danh mục mới
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<ActionResult<CategoryDto>> CreateCategory([FromBody] CreateCategoryDto createDto)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var category = await _categoryService.CreateCategoryAsync(createDto);
            return CreatedAtAction(nameof(GetCategoryById), new { id = category.CategoryId }, category);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating category");
            return StatusCode(500, new { message = "Đã xảy ra lỗi khi tạo danh mục" });
        }
    }

    /// <summary>
    /// Cập nhật danh mục
    /// </summary>
    [HttpPut("{id}")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<ActionResult<CategoryDto>> UpdateCategory(int id, [FromBody] UpdateCategoryDto updateDto)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var category = await _categoryService.UpdateCategoryAsync(id, updateDto);
            if (category == null)
                return NotFound(new { message = "Không tìm thấy danh mục" });

            return Ok(category);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating category {CategoryId}", id);
            return StatusCode(500, new { message = "Đã xảy ra lỗi khi cập nhật danh mục" });
        }
    }

    /// <summary>
    /// Xóa danh mục
    /// </summary>
    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<ActionResult> DeleteCategory(int id)
    {
        try
        {
            var result = await _categoryService.DeleteCategoryAsync(id);
            if (!result)
                return NotFound(new { message = "Không tìm thấy danh mục" });

            return Ok(new { message = "Xóa danh mục thành công" });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting category {CategoryId}", id);
            return StatusCode(500, new { message = "Đã xảy ra lỗi khi xóa danh mục" });
        }
    }

    /// <summary>
    /// Lấy danh sách category mặc định cho TocoToco
    /// </summary>
    [HttpGet("default")]
    [Authorize(Roles = "Admin,Manager,Employee")]
    public async Task<ActionResult<List<DefaultCategoryDto>>> GetDefaultCategories()
    {
        try
        {
            var categories = await _categoryService.GetDefaultCategoriesAsync();
            return Ok(categories);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting default categories");
            return StatusCode(500, new { message = "Đã xảy ra lỗi khi lấy danh sách danh mục mặc định" });
        }
    }

    /// <summary>
    /// Khởi tạo dữ liệu category mặc định cho TocoToco
    /// </summary>
    [HttpPost("seed")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult> SeedDefaultCategories()
    {
        try
        {
            var seeded = await _categoryService.SeedDefaultCategoriesAsync();
            if (!seeded)
                return BadRequest(new { message = "Đã có dữ liệu danh mục, không thể khởi tạo lại" });

            return Ok(new { message = "Khởi tạo dữ liệu danh mục mặc định thành công" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error seeding default categories");
            return StatusCode(500, new { message = "Đã xảy ra lỗi khi khởi tạo dữ liệu danh mục" });
        }
    }
}
