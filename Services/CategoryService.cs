using Microsoft.EntityFrameworkCore;
using WarehouseManage.Data;
using WarehouseManage.DTOs.Product;
using WarehouseManage.Interfaces;
using WarehouseManage.Model;

namespace WarehouseManage.Services;

public class CategoryService : ICategoryService
{
    private readonly WarehouseDbContext _context;
    private readonly ILogger<CategoryService> _logger;

    public CategoryService(WarehouseDbContext context, ILogger<CategoryService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<List<CategoryDto>> GetAllCategoriesAsync()
    {
        try
        {
            var categories = await _context.Categories
                .Include(c => c.Products)
                .Select(c => new CategoryDto
                {
                    CategoryId = c.CategoryId,
                    Name = c.Name,
                    Description = c.Description,
                    Icon = c.Icon,
                    Color = c.Color,
                    StorageType = c.StorageType,
                    IsPerishable = c.IsPerishable,
                    DefaultMinStock = c.DefaultMinStock,
                    DefaultMaxStock = c.DefaultMaxStock,
                    Status = c.Status,
                    CreatedAt = c.CreatedAt,
                    UpdatedAt = c.UpdatedAt,
                    ProductCount = c.Products.Count(p => p.Status == true)
                })
                .OrderBy(c => c.Name)
                .ToListAsync();

            return categories;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all categories");
            throw;
        }
    }

    public async Task<CategoryDto?> GetCategoryByIdAsync(int categoryId)
    {
        try
        {
            var category = await _context.Categories
                .Include(c => c.Products)
                .Where(c => c.CategoryId == categoryId)
                .Select(c => new CategoryDto
                {
                    CategoryId = c.CategoryId,
                    Name = c.Name,
                    Description = c.Description,
                    Icon = c.Icon,
                    Color = c.Color,
                    StorageType = c.StorageType,
                    IsPerishable = c.IsPerishable,
                    DefaultMinStock = c.DefaultMinStock,
                    DefaultMaxStock = c.DefaultMaxStock,
                    Status = c.Status,
                    CreatedAt = c.CreatedAt,
                    UpdatedAt = c.UpdatedAt,
                    ProductCount = c.Products.Count(p => p.Status == true)
                })
                .FirstOrDefaultAsync();

            return category;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting category by id {CategoryId}", categoryId);
            throw;
        }
    }

    public async Task<CategoryDto> CreateCategoryAsync(CreateCategoryDto createDto)
    {
        try
        {
            var category = new Category
            {
                Name = createDto.Name,
                Description = createDto.Description,
                Icon = createDto.Icon,
                Color = createDto.Color,
                StorageType = createDto.StorageType,
                IsPerishable = createDto.IsPerishable,
                DefaultMinStock = createDto.DefaultMinStock,
                DefaultMaxStock = createDto.DefaultMaxStock,
                Status = createDto.Status,
                CreatedAt = DateTime.Now
            };

            _context.Categories.Add(category);
            await _context.SaveChangesAsync();

            return new CategoryDto
            {
                CategoryId = category.CategoryId,
                Name = category.Name,
                Description = category.Description,
                Icon = category.Icon,
                Color = category.Color,
                StorageType = category.StorageType,
                IsPerishable = category.IsPerishable,
                DefaultMinStock = category.DefaultMinStock,
                DefaultMaxStock = category.DefaultMaxStock,
                Status = category.Status,
                CreatedAt = category.CreatedAt,
                UpdatedAt = category.UpdatedAt,
                ProductCount = 0
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating category");
            throw;
        }
    }

    public async Task<CategoryDto?> UpdateCategoryAsync(int categoryId, UpdateCategoryDto updateDto)
    {
        try
        {
            var category = await _context.Categories.FindAsync(categoryId);
            if (category == null)
                return null;

            category.Name = updateDto.Name;
            category.Description = updateDto.Description;
            category.Icon = updateDto.Icon;
            category.Color = updateDto.Color;
            category.StorageType = updateDto.StorageType;
            category.IsPerishable = updateDto.IsPerishable;
            category.DefaultMinStock = updateDto.DefaultMinStock;
            category.DefaultMaxStock = updateDto.DefaultMaxStock;
            category.Status = updateDto.Status;
            category.UpdatedAt = DateTime.Now;

            await _context.SaveChangesAsync();

            return await GetCategoryByIdAsync(categoryId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating category {CategoryId}", categoryId);
            throw;
        }
    }

    public async Task<bool> DeleteCategoryAsync(int categoryId)
    {
        try
        {
            var category = await _context.Categories
                .Include(c => c.Products)
                .FirstOrDefaultAsync(c => c.CategoryId == categoryId);

            if (category == null)
                return false;

            // Kiểm tra xem có sản phẩm nào đang sử dụng category này không
            if (category.Products.Any(p => p.Status == true))
            {
                throw new InvalidOperationException("Không thể xóa danh mục đang có sản phẩm hoạt động");
            }

            _context.Categories.Remove(category);
            await _context.SaveChangesAsync();
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting category {CategoryId}", categoryId);
            throw;
        }
    }

    public async Task<List<CategoryDto>> GetActiveCategoriesAsync()
    {
        try
        {
            var categories = await _context.Categories
                .Where(c => c.Status == true)
                .Include(c => c.Products)
                .Select(c => new CategoryDto
                {
                    CategoryId = c.CategoryId,
                    Name = c.Name,
                    Description = c.Description,
                    Icon = c.Icon,
                    Color = c.Color,
                    StorageType = c.StorageType,
                    IsPerishable = c.IsPerishable,
                    DefaultMinStock = c.DefaultMinStock,
                    DefaultMaxStock = c.DefaultMaxStock,
                    Status = c.Status,
                    CreatedAt = c.CreatedAt,
                    UpdatedAt = c.UpdatedAt,
                    ProductCount = c.Products.Count(p => p.Status == true)
                })
                .OrderBy(c => c.Name)
                .ToListAsync();

            return categories;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting active categories");
            throw;
        }
    }

    public async Task<List<DefaultCategoryDto>> GetDefaultCategoriesAsync()
    {
        try
        {
            // Danh sách category mặc định cho TocoToco
            var defaultCategories = new List<DefaultCategoryDto>
            {
                new DefaultCategoryDto
                {
                    Name = "Trà",
                    Description = "Các loại trà cơ bản: trà đen, trà xanh, trà ô long",
                    Icon = "🍃",
                    Color = "#4CAF50",
                    ExampleProducts = new List<string> { "Trà đen Ceylon", "Trà xanh Sencha", "Trà ô long Đài Loan", "Trà Earl Grey" },
                    StorageType = "Khô",
                    IsPerishable = false,
                    DefaultMinStock = 50,
                    DefaultMaxStock = 200
                },
                new DefaultCategoryDto
                {
                    Name = "Sữa",
                    Description = "Sữa tươi, sữa đặc, kem tươi cho pha chế",
                    Icon = "🥛",
                    Color = "#FFF9C4",
                    ExampleProducts = new List<string> { "Sữa tươi không đường", "Sữa đặc có đường", "Kem tươi whipping", "Sữa hạnh nhân" },
                    StorageType = "Lạnh",
                    IsPerishable = true,
                    DefaultMinStock = 20,
                    DefaultMaxStock = 100
                },
                new DefaultCategoryDto
                {
                    Name = "Topping",
                    Description = "Các loại topping: trân châu, thạch, pudding",
                    Icon = "🧋",
                    Color = "#8D6E63",
                    ExampleProducts = new List<string> { "Trân châu đen", "Trân châu trắng", "Thạch dừa", "Pudding trứng", "Trân châu ngọc trai" },
                    StorageType = "Lạnh",
                    IsPerishable = true,
                    DefaultMinStock = 30,
                    DefaultMaxStock = 150
                },
                new DefaultCategoryDto
                {
                    Name = "Syrup",
                    Description = "Các loại syrup, đường, chất ngọt",
                    Icon = "🍯",
                    Color = "#FF9800",
                    ExampleProducts = new List<string> { "Syrup vanilla", "Syrup caramel", "Đường phèn", "Syrup hazelnut", "Đường mía" },
                    StorageType = "Khô",
                    IsPerishable = false,
                    DefaultMinStock = 25,
                    DefaultMaxStock = 100
                },
                new DefaultCategoryDto
                {
                    Name = "Bột pha chế",
                    Description = "Bột cacao, matcha, coffee và các loại bột pha chế khác",
                    Icon = "☕",
                    Color = "#795548",
                    ExampleProducts = new List<string> { "Bột cacao nguyên chất", "Matcha Nhật Bản", "Bột coffee espresso", "Bột chocolate" },
                    StorageType = "Khô",
                    IsPerishable = false,
                    DefaultMinStock = 15,
                    DefaultMaxStock = 80
                },
                new DefaultCategoryDto
                {
                    Name = "Trái cây",
                    Description = "Trái cây tươi và đông lạnh cho pha chế",
                    Icon = "🍓",
                    Color = "#E91E63",
                    ExampleProducts = new List<string> { "Dâu tây tươi", "Xoài cắt lát", "Chanh tươi", "Cam tươi", "Lý chua đông lạnh" },
                    StorageType = "Lạnh",
                    IsPerishable = true,
                    DefaultMinStock = 10,
                    DefaultMaxStock = 50
                },
                new DefaultCategoryDto
                {
                    Name = "Vật tư",
                    Description = "Ly, ống hút, nắp ly, túi đựng",
                    Icon = "🥤",
                    Color = "#607D8B",
                    ExampleProducts = new List<string> { "Ly nhựa 500ml", "Ống hút giấy", "Nắp ly có lỗ", "Túi nilon đựng đồ uống", "Seal ly nhựa" },
                    StorageType = "Khô",
                    IsPerishable = false,
                    DefaultMinStock = 100,
                    DefaultMaxStock = 500
                },
                new DefaultCategoryDto
                {
                    Name = "Đá",
                    Description = "Đá viên, đá bào cho pha chế",
                    Icon = "🧊",
                    Color = "#B3E5FC",
                    ExampleProducts = new List<string> { "Đá viên trắng", "Đá bào mịn", "Đá ngọc trai" },
                    StorageType = "Đông lạnh",
                    IsPerishable = true,
                    DefaultMinStock = 200,
                    DefaultMaxStock = 1000
                }
            };

            return await Task.FromResult(defaultCategories);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting default categories");
            throw;
        }
    }

    public async Task<bool> SeedDefaultCategoriesAsync()
    {
        try
        {
            // Kiểm tra xem đã có category nào chưa
            var existingCount = await _context.Categories.CountAsync();
            if (existingCount > 0)
            {
                _logger.LogInformation("Categories already exist, skipping seed");
                return false;
            }

            var defaultCategories = await GetDefaultCategoriesAsync();
            var categories = defaultCategories.Select(dc => new Category
            {
                Name = dc.Name,
                Description = dc.Description,
                Icon = dc.Icon,
                Color = dc.Color,
                StorageType = dc.StorageType,
                IsPerishable = dc.IsPerishable,
                DefaultMinStock = dc.DefaultMinStock,
                DefaultMaxStock = dc.DefaultMaxStock,
                Status = true,
                CreatedAt = DateTime.Now
            }).ToList();

            _context.Categories.AddRange(categories);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Successfully seeded {Count} default categories", categories.Count);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error seeding default categories");
            throw;
        }
    }
}
