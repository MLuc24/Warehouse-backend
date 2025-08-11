using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using WarehouseManage.Constants;
using WarehouseManage.DTOs.Auth;
using WarehouseManage.DTOs.Profile;
using WarehouseManage.Interfaces;

namespace WarehouseManage.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ProfileController : ControllerBase
{
    private readonly IUserRepository _userRepository;
    private readonly ILogger<ProfileController> _logger;

    public ProfileController(
        IUserRepository userRepository,
        ILogger<ProfileController> logger)
    {
        _userRepository = userRepository;
        _logger = logger;
    }

    /// <summary>
    /// Get current user profile
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetProfile()
    {
        try
        {
            if (!User.Identity?.IsAuthenticated ?? true)
            {
                return Unauthorized(new { success = false, message = "User not authenticated" });
            }
            
            var userIdClaim = User.FindFirst(AuthConstants.ClaimTypes.USER_ID)?.Value;
            
            // Check for userId claim
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
            {
                return Unauthorized(new { message = "Invalid user token" });
            }

            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
            {
                return NotFound(new { message = "User not found" });
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
            _logger.LogError(ex, "Error getting user profile");
            return StatusCode(500, new { message = "Internal server error" });
        }
    }

    /// <summary>
    /// Update user profile
    /// </summary>
    [HttpPut]
    public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileRequestDto request)
    {
        try
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
            {
                return Unauthorized(new { message = "Invalid user token" });
            }

            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
            {
                return NotFound(new { message = "User not found" });
            }

            // Update user fields
            user.FullName = request.FullName;
            user.PhoneNumber = request.PhoneNumber;
            user.Address = request.Address;
            user.Image = request.Image;

            await _userRepository.UpdateAsync(user);

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

            _logger.LogInformation("User {UserId} updated profile successfully", userId);
            return Ok(new { success = true, message = "Profile updated successfully", data = userInfo });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating user profile");
            return StatusCode(500, new { message = "Internal server error" });
        }
    }

    /// <summary>
    /// Change user password
    /// </summary>
    [HttpPut("change-password")]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequestDto request)
    {
        try
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
            {
                return Unauthorized(new { message = "Invalid user token" });
            }

            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
            {
                return NotFound(new { message = "User not found" });
            }

            // Verify current password
            if (!WarehouseManage.Helpers.PasswordHelper.VerifyPassword(request.CurrentPassword, user.PasswordHash))
            {
                return BadRequest(new { message = "Current password is incorrect" });
            }

            // Validate new password
            if (request.NewPassword != request.ConfirmPassword)
            {
                return BadRequest(new { message = "New password and confirmation do not match" });
            }

            if (request.NewPassword.Length < 6)
            {
                return BadRequest(new { message = "New password must be at least 6 characters long" });
            }

            // Check if new password is different from current
            if (WarehouseManage.Helpers.PasswordHelper.VerifyPassword(request.NewPassword, user.PasswordHash))
            {
                return BadRequest(new { message = "New password must be different from current password" });
            }

            // Hash new password and update
            user.PasswordHash = WarehouseManage.Helpers.PasswordHelper.HashPassword(request.NewPassword);
            await _userRepository.UpdateAsync(user);

            _logger.LogInformation("User {UserId} changed password successfully", userId);
            return Ok(new { success = true, message = "Password changed successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error changing user password");
            return StatusCode(500, new { message = "Internal server error" });
        }
    }
}
