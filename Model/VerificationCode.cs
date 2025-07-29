using System;

namespace WarehouseManage.Model;

public class VerificationCode
{
    public int Id { get; set; }
    
    public string Contact { get; set; } = null!; // Email hoặc số điện thoại
    
    public string Code { get; set; } = null!;
    
    public string Type { get; set; } = null!; // "Email" hoặc "Phone"
    
    public string Purpose { get; set; } = null!; // "Registration", "ForgotPassword", etc.
    
    public DateTime CreatedAt { get; set; }
    
    public DateTime ExpiresAt { get; set; }
    
    public bool IsUsed { get; set; } = false;
    
    public int Attempts { get; set; } = 0;
    
    public bool IsExpired => DateTime.UtcNow > ExpiresAt;
}
