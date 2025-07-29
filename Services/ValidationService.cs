using WarehouseManage.Data;
using WarehouseManage.Extensions;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace WarehouseManage.Services;

public class ValidationService : IValidationService
{
    private readonly WarehouseDbContext _context;
    private readonly ILogger<ValidationService> _logger;
    private readonly IHttpClientFactory _httpClientFactory;

    public ValidationService(WarehouseDbContext context, ILogger<ValidationService> logger, IHttpClientFactory httpClientFactory)
    {
        _context = context;
        _logger = logger;
        _httpClientFactory = httpClientFactory;
    }

    public async Task<ValidationResult> ValidateUserRegistrationAsync(string username, string email, string? phoneNumber)
    {
        var errors = new List<string>();

        // Validate email format và existence với API
        var emailValidation = await ValidateEmailAsync(email);
        if (!emailValidation.IsValid)
        {
            errors.AddRange(emailValidation.Errors);
        }

        // Validate phone format và existence với API nếu có
        if (!string.IsNullOrEmpty(phoneNumber))
        {
            var phoneValidation = await ValidatePhoneAsync(phoneNumber);
            if (!phoneValidation.IsValid)
            {
                errors.AddRange(phoneValidation.Errors);
            }
        }

        // Check username availability
        if (await _context.Users.AnyAsync(u => u.Username == username))
        {
            errors.Add("Tên đăng nhập đã được sử dụng");
        }

        // Check email availability
        if (await _context.Users.AnyAsync(u => u.Email == email))
        {
            errors.Add("Email đã được sử dụng");
        }

        // Check phone number availability nếu có
        if (!string.IsNullOrEmpty(phoneNumber))
        {
            if (await _context.Users.AnyAsync(u => u.PhoneNumber == phoneNumber))
            {
                errors.Add("Số điện thoại đã được sử dụng");
            }
        }

        return new ValidationResult
        {
            IsValid = !errors.Any(),
            Errors = errors
        };
    }

    public ValidationResult ValidateUserLogin(string identifier)
    {
        var errors = new List<string>();

        // Chỉ validate business logic - frontend đã validate format
        if (string.IsNullOrWhiteSpace(identifier))
        {
            errors.Add("Vui lòng nhập email hoặc tên đăng nhập");
        }

        return new ValidationResult
        {
            IsValid = !errors.Any(),
            Errors = errors
        };
    }

    public async Task<ValidationResult> ValidateEmailAsync(string email)
    {
        var errors = new List<string>();

        // Kiểm tra format cơ bản
        if (!IsValidEmailFormat(email))
        {
            errors.Add("Email không đúng định dạng");
            return new ValidationResult { IsValid = false, Errors = errors };
        }

        // Kiểm tra email tồn tại với API
        var emailValidationApiKey = Environment.GetEnvironmentVariable("EMAIL_VALIDATION_API_KEY");
        if (!string.IsNullOrEmpty(emailValidationApiKey))
        {
            try
            {
                var httpClient = _httpClientFactory.CreateClient("ApiValidation");
                
                var apiUrl = $"https://emailvalidation.abstractapi.com/v1/?api_key={emailValidationApiKey}&email={email}";
                var response = await httpClient.GetAsync(apiUrl);
                
                if (response.IsSuccessStatusCode)
                {
                    var jsonResponse = await response.Content.ReadAsStringAsync();
                    var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                    var result = JsonSerializer.Deserialize<EmailValidationResponse>(jsonResponse, options);
                    
                    if (result?.Deliverability != "DELIVERABLE")
                    {
                        errors.Add("Email không tồn tại hoặc không thể gửi được");
                        _logger.LogInformation("Email validation failed for {Email}. Deliverability: {Deliverability}", email, result?.Deliverability);
                    }
                    else
                    {
                        _logger.LogInformation("Email validation passed for {Email}", email);
                    }
                }
                else
                {
                    _logger.LogWarning("Email validation API returned status {StatusCode} for {Email}", response.StatusCode, email);
                    // Không thêm error - để validation pass khi API không khả dụng
                }
            }
            catch (TaskCanceledException tcEx) when (tcEx.InnerException is TimeoutException)
            {
                _logger.LogInformation("Email validation timeout for {Email}. Skipping API validation to improve user experience.", email);
                // Không thêm error - cho phép email pass khi timeout
            }
            catch (HttpRequestException)
            {
                _logger.LogInformation("HTTP/SSL error during email validation for {Email}. Skipping API validation.", email);
                // Không thêm error - cho phép email pass khi có lỗi kết nối
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Unexpected error during email validation for {Email}. Skipping API validation.", email);
                // Không thêm error - cho phép email pass khi có lỗi bất ngờ
            }
        }
        else
        {
            _logger.LogInformation("Email validation API key not configured, skipping API validation for {Email}", email);
        }

        return new ValidationResult { IsValid = !errors.Any(), Errors = errors };
    }

    public async Task<ValidationResult> ValidatePhoneAsync(string phoneNumber)
    {
        var errors = new List<string>();

        // Normalize phone number
        var normalizedPhone = NormalizePhoneNumber(phoneNumber);
        
        // Kiểm tra format cơ bản
        if (!IsValidPhoneFormat(normalizedPhone))
        {
            errors.Add("Số điện thoại không đúng định dạng");
            return new ValidationResult { IsValid = false, Errors = errors };
        }

        // Kiểm tra phone tồn tại với API
        var phoneValidationApiKey = Environment.GetEnvironmentVariable("PHONE_VALIDATION_API_KEY");
        if (!string.IsNullOrEmpty(phoneValidationApiKey))
        {
            try
            {
                var httpClient = _httpClientFactory.CreateClient("ApiValidation");
                
                var apiUrl = $"https://phonevalidation.abstractapi.com/v1/?api_key={phoneValidationApiKey}&phone={normalizedPhone}";
                var response = await httpClient.GetAsync(apiUrl);
                
                if (response.IsSuccessStatusCode)
                {
                    var jsonResponse = await response.Content.ReadAsStringAsync();
                    var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                    var result = JsonSerializer.Deserialize<PhoneValidationResponse>(jsonResponse, options);
                    
                    if (result?.Valid != true)
                    {
                        errors.Add("Số điện thoại không tồn tại hoặc không hợp lệ");
                        _logger.LogInformation("Phone validation failed for {Phone}. Valid: {Valid}", normalizedPhone, result?.Valid);
                    }
                    else
                    {
                        _logger.LogInformation("Phone validation passed for {Phone}", normalizedPhone);
                    }
                }
                else
                {
                    _logger.LogWarning("Phone validation API returned status {StatusCode} for {Phone}", response.StatusCode, normalizedPhone);
                    // Không thêm error - để validation pass khi API không khả dụng
                }
            }
            catch (TaskCanceledException tcEx) when (tcEx.InnerException is TimeoutException)
            {
                _logger.LogInformation("Phone validation timeout for {Phone}. Skipping API validation to improve user experience.", normalizedPhone);
                // Không thêm error - cho phép phone pass khi timeout
            }
            catch (HttpRequestException)
            {
                _logger.LogInformation("HTTP/SSL error during phone validation for {Phone}. Skipping API validation.", normalizedPhone);
                // Không thêm error - cho phép phone pass khi có lỗi kết nối
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Unexpected error during phone validation for {Phone}. Skipping API validation.", normalizedPhone);
                // Không thêm error - cho phép phone pass khi có lỗi bất ngờ
            }
        }
        else
        {
            _logger.LogInformation("Phone validation API key not configured, skipping API validation for {Phone}", normalizedPhone);
        }

        return new ValidationResult { IsValid = !errors.Any(), Errors = errors };
    }

    private bool IsValidEmailFormat(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return false;

        try
        {
            var addr = new System.Net.Mail.MailAddress(email);
            return addr.Address == email;
        }
        catch
        {
            return false;
        }
    }

    private bool IsValidPhoneFormat(string phoneNumber)
    {
        if (string.IsNullOrWhiteSpace(phoneNumber))
            return false;

        // Kiểm tra format phone - phải có ít nhất 10 số và bắt đầu bằng +
        var phoneRegex = new Regex(@"^\+[1-9]\d{8,14}$");
        return phoneRegex.IsMatch(phoneNumber);
    }

    private string NormalizePhoneNumber(string phoneNumber)
    {
        if (string.IsNullOrWhiteSpace(phoneNumber))
            return phoneNumber;

        // Loại bỏ khoảng trắng và các ký tự đặc biệt
        phoneNumber = phoneNumber.Replace(" ", "").Replace("-", "").Replace("(", "").Replace(")", "").Replace(".", "");

        // Nếu số bắt đầu bằng 0, thay thế bằng +84 (Vietnam)
        if (phoneNumber.StartsWith("0"))
        {
            phoneNumber = "+84" + phoneNumber.Substring(1);
        }
        // Nếu số bắt đầu bằng 84, thêm +
        else if (phoneNumber.StartsWith("84") && !phoneNumber.StartsWith("+"))
        {
            phoneNumber = "+" + phoneNumber;
        }
        // Nếu chưa có mã quốc gia và không bắt đầu bằng +, thêm +84
        else if (!phoneNumber.StartsWith("+") && phoneNumber.Length >= 9)
        {
            phoneNumber = "+84" + phoneNumber;
        }

        return phoneNumber;
    }
}

// DTOs for API responses
public class EmailValidationResponse
{
    public string? Email { get; set; }
    public string? Deliverability { get; set; }
    public bool IsSmtpValid { get; set; }
}

public class PhoneValidationResponse
{
    public string? Phone { get; set; }
    public bool Valid { get; set; }
    public string? Carrier { get; set; }
    public string? Type { get; set; }
}

public class ValidationResult
{
    public bool IsValid { get; set; }
    public List<string> Errors { get; set; } = new();
}

public interface IValidationService
{
    Task<ValidationResult> ValidateUserRegistrationAsync(string username, string email, string? phoneNumber);
    ValidationResult ValidateUserLogin(string identifier);
    Task<ValidationResult> ValidateEmailAsync(string email);
    Task<ValidationResult> ValidatePhoneAsync(string phoneNumber);
}
