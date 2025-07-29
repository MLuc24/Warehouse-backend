using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace WarehouseManage.Extensions;

/// <summary>
/// Extensions for basic validation needed by backend business logic only
/// Frontend handles all UI validation, format checking, and user experience
/// </summary>
public static class ValidationExtensions
{
    /// <summary>
    /// Check if ID is valid for database operations
    /// </summary>
    public static bool IsValidId(this int id) => id > 0;
    
    /// <summary>
    /// Check if string is not empty for required business fields
    /// </summary>
    public static bool IsNotEmpty(this string? value) => !string.IsNullOrWhiteSpace(value);
    
    /// <summary>
    /// Basic email validation using .NET built-in validator
    /// Used only for business logic validation, not UI feedback
    /// </summary>
    public static bool IsValidEmail(this string? email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return false;
            
        return new EmailAddressAttribute().IsValid(email);
    }
    
    /// <summary>
    /// Basic phone number validation for database constraints
    /// Frontend handles detailed format validation and UX
    /// </summary>
    public static bool IsValidPhoneNumber(this string? phoneNumber)
    {
        if (string.IsNullOrWhiteSpace(phoneNumber))
            return false;
        
        // Simple check for Vietnamese phone format - business logic only
        var phoneRegex = new Regex(@"^(\+84|84|0)(3[2-9]|5[689]|7[06-9]|8[1-689]|9[0-46-9])[0-9]{7}$");
        var cleanPhone = phoneNumber.Replace(" ", "").Replace("-", "").Replace("(", "").Replace(")", "");
        
        return phoneRegex.IsMatch(cleanPhone);
    }
}
