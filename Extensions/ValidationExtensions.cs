using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace WarehouseManage.Extensions;

public static class ValidationExtensions
{
    public static bool IsValidId(this int id) => id > 0;
    
    public static bool IsNotEmpty(this string? value) => !string.IsNullOrWhiteSpace(value);
    
    public static bool IsValidUsername(this string? username)
    {
        if (string.IsNullOrWhiteSpace(username))
            return false;
            
        return username.Length >= 3 && username.Length <= 50;
    }
    
    public static bool IsValidPassword(this string? password)
    {
        if (string.IsNullOrWhiteSpace(password))
            return false;
            
        return password.Length >= 6;
    }
    
    public static bool IsValidEmail(this string? email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return false;
            
        return new EmailAddressAttribute().IsValid(email);
    }
    
    public static bool IsValidPhoneNumber(this string? phoneNumber)
    {
        if (string.IsNullOrWhiteSpace(phoneNumber))
            return false;
        
        // Regex cho số điện thoại Việt Nam
        // Hỗ trợ: 0xxxxxxxxx, +84xxxxxxxxx, 84xxxxxxxxx
        var phoneRegex = new Regex(@"^(\+84|84|0)(3[2-9]|5[689]|7[06-9]|8[1-689]|9[0-46-9])[0-9]{7}$");
        
        // Loại bỏ khoảng trắng và dấu gạch ngang
        var cleanPhone = phoneNumber.Replace(" ", "").Replace("-", "");
        
        return phoneRegex.IsMatch(cleanPhone);
    }
}
