using System.ComponentModel.DataAnnotations;

namespace WarehouseManage.DTOs.Auth;

/// <summary>
/// DTO chung cho việc gửi mã xác thực với purpose cụ thể
/// </summary>
public class SendVerificationWithPurposeDto
{
    [Required(ErrorMessage = "Contact là bắt buộc")]
    [StringLength(255, ErrorMessage = "Contact không được vượt quá 255 ký tự")]
    public string Contact { get; set; } = null!;

    [Required(ErrorMessage = "Type là bắt buộc")]
    [StringLength(50, ErrorMessage = "Type không được vượt quá 50 ký tự")]
    public string Type { get; set; } = null!;

    [Required(ErrorMessage = "Purpose là bắt buộc")]
    [StringLength(50, ErrorMessage = "Purpose không được vượt quá 50 ký tự")]
    public string Purpose { get; set; } = null!;
}

/// <summary>
/// DTO chung cho việc xác thực mã với purpose cụ thể
/// </summary>
public class VerifyCodeWithPurposeDto
{
    [Required(ErrorMessage = "Contact là bắt buộc")]
    [StringLength(255, ErrorMessage = "Contact không được vượt quá 255 ký tự")]
    public string Contact { get; set; } = null!;

    [Required(ErrorMessage = "Code là bắt buộc")]
    [StringLength(6, MinimumLength = 6, ErrorMessage = "Code phải có 6 chữ số")]
    [RegularExpression("^[0-9]{6}$", ErrorMessage = "Code chỉ được chứa số")]
    public string Code { get; set; } = null!;

    [Required(ErrorMessage = "Type là bắt buộc")]
    [StringLength(50, ErrorMessage = "Type không được vượt quá 50 ký tự")]
    public string Type { get; set; } = null!;

    [Required(ErrorMessage = "Purpose là bắt buộc")]
    [StringLength(50, ErrorMessage = "Purpose không được vượt quá 50 ký tự")]
    public string Purpose { get; set; } = null!;
}

/// <summary>
/// DTO phản hồi chung cho các thao tác verification
/// </summary>
public class VerificationResponseDto
{
    public bool Success { get; set; }
    public string Message { get; set; } = null!;
    public string? NextStep { get; set; }
}
