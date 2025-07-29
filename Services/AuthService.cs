using AutoMapper;
using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;
using WarehouseManage.Constants;
using WarehouseManage.Data;
using WarehouseManage.DTOs.Auth;
using WarehouseManage.Extensions;
using WarehouseManage.Helpers;
using WarehouseManage.Interfaces;
using WarehouseManage.Model;

namespace WarehouseManage.Services;

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly JwtHelper _jwtHelper;
    private readonly IMapper _mapper;
    private readonly ILogger<AuthService> _logger;
    private readonly IVerificationService _verificationService;
    private readonly IValidationService _validationService;
    private readonly WarehouseDbContext _context;

    public AuthService(
        IUserRepository userRepository,
        JwtHelper jwtHelper,
        IMapper mapper,
        ILogger<AuthService> logger,
        IVerificationService verificationService,
        IValidationService validationService,
        WarehouseDbContext context)
    {
        _userRepository = userRepository;
        _jwtHelper = jwtHelper;
        _mapper = mapper;
        _logger = logger;
        _verificationService = verificationService;
        _validationService = validationService;
        _context = context;
    }

    public async Task<LoginResponseDto> LoginAsync(LoginRequestDto request)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Password))
            {
                throw new ArgumentException(ErrorMessages.Auth.INVALID_CREDENTIALS);
            }

            var user = await _userRepository.GetByUsernameAsync(request.Username);
            if (user == null)
            {
                _logger.LogWarning("Login attempt with non-existent username: {Username}", request.Username);
                throw new UnauthorizedAccessException(ErrorMessages.Auth.INVALID_CREDENTIALS);
            }

            if (user.Status != true)
            {
                _logger.LogWarning("Login attempt with disabled account: {Username}", request.Username);
                throw new UnauthorizedAccessException(ErrorMessages.Auth.ACCOUNT_DISABLED);
            }

            if (!PasswordHelper.VerifyPassword(request.Password, user.PasswordHash))
            {
                _logger.LogWarning("Login attempt with invalid password for user: {Username}", request.Username);
                throw new UnauthorizedAccessException(ErrorMessages.Auth.INVALID_CREDENTIALS);
            }

            var accessToken = _jwtHelper.GenerateAccessToken(user);
            var refreshToken = _jwtHelper.GenerateRefreshToken();
            var expiryTime = _jwtHelper.GetTokenExpiryTime();

            _logger.LogInformation("User {Username} logged in successfully", user.Username);

            return new LoginResponseDto
            {
                Token = accessToken,
                RefreshToken = refreshToken,
                ExpiresAt = expiryTime,
                User = new UserInfoDto
                {
                    UserId = user.UserId,
                    Username = user.Username,
                    FullName = user.FullName,
                    Email = user.Email ?? string.Empty,
                    Role = user.Role
                }
            };
        }
        catch (ArgumentException)
        {
            throw;
        }
        catch (UnauthorizedAccessException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during login for username: {Username}", request.Username);
            throw new Exception(ErrorMessages.General.INTERNAL_ERROR);
        }
    }

    public async Task<bool> ValidateUserCredentialsAsync(string username, string password)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
                return false;

            var user = await _userRepository.GetByUsernameAsync(username);
            if (user == null || user.Status != true)
                return false;

            return PasswordHelper.VerifyPassword(password, user.PasswordHash);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating credentials for username: {Username}", username);
            return false;
        }
    }

    // Common method for sending verification code with different purposes
    private async Task<bool> SendVerificationCodeInternalAsync(string contact, string type, string purpose)
    {
        try
        {
            return await _verificationService.SendVerificationCodeAsync(contact, type, purpose);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending verification code to {Contact} for purpose {Purpose}", contact, purpose);
            return false;
        }
    }

    // Common method for verifying code with different purposes
    private async Task<bool> VerifyCodeInternalAsync(string contact, string code, string type, string purpose)
    {
        try
        {
            return await _verificationService.VerifyCodeAsync(contact, code, type, purpose);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error verifying code for {Contact} with purpose {Purpose}", contact, purpose);
            return false;
        }
    }

    // Public method for sending verification code with specific purpose
    public async Task<bool> SendVerificationCodeWithPurposeAsync(string contact, string type, string purpose)
    {
        return await SendVerificationCodeInternalAsync(contact, type, purpose);
    }

    // Public method for verifying code with specific purpose
    public async Task<bool> VerifyCodeWithPurposeAsync(string contact, string code, string type, string purpose)
    {
        return await VerifyCodeInternalAsync(contact, code, type, purpose);
    }

    public async Task<RegistrationResponseDto> CompleteRegistrationAsync(CompleteRegistrationRequestDto request)
    {
        try
        {
            // Kiểm tra xem contact đã được verify chưa
            var isVerified = await _verificationService.IsContactVerifiedAsync(
                request.Contact, 
                request.Type, 
                VerificationConstants.Purposes.REGISTRATION);

            if (!isVerified)
            {
                return new RegistrationResponseDto
                {
                    Success = false,
                    Message = ErrorMessages.Auth.VERIFICATION_REQUIRED
                };
            }

            // Sử dụng ValidationService để kiểm tra logic nghiệp vụ
            var email = request.Type == VerificationConstants.Types.EMAIL ? request.Contact : null;
            var phoneNumber = request.Type == VerificationConstants.Types.PHONE ? request.Contact : null;
            
            var validationResult = await _validationService.ValidateUserRegistrationAsync(
                request.Username, 
                email ?? "", 
                phoneNumber);

            if (!validationResult.IsValid)
            {
                return new RegistrationResponseDto
                {
                    Success = false,
                    Message = string.Join(", ", validationResult.Errors)
                };
            }

            // Create new user
            var passwordHash = PasswordHelper.HashPassword(request.Password);
            var user = new User
            {
                Username = request.Username,
                PasswordHash = passwordHash,
                FullName = request.FullName,
                Email = request.Type == VerificationConstants.Types.EMAIL ? request.Contact : null,
                PhoneNumber = request.Type == VerificationConstants.Types.PHONE ? request.Contact : null,
                IsEmailVerified = request.Type == VerificationConstants.Types.EMAIL,
                IsPhoneVerified = request.Type == VerificationConstants.Types.PHONE,
                Role = "User", // Default role
                Status = true,
                CreatedAt = DateTime.UtcNow
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            // Generate tokens
            var accessToken = _jwtHelper.GenerateAccessToken(user);
            var refreshToken = _jwtHelper.GenerateRefreshToken();
            var expiryTime = _jwtHelper.GetTokenExpiryTime();

            var loginResponse = new LoginResponseDto
            {
                Token = accessToken,
                RefreshToken = refreshToken,
                ExpiresAt = expiryTime,
                User = new UserInfoDto
                {
                    UserId = user.UserId,
                    Username = user.Username,
                    FullName = user.FullName,
                    Email = user.Email ?? string.Empty,
                    Role = user.Role
                }
            };

            _logger.LogInformation("User {Username} registered successfully", user.Username);

            return new RegistrationResponseDto
            {
                Success = true,
                Message = "Đăng ký thành công",
                UserData = loginResponse
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error completing registration for {Contact}", request.Contact);
            return new RegistrationResponseDto
            {
                Success = false,
                Message = ErrorMessages.General.INTERNAL_ERROR
            };
        }
    }

    public async Task<ForgotPasswordResponseDto> ResetPasswordAsync(ResetPasswordDto request)
    {
        try
        {
            // Kiểm tra email có tồn tại trong hệ thống không
            var user = await _userRepository.GetByEmailAsync(request.Email);
            if (user == null)
            {
                return new ForgotPasswordResponseDto
                {
                    Success = false,
                    Message = "Email không hợp lệ."
                };
            }

            // Sử dụng method chung để xác thực mã lần nữa để đảm bảo an toàn
            var isCodeValid = await VerifyCodeInternalAsync(
                request.Email,
                request.Code,
                VerificationConstants.Types.EMAIL,
                VerificationConstants.Purposes.FORGOT_PASSWORD
            );

            if (!isCodeValid)
            {
                return new ForgotPasswordResponseDto
                {
                    Success = false,
                    Message = "Mã xác thực không hợp lệ hoặc đã hết hạn."
                };
            }

            // Đặt lại mật khẩu
            var hashedPassword = PasswordHelper.HashPassword(request.NewPassword);
            user.PasswordHash = hashedPassword;

            await _userRepository.UpdateAsync(user);

            // Log thành công
            _logger.LogInformation("Password reset successfully for user: {Email}", request.Email);

            return new ForgotPasswordResponseDto
            {
                Success = true,
                Message = "Mật khẩu đã được đặt lại thành công. Bạn có thể đăng nhập bằng mật khẩu mới."
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error resetting password for email: {Email}", request.Email);
            return new ForgotPasswordResponseDto
            {
                Success = false,
                Message = ErrorMessages.General.INTERNAL_ERROR
            };
        }
    }

    public async Task<bool> LogoutAsync()
    {
        try
        {
            // Đơn giản chỉ return true vì không có blacklist token
            // Client sẽ tự xóa token ở phía frontend
            await Task.CompletedTask;
            
            _logger.LogInformation("User logged out successfully");
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during logout");
            return false;
        }
    }
}
