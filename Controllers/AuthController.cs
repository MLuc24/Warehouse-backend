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
    /// Gửi mã xác thực đến email hoặc số điện thoại
    /// </summary>
    /// <param name="request">Thông tin liên hệ và loại xác thực</param>
    /// <returns>Kết quả gửi mã xác thực</returns>
    [HttpPost("send-verification-code")]
    public async Task<IActionResult> SendVerificationCode([FromBody] SendVerificationCodeRequestDto request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new { success = false, message = ErrorMessages.General.INVALID_INPUT, errors = ModelState });
            }

            await _authService.SendVerificationCodeAsync(request);
            return Ok(new { success = true, message = ErrorMessages.Auth.VERIFICATION_CODE_SENT });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { success = false, message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled error in SendVerificationCode endpoint");
            return StatusCode(500, new { success = false, message = ErrorMessages.General.INTERNAL_ERROR });
        }
    }

    /// <summary>
    /// Xác thực mã được gửi về email hoặc SMS
    /// </summary>
    /// <param name="request">Thông tin xác thực mã</param>
    /// <returns>Kết quả xác thực</returns>
    [HttpPost("verify-code")]
    public async Task<IActionResult> VerifyCode([FromBody] VerifyCodeRequestDto request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new { success = false, message = ErrorMessages.General.INVALID_INPUT, errors = ModelState });
            }

            await _authService.VerifyCodeAsync(request);
            return Ok(new { success = true, message = "Xác thực thành công" });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { success = false, message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled error in VerifyCode endpoint");
            return StatusCode(500, new { success = false, message = ErrorMessages.General.INTERNAL_ERROR });
        }
    }

    /// <summary>
    /// Hoàn tất quá trình đăng ký tài khoản
    /// </summary>
    /// <param name="request">Thông tin đăng ký đầy đủ</param>
    /// <returns>Kết quả đăng ký và token đăng nhập</returns>
    [HttpPost("complete-registration")]
    public async Task<IActionResult> CompleteRegistration([FromBody] CompleteRegistrationRequestDto request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new { success = false, message = ErrorMessages.General.INVALID_INPUT, errors = ModelState });
            }

            var result = await _authService.CompleteRegistrationAsync(request);
            
            if (!result.Success)
            {
                return BadRequest(new { success = false, message = result.Message });
            }

            return Ok(new { success = true, message = result.Message, data = result.UserData });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled error in CompleteRegistration endpoint");
            return StatusCode(500, new { success = false, message = ErrorMessages.General.INTERNAL_ERROR });
        }
    }

    /// <summary>
    /// Kiểm tra tính khả dụng của email hoặc số điện thoại
    /// </summary>
    /// <param name="contact">Email hoặc số điện thoại</param>
    /// <param name="type">Loại liên hệ (Email hoặc Phone)</param>
    /// <returns>Tình trạng khả dụng</returns>
    [HttpGet("check-contact-availability")]
    public async Task<IActionResult> CheckContactAvailability([FromQuery] string contact, [FromQuery] string type)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(contact) || string.IsNullOrWhiteSpace(type))
            {
                return BadRequest(new { success = false, message = ErrorMessages.General.INVALID_INPUT });
            }

            var isAvailable = await _authService.IsContactAvailableAsync(contact, type);
            return Ok(new { success = true, available = isAvailable });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled error in CheckContactAvailability endpoint");
            return StatusCode(500, new { success = false, message = ErrorMessages.General.INTERNAL_ERROR });
        }
    }

    /// <summary>
    /// Kiểm tra tính khả dụng của tên đăng nhập
    /// </summary>
    /// <param name="username">Tên đăng nhập</param>
    /// <returns>Tình trạng khả dụng</returns>
    [HttpGet("check-username-availability")]
    public async Task<IActionResult> CheckUsernameAvailability([FromQuery] string username)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(username))
            {
                return BadRequest(new { success = false, message = ErrorMessages.General.INVALID_INPUT });
            }

            var isAvailable = await _authService.IsUsernameAvailableAsync(username);
            return Ok(new { success = true, available = isAvailable });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled error in CheckUsernameAvailability endpoint");
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
