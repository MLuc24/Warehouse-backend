using System.ComponentModel.DataAnnotations;

namespace WarehouseManage.DTOs.Profile;

public class ProfileResponseDto
{
    public int Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string? Address { get; set; }
    public string? Image { get; set; }
    public string Role { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

public class UpdateProfileRequestDto
{
    [Required]
    [StringLength(100, MinimumLength = 1)]
    public string FullName { get; set; } = string.Empty;

    [StringLength(15)]
    public string? PhoneNumber { get; set; }

    [StringLength(255)]
    public string? Address { get; set; }

    [StringLength(500)]
    public string? Image { get; set; }
}

public class ChangePasswordRequestDto
{
    [Required]
    public string CurrentPassword { get; set; } = string.Empty;

    [Required]
    [StringLength(100, MinimumLength = 6)]
    public string NewPassword { get; set; } = string.Empty;

    [Required]
    [Compare("NewPassword", ErrorMessage = "The new password and confirmation password do not match.")]
    public string ConfirmPassword { get; set; } = string.Empty;
}
