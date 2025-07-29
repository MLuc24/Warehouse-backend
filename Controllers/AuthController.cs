using Microsoft.AspNetCore.Mvc;
using WarehouseManage.Constants;
using WarehouseManage.DTOs.Auth;
using WarehouseManage.Interfaces;

namespace WarehouseManage.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(IAuthService authService, ILogger<AuthController> logger)
    {
        _authService = authService;
        _logger = logger;
    }

    /// <summary>
    /// Đăng nhập vào hệ thống
    /// </summary>
    /// <param name="request">Thông tin đăng nhập</param>
    /// <returns>Token và thông tin người dùng</returns>
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequestDto request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new { message = ErrorMessages.General.INVALID_INPUT, errors = ModelState });
            }

            var result = await _authService.LoginAsync(request);
            return Ok(new { success = true, data = result });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { success = false, message = ex.Message });
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { success = false, message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled error in Login endpoint");
            return StatusCode(500, new { success = false, message = ErrorMessages.General.INTERNAL_ERROR });
        }
    }

    /// <summary>
    /// Kiểm tra trạng thái đăng nhập
    /// </summary>
    /// <returns>Thông tin người dùng hiện tại</returns>
    [HttpGet("me")]
    [Microsoft.AspNetCore.Authorization.Authorize]
    public IActionResult GetCurrentUser()
    {
        try
        {
            var userId = User.FindFirst(AuthConstants.ClaimTypes.USER_ID)?.Value;
            var username = User.FindFirst(AuthConstants.ClaimTypes.USERNAME)?.Value;
            var fullName = User.FindFirst(AuthConstants.ClaimTypes.FULL_NAME)?.Value;
            var role = User.FindFirst(AuthConstants.ClaimTypes.ROLE)?.Value;

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new { success = false, message = ErrorMessages.Auth.INVALID_TOKEN });
            }

            var userInfo = new UserInfoDto
            {
                UserId = int.Parse(userId),
                Username = username!,
                FullName = fullName!,
                Email = string.Empty, // Có thể lấy từ claims nếu cần
                Role = role!
            };

            return Ok(new { success = true, data = userInfo });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting current user info");
            return StatusCode(500, new { success = false, message = ErrorMessages.General.INTERNAL_ERROR });
        }
    }
}
