using System.ComponentModel.DataAnnotations;

namespace WarehouseManage.Interfaces;

/// <summary>
/// Base interface cho các service CRUD operations chung
/// </summary>
/// <typeparam name="TDto">DTO type</typeparam>
/// <typeparam name="TCreateDto">Create DTO type</typeparam>
/// <typeparam name="TUpdateDto">Update DTO type</typeparam>
/// <typeparam name="TSearchDto">Search DTO type</typeparam>
/// <typeparam name="TListResponseDto">List response DTO type</typeparam>
public interface IBaseService<TDto, TCreateDto, TUpdateDto, TSearchDto, TListResponseDto>
    where TDto : class
    where TCreateDto : class
    where TUpdateDto : class
    where TSearchDto : class
    where TListResponseDto : class
{
    // CRUD Operations
    Task<TDto?> GetByIdAsync(int id);
    Task<TListResponseDto> GetAllAsync(TSearchDto searchDto);
    Task<TDto> CreateAsync(TCreateDto createDto);
    Task<TDto?> UpdateAsync(int id, TUpdateDto updateDto);
    Task<bool> DeleteAsync(int id);
    Task<bool> ReactivateAsync(int id);
    
    // Business Logic
    Task<bool> ExistsAsync(int id);
}
