using WarehouseManage.DTOs;

namespace WarehouseManage.Interfaces
{
    public interface IAuthService
    {
        Task<LoginResponseDto?> LoginAsync(LoginRequestDto loginRequest);
        Task<string> GenerateJwtTokenAsync(int userId, string username, string role);
        Task<bool> ValidateUserCredentialsAsync(string username, string password);
        string HashPassword(string password);
        bool VerifyPassword(string password, string hash);
    }
}
