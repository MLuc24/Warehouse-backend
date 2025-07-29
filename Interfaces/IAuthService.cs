using WarehouseManage.DTOs.Auth;

namespace WarehouseManage.Interfaces;

public interface IAuthService
{
    Task<LoginResponseDto> LoginAsync(LoginRequestDto request);
    Task<bool> ValidateUserCredentialsAsync(string username, string password);
    
    // Registration methods
    Task<bool> SendVerificationCodeAsync(SendVerificationCodeRequestDto request);
    Task<bool> VerifyCodeAsync(VerifyCodeRequestDto request);
    Task<RegistrationResponseDto> CompleteRegistrationAsync(CompleteRegistrationRequestDto request);
}
