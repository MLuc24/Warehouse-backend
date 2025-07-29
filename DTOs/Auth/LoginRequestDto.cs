using System.ComponentModel.DataAnnotations;

namespace WarehouseManage.DTOs.Auth;

public class LoginRequestDto
{
    [Required(ErrorMessage = "Tên đăng nhập là bắt buộc")]
    [StringLength(50, MinimumLength = 3, ErrorMessage = "Tên đăng nhập phải từ 3-50 ký tự")]
    public string Username { get; set; } = null!;

    [Required(ErrorMessage = "Mật khẩu là bắt buộc")]
    [StringLength(100, MinimumLength = 6, ErrorMessage = "Mật khẩu phải từ 6-100 ký tự")]
    public string Password { get; set; } = null!;
}
