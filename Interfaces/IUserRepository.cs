using WarehouseManage.Model;

namespace WarehouseManage.Interfaces;

public interface IUserRepository
{
    Task<User?> GetByUsernameAsync(string username);
    Task<User?> GetByEmailAsync(string email);
    Task<User?> GetByIdAsync(int userId);
    Task<bool> IsUsernameExistsAsync(string username);
    Task<bool> IsEmailExistsAsync(string email);
    Task UpdateAsync(User user);
    Task<User> CreateAsync(User user);
}
