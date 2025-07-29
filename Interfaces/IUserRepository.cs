using WarehouseManage.Model;

namespace WarehouseManage.Interfaces;

public interface IUserRepository
{
    Task<User?> GetByUsernameAsync(string username);
    Task<User?> GetByIdAsync(int userId);
    Task<bool> IsUsernameExistsAsync(string username);
}
