using System.ComponentModel.DataAnnotations;

namespace WarehouseManage.DTOs.Auth;

/// <summary>
/// DTO cho yêu cầu quên mật khẩu - bước 1: nhập email
/// </summary>
public class ForgotPasswordRequestDto
{
    [Required(ErrorMessage = "Email là bắt buộc")]
    [EmailAddress(ErrorMessage = "Định dạng email không hợp lệ")]
    [StringLength(100, ErrorMessage = "Email không được vượt quá 100 ký tự")]
    public string Email { get; set; } = null!;
}

/// <summary>
/// DTO cho xác thực mã OTP - bước 2: nhập mã xác thực
/// </summary>
public class VerifyForgotPasswordCodeDto
{
    [Required(ErrorMessage = "Email là bắt buộc")]
    [EmailAddress(ErrorMessage = "Định dạng email không hợp lệ")]
    public string Email { get; set; } = null!;
    
    [Required(ErrorMessage = "Mã xác thực là bắt buộc")]
    [StringLength(6, MinimumLength = 6, ErrorMessage = "Mã xác thực phải có 6 chữ số")]
    [RegularExpression("^[0-9]{6}$", ErrorMessage = "Mã xác thực chỉ được chứa số")]
    public string Code { get; set; } = null!;
}

/// <summary>
/// DTO cho đặt lại mật khẩu mới - bước 3: nhập mật khẩu mới
/// </summary>
public class ResetPasswordDto
{
    [Required(ErrorMessage = "Email là bắt buộc")]
    [EmailAddress(ErrorMessage = "Định dạng email không hợp lệ")]
    public string Email { get; set; } = null!;
    
    [Required(ErrorMessage = "Mật khẩu mới là bắt buộc")]
    [StringLength(100, MinimumLength = 6, ErrorMessage = "Mật khẩu phải từ 6 đến 100 ký tự")]
    [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]{6,}$", 
        ErrorMessage = "Mật khẩu phải chứa ít nhất 1 chữ hoa, 1 chữ thường, 1 số và 1 ký tự đặc biệt")]
    public string NewPassword { get; set; } = null!;
    
    [Required(ErrorMessage = "Xác nhận mật khẩu là bắt buộc")]
    [Compare("NewPassword", ErrorMessage = "Xác nhận mật khẩu không khớp")]
    public string ConfirmNewPassword { get; set; } = null!;
}

/// <summary>
/// DTO phản hồi cho các bước trong quá trình quên mật khẩu
/// </summary>
public class ForgotPasswordResponseDto
{
    public bool Success { get; set; }
    public string Message { get; set; } = null!;
    public string? NextStep { get; set; } // Hướng dẫn bước tiếp theo
}
