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
    private readonly IUserRepository _userRepository;
    private readonly ILogger<AuthController> _logger;

    public AuthController(IAuthService authService, IUserRepository userRepository, ILogger<AuthController> logger)
    {
        _authService = authService;
        _userRepository = userRepository;
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
    /// Kiểm tra trạng thái đăng nhập
    /// </summary>
    /// <returns>Thông tin người dùng hiện tại</returns>
    [HttpGet("me")]
    [Microsoft.AspNetCore.Authorization.Authorize]
    public async Task<IActionResult> GetCurrentUser()
    {
        try
        {
            var userIdClaim = User.FindFirst(AuthConstants.ClaimTypes.USER_ID)?.Value;
            
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
            {
                return Unauthorized(new { success = false, message = ErrorMessages.Auth.INVALID_TOKEN });
            }

            // Get full user data from database
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
            {
                return NotFound(new { success = false, message = "User not found" });
            }

            var userInfo = new UserInfoDto
            {
                UserId = user.UserId,
                Username = user.Username,
                FullName = user.FullName,
                Email = user.Email ?? string.Empty,
                PhoneNumber = user.PhoneNumber,
                Address = user.Address,
                Image = user.Image,
                Role = user.Role
            };

            return Ok(new { success = true, data = userInfo });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting current user info");
            return StatusCode(500, new { success = false, message = ErrorMessages.General.INTERNAL_ERROR });
        }
    }

    /// <summary>
    /// Đăng xuất khỏi hệ thống
    /// </summary>
    /// <returns>Kết quả đăng xuất</returns>
    [HttpPost("logout")]
    [Microsoft.AspNetCore.Authorization.Authorize]
    public async Task<IActionResult> Logout()
    {
        try
        {
            var result = await _authService.LogoutAsync();
            
            if (result)
            {
                return Ok(new { success = true, message = "Đăng xuất thành công." });
            }
            
            return StatusCode(500, new { success = false, message = "Có lỗi xảy ra khi đăng xuất." });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in Logout endpoint");
            return StatusCode(500, new { success = false, message = ErrorMessages.General.INTERNAL_ERROR });
        }
    }

    /// <summary>
    /// Endpoint chung để gửi mã xác thực với purpose cụ thể
    /// </summary>
    /// <param name="request">Thông tin gửi mã xác thực</param>
    /// <returns>Kết quả gửi mã</returns>
    [HttpPost("verification/send-code")]
    public async Task<IActionResult> SendVerificationCodeWithPurpose([FromBody] SendVerificationWithPurposeDto request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new { success = false, message = ErrorMessages.General.INVALID_INPUT, errors = ModelState });
            }

            var result = await _authService.SendVerificationCodeWithPurposeAsync(request.Contact, request.Type, request.Purpose);
            
            if (result.Success)
            {
                return Ok(result);
            }
            else
            {
                return BadRequest(result);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in SendVerificationCodeWithPurpose endpoint");
            return StatusCode(500, new { success = false, message = ErrorMessages.General.INTERNAL_ERROR });
        }
    }

    /// <summary>
    /// Endpoint chung để xác thực mã với purpose cụ thể
    /// </summary>
    /// <param name="request">Thông tin xác thực mã</param>
    /// <returns>Kết quả xác thực</returns>
    [HttpPost("verification/verify-code")]
    public async Task<IActionResult> VerifyCodeWithPurpose([FromBody] VerifyCodeWithPurposeDto request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new { success = false, message = ErrorMessages.General.INVALID_INPUT, errors = ModelState });
            }

            var result = await _authService.VerifyCodeWithPurposeAsync(request.Contact, request.Code, request.Type, request.Purpose);
            
            var message = request.Purpose switch
            {
                VerificationConstants.Purposes.REGISTRATION => "Xác thực đăng ký thành công.",
                VerificationConstants.Purposes.FORGOT_PASSWORD => "Xác thực đặt lại mật khẩu thành công.",
                _ => "Xác thực thành công."
            };

            var nextStep = request.Purpose switch
            {
                VerificationConstants.Purposes.REGISTRATION => "Hoàn tất thông tin đăng ký.",
                VerificationConstants.Purposes.FORGOT_PASSWORD => "Nhập mật khẩu mới.",
                _ => null
            };

            return Ok(new VerificationResponseDto
            {
                Success = result,
                Message = result ? message : "Mã xác thực không hợp lệ hoặc đã hết hạn.",
                NextStep = result ? nextStep : null
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in VerifyCodeWithPurpose endpoint");
            return StatusCode(500, new { success = false, message = ErrorMessages.General.INTERNAL_ERROR });
        }
    }

    /// <summary>
    /// Bước 3: Đặt lại mật khẩu mới
    /// </summary>
    /// <param name="request">Email, mã xác thực và mật khẩu mới</param>
    /// <returns>Kết quả đặt lại mật khẩu</returns>
    [HttpPost("forgot-password/reset")]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new { success = false, message = ErrorMessages.General.INVALID_INPUT, errors = ModelState });
            }

            var result = await _authService.ResetPasswordAsync(request);
            if (!result.Success)
            {
                return BadRequest(new { success = false, message = result.Message });
            }

            return Ok(new { success = true, message = result.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in ResetPassword endpoint");
            return StatusCode(500, new { success = false, message = ErrorMessages.General.INTERNAL_ERROR });
        }
    }
}
