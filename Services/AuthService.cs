using AutoMapper;
using WarehouseManage.Constants;
using WarehouseManage.DTOs.Auth;
using WarehouseManage.Helpers;
using WarehouseManage.Interfaces;

namespace WarehouseManage.Services;

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly JwtHelper _jwtHelper;
    private readonly IMapper _mapper;
    private readonly ILogger<AuthService> _logger;

    public AuthService(
        IUserRepository userRepository,
        JwtHelper jwtHelper,
        IMapper mapper,
        ILogger<AuthService> logger)
    {
        _userRepository = userRepository;
        _jwtHelper = jwtHelper;
        _mapper = mapper;
        _logger = logger;
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
}
