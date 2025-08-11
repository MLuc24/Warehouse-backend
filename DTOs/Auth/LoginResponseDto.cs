namespace WarehouseManage.DTOs.Auth;

public class LoginResponseDto
{
    public string Token { get; set; } = null!;
    public string RefreshToken { get; set; } = null!;
    public UserInfoDto User { get; set; } = null!;
    public DateTime ExpiresAt { get; set; }
}

public class UserInfoDto
{
    public int UserId { get; set; }
    public string Username { get; set; } = null!;
    public string FullName { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string? PhoneNumber { get; set; }
    public string? Address { get; set; }
    public string? Image { get; set; }
    public string Role { get; set; } = null!;
}
