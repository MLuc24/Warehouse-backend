namespace WarehouseManage.Interfaces;

/// <summary>
/// Base interface cho các repository CRUD operations chung
/// </summary>
/// <typeparam name="TEntity">Entity type</typeparam>
/// <typeparam name="TSearchDto">Search DTO type</typeparam>
/// <typeparam name="TListResponseDto">List response DTO type</typeparam>
public interface IBaseRepository<TEntity, TSearchDto, TListResponseDto>
    where TEntity : class
    where TSearchDto : class
    where TListResponseDto : class
{
    // CRUD Operations
    Task<TEntity?> GetByIdAsync(int id);
    Task<TListResponseDto> GetAllAsync(TSearchDto searchDto);
    Task<TEntity> CreateAsync(TEntity entity);
    Task<TEntity?> UpdateAsync(int id, TEntity entity);
    Task<bool> DeleteAsync(int id);
    Task<bool> ReactivateAsync(int id);
    
    // Additional queries
    Task<bool> ExistsAsync(int id);
    Task<bool> ExistsByNameAsync(string name, int? excludeId = null);
    Task<bool> ExistsByEmailAsync(string email, int? excludeId = null);
}
