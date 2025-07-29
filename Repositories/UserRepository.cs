using Microsoft.EntityFrameworkCore;
using WarehouseManage.Data;
using WarehouseManage.Interfaces;
using WarehouseManage.Model;

namespace WarehouseManage.Repositories;

public class UserRepository : IUserRepository
{
    private readonly WarehouseDbContext _context;

    public UserRepository(WarehouseDbContext context)
    {
        _context = context;
    }

    public async Task<User?> GetByUsernameAsync(string username)
    {
        if (string.IsNullOrWhiteSpace(username))
            return null;

        return await _context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Username == username);
    }

    public async Task<User?> GetByIdAsync(int userId)
    {
        if (userId <= 0)
            return null;

        return await _context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.UserId == userId);
    }

    public async Task<bool> IsUsernameExistsAsync(string username)
    {
        if (string.IsNullOrWhiteSpace(username))
            return false;

        return await _context.Users
            .AsNoTracking()
            .AnyAsync(u => u.Username == username);
    }
}
