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
    private readonly WarehouseDbContext _context;

    public AuthService(
        IUserRepository userRepository,
        JwtHelper jwtHelper,
        IMapper mapper,
        ILogger<AuthService> logger,
        IVerificationService verificationService,
        WarehouseDbContext context)
    {
        _userRepository = userRepository;
        _jwtHelper = jwtHelper;
        _mapper = mapper;
        _logger = logger;
        _verificationService = verificationService;
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

    public async Task<bool> SendVerificationCodeAsync(SendVerificationCodeRequestDto request)
    {
        try
        {
            // Validate contact format
            if (!IsValidContactFormat(request.Contact, request.Type))
            {
                throw new ArgumentException(
                    request.Type == VerificationConstants.Types.EMAIL 
                        ? ErrorMessages.Auth.INVALID_EMAIL_FORMAT 
                        : ErrorMessages.Auth.INVALID_PHONE_FORMAT);
            }

            // Check if contact already exists
            if (!await IsContactAvailableAsync(request.Contact, request.Type))
            {
                throw new ArgumentException(
                    request.Type == VerificationConstants.Types.EMAIL 
                        ? ErrorMessages.Auth.EMAIL_ALREADY_EXISTS 
                        : ErrorMessages.Auth.PHONE_ALREADY_EXISTS);
            }

            var result = await _verificationService.SendVerificationCodeAsync(request.Contact, request.Type);
            if (!result)
            {
                throw new Exception(ErrorMessages.Auth.SEND_VERIFICATION_FAILED);
            }

            return true;
        }
        catch (ArgumentException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending verification code to {Contact}", request.Contact);
            throw new Exception(ErrorMessages.Auth.SEND_VERIFICATION_FAILED);
        }
    }

    public async Task<bool> VerifyCodeAsync(VerifyCodeRequestDto request)
    {
        try
        {
            var result = await _verificationService.VerifyCodeAsync(
                request.Contact, 
                request.Code, 
                request.Type, 
                VerificationConstants.Purposes.REGISTRATION);

            if (!result)
            {
                throw new ArgumentException(ErrorMessages.Auth.VERIFICATION_CODE_INVALID);
            }

            return true;
        }
        catch (ArgumentException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error verifying code for {Contact}", request.Contact);
            throw new Exception(ErrorMessages.General.INTERNAL_ERROR);
        }
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

            // Validate contact format
            if (!IsValidContactFormat(request.Contact, request.Type))
            {
                return new RegistrationResponseDto
                {
                    Success = false,
                    Message = request.Type == VerificationConstants.Types.EMAIL 
                        ? ErrorMessages.Auth.INVALID_EMAIL_FORMAT 
                        : ErrorMessages.Auth.INVALID_PHONE_FORMAT
                };
            }

            // Check if contact is still available
            if (!await IsContactAvailableAsync(request.Contact, request.Type))
            {
                return new RegistrationResponseDto
                {
                    Success = false,
                    Message = request.Type == VerificationConstants.Types.EMAIL 
                        ? ErrorMessages.Auth.EMAIL_ALREADY_EXISTS 
                        : ErrorMessages.Auth.PHONE_ALREADY_EXISTS
                };
            }

            // Check if username is available
            if (!await IsUsernameAvailableAsync(request.Username))
            {
                return new RegistrationResponseDto
                {
                    Success = false,
                    Message = ErrorMessages.Auth.USERNAME_ALREADY_EXISTS
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

    public async Task<bool> IsContactAvailableAsync(string contact, string type)
    {
        try
        {
            if (type == VerificationConstants.Types.EMAIL)
            {
                return !await _context.Users.AnyAsync(u => u.Email == contact);
            }
            else if (type == VerificationConstants.Types.PHONE)
            {
                return !await _context.Users.AnyAsync(u => u.PhoneNumber == contact);
            }
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking contact availability for {Contact}", contact);
            return false;
        }
    }

    public async Task<bool> IsUsernameAvailableAsync(string username)
    {
        try
        {
            return !await _context.Users.AnyAsync(u => u.Username == username);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking username availability for {Username}", username);
            return false;
        }
    }

    private static bool IsValidContactFormat(string contact, string type)
    {
        if (type == VerificationConstants.Types.EMAIL)
        {
            return contact.IsValidEmail();
        }
        else if (type == VerificationConstants.Types.PHONE)
        {
            return contact.IsValidPhoneNumber();
        }
        return false;
    }
}
