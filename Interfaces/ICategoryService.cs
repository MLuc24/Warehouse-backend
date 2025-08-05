using WarehouseManage.DTOs.Product;

namespace WarehouseManage.Interfaces;

/// <summary>
/// Interface cho dịch vụ quản lý danh mục sản phẩm
/// </summary>
public interface ICategoryService
{
    // CRUD operations
    Task<List<CategoryDto>> GetAllCategoriesAsync();
    Task<CategoryDto?> GetCategoryByIdAsync(int categoryId);
    Task<CategoryDto> CreateCategoryAsync(CreateCategoryDto createDto);
    Task<CategoryDto?> UpdateCategoryAsync(int categoryId, UpdateCategoryDto updateDto);
    Task<bool> DeleteCategoryAsync(int categoryId);

    // Special operations
    Task<List<DefaultCategoryDto>> GetDefaultCategoriesAsync();
    Task<bool> SeedDefaultCategoriesAsync(); // Seed dữ liệu mặc định
    Task<List<CategoryDto>> GetActiveCategoriesAsync();
}
