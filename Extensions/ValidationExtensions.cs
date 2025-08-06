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
    /// Check if quantity is valid for goods receipt
    /// </summary>
    public static bool IsValidQuantity(this int quantity) => quantity > 0 && quantity <= 999999;
    
    /// <summary>
    /// Check if unit price is valid for goods receipt
    /// </summary>
    public static bool IsValidUnitPrice(this decimal unitPrice) => unitPrice > 0 && unitPrice <= 999999999.99m;
    
    /// <summary>
    /// Check if receipt number follows the correct format
    /// </summary>
    public static bool IsValidReceiptNumber(this string? receiptNumber)
    {
        if (string.IsNullOrWhiteSpace(receiptNumber))
            return false;
            
        // Format: GR20250806001 (GR + YYYYMMDD + 3 digits)
        return Regex.IsMatch(receiptNumber, @"^GR\d{8}\d{3}$");
    }
    
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
