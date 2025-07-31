using WarehouseManage.DTOs.Auth;

namespace WarehouseManage.Interfaces;

public interface IAuthService
{
    Task<LoginResponseDto> LoginAsync(LoginRequestDto request);
    Task<bool> ValidateUserCredentialsAsync(string username, string password);
    
    // Registration method - giữ lại vì có logic đặc biệt
    Task<RegistrationResponseDto> CompleteRegistrationAsync(CompleteRegistrationRequestDto request);
    
    // Common verification methods
    Task<VerificationResponseDto> SendVerificationCodeWithPurposeAsync(string contact, string type, string purpose);
    Task<bool> VerifyCodeWithPurposeAsync(string contact, string code, string type, string purpose);
    
    // Reset password method - giữ lại vì có logic đặc biệt  
    Task<ForgotPasswordResponseDto> ResetPasswordAsync(ResetPasswordDto request);
    
    // Logout method
    Task<bool> LogoutAsync();
}
