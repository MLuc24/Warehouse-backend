using System.ComponentModel.DataAnnotations;

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
}
