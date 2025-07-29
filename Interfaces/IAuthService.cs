using WarehouseManage.DTOs.Auth;

namespace WarehouseManage.Interfaces;

public interface IAuthService
{
    Task<LoginResponseDto> LoginAsync(LoginRequestDto request);
    Task<bool> ValidateUserCredentialsAsync(string username, string password);
}
