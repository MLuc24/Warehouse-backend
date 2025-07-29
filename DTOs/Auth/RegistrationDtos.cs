using System.ComponentModel.DataAnnotations;

namespace WarehouseManage.DTOs.Auth;

public class SendVerificationCodeRequestDto
{
    [Required(ErrorMessage = "Email hoặc số điện thoại là bắt buộc")]
    public string Contact { get; set; } = null!;
    
    [Required(ErrorMessage = "Loại xác thực là bắt buộc")]
    [RegularExpression("^(Email|Phone)$", ErrorMessage = "Loại xác thực phải là 'Email' hoặc 'Phone'")]
    public string Type { get; set; } = null!; // "Email" hoặc "Phone"
}

public class VerifyCodeRequestDto
{
    [Required(ErrorMessage = "Email hoặc số điện thoại là bắt buộc")]
    public string Contact { get; set; } = null!;
    
    [Required(ErrorMessage = "Mã xác thực là bắt buộc")]
    [StringLength(6, MinimumLength = 6, ErrorMessage = "Mã xác thực phải có 6 chữ số")]
    public string Code { get; set; } = null!;
    
    [Required(ErrorMessage = "Loại xác thực là bắt buộc")]
    [RegularExpression("^(Email|Phone)$", ErrorMessage = "Loại xác thực phải là 'Email' hoặc 'Phone'")]
    public string Type { get; set; } = null!;
}

public class CompleteRegistrationRequestDto
{
    [Required(ErrorMessage = "Email hoặc số điện thoại là bắt buộc")]
    public string Contact { get; set; } = null!;
    
    [Required(ErrorMessage = "Loại xác thực là bắt buộc")]
    [RegularExpression("^(Email|Phone)$", ErrorMessage = "Loại xác thực phải là 'Email' hoặc 'Phone'")]
    public string Type { get; set; } = null!;
    
    [Required(ErrorMessage = "Tên đầy đủ là bắt buộc")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "Tên đầy đủ phải từ 2 đến 100 ký tự")]
    public string FullName { get; set; } = null!;
    
    [Required(ErrorMessage = "Tên đăng nhập là bắt buộc")]
    [StringLength(50, MinimumLength = 3, ErrorMessage = "Tên đăng nhập phải từ 3 đến 50 ký tự")]
    [RegularExpression("^[a-zA-Z0-9_]+$", ErrorMessage = "Tên đăng nhập chỉ được chứa chữ cái, số và dấu gạch dưới")]
    public string Username { get; set; } = null!;
    
    [Required(ErrorMessage = "Mật khẩu là bắt buộc")]
    [StringLength(100, MinimumLength = 6, ErrorMessage = "Mật khẩu phải từ 6 đến 100 ký tự")]
    public string Password { get; set; } = null!;
    
    [Required(ErrorMessage = "Xác nhận mật khẩu là bắt buộc")]
    [Compare("Password", ErrorMessage = "Xác nhận mật khẩu không khớp")]
    public string ConfirmPassword { get; set; } = null!;
}

public class RegistrationResponseDto
{
    public bool Success { get; set; }
    public string Message { get; set; } = null!;
    public LoginResponseDto? UserData { get; set; }
}
