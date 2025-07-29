using System.Net;
using System.Net.Mail;
using System.Text;
using System.Text.Json;
using WarehouseManage.Interfaces;

namespace WarehouseManage.Services.Communication;

public class NotificationService : INotificationService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<NotificationService> _logger;
    private readonly HttpClient _httpClient;
    private readonly IWebHostEnvironment _environment;

    public NotificationService(IConfiguration configuration, ILogger<NotificationService> logger, HttpClient httpClient, IWebHostEnvironment environment)
    {
        _configuration = configuration;
        _logger = logger;
        _httpClient = httpClient;
        _environment = environment;
    }

    public async Task<bool> SendEmailAsync(string to, string subject, string body)
    {
        try
        {
            // Kiểm tra email có tồn tại thực tế không
            var isValidEmail = await ValidateEmailExistsAsync(to);
            if (!isValidEmail)
            {
                _logger.LogWarning("Email {Email} does not exist or is invalid", to);
                return false;
            }

            var smtpHost = _configuration["EmailSettings:SmtpHost"];
            var smtpPort = int.Parse(_configuration["EmailSettings:SmtpPort"] ?? "587");
            var smtpUsername = Environment.GetEnvironmentVariable("SMTP_USERNAME");
            var smtpPassword = Environment.GetEnvironmentVariable("SMTP_PASSWORD");
            var fromEmail = Environment.GetEnvironmentVariable("SMTP_FROM_EMAIL");
            var fromName = _configuration["EmailSettings:FromName"];

            if (string.IsNullOrEmpty(smtpHost) || string.IsNullOrEmpty(smtpUsername) || 
                string.IsNullOrEmpty(smtpPassword) || string.IsNullOrEmpty(fromEmail))
            {
                _logger.LogError("Email configuration is missing in environment variables");
                return false;
            }

            using var client = new SmtpClient(smtpHost, smtpPort)
            {
                EnableSsl = true,
                Credentials = new NetworkCredential(smtpUsername, smtpPassword)
            };

            var mailMessage = new MailMessage
            {
                From = new MailAddress(fromEmail, fromName),
                Subject = subject,
                Body = body,
                IsBodyHtml = false
            };
            
            mailMessage.To.Add(to);

            await client.SendMailAsync(mailMessage);
            _logger.LogInformation("Email sent successfully to {Email}", to);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send email to {Email}", to);
            return false;
        }
    }

    public async Task<bool> SendSmsAsync(string phoneNumber, string message)
    {
        try
        {
            // Kiểm tra số điện thoại có tồn tại thực tế không
            var normalizedPhone = NormalizePhoneNumber(phoneNumber);
            var isValidPhone = await ValidatePhoneExistsAsync(normalizedPhone);
            if (!isValidPhone)
            {
                _logger.LogWarning("Phone number {Phone} does not exist or is invalid", phoneNumber);
                return false;
            }

            var apiKey = Environment.GetEnvironmentVariable("TWILIO_ACCOUNT_SID");
            var apiSecret = Environment.GetEnvironmentVariable("TWILIO_AUTH_TOKEN");
            var fromNumber = Environment.GetEnvironmentVariable("TWILIO_FROM_NUMBER");
            var apiUrl = _configuration["SmsSettings:ApiUrl"]?.Replace("ACCOUNT_SID", apiKey);

            if (string.IsNullOrEmpty(apiKey) || string.IsNullOrEmpty(apiSecret) || 
                string.IsNullOrEmpty(apiUrl) || string.IsNullOrEmpty(fromNumber))
            {
                _logger.LogError("SMS configuration is missing in environment variables");
                return false;
            }

            // Tạo form data cho Twilio API
            var formData = new List<KeyValuePair<string, string>>
            {
                new("To", normalizedPhone),
                new("From", fromNumber),
                new("Body", message)
            };

            var content = new FormUrlEncodedContent(formData);

            // Twilio sử dụng Basic Authentication
            var credentials = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{apiKey}:{apiSecret}"));
            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Basic {credentials}");

            var response = await _httpClient.PostAsync(apiUrl, content);

            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("SMS sent successfully to {PhoneNumber}", phoneNumber);
                return true;
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogError("Failed to send SMS to {PhoneNumber}. Status: {StatusCode}, Error: {Error}", 
                    phoneNumber, response.StatusCode, errorContent);
                return false;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception occurred while sending SMS to {PhoneNumber}", phoneNumber);
            return false;
        }
    }

    private async Task<bool> ValidateEmailExistsAsync(string email)
    {
        try
        {
            var emailValidationApiKey = Environment.GetEnvironmentVariable("EMAIL_VALIDATION_API_KEY");
            if (string.IsNullOrEmpty(emailValidationApiKey))
            {
                _logger.LogWarning("Email validation API key not configured, skipping validation");
                return true; // Chỉ skip khi không có API key
            }

            var apiUrl = $"https://emailvalidation.abstractapi.com/v1/?api_key={emailValidationApiKey}&email={email}";
            
            var response = await _httpClient.GetAsync(apiUrl);
            if (response.IsSuccessStatusCode)
            {
                var jsonResponse = await response.Content.ReadAsStringAsync();
                _logger.LogInformation("Email validation response for {Email}: {Response}", email, jsonResponse);
                
                var result = JsonSerializer.Deserialize<EmailValidationResponse>(jsonResponse);
                
                // Nghiêm ngặt - chỉ chấp nhận email DELIVERABLE
                var isValid = result?.Deliverability == "DELIVERABLE";
                
                if (!isValid)
                {
                    _logger.LogWarning("Email validation FAILED for {Email}. Deliverability: {Deliverability}, SmtpValid: {SmtpValid}. Email does not exist or is not deliverable.", 
                        email, result?.Deliverability, result?.IsSmtpValid);
                }
                else
                {
                    _logger.LogInformation("Email validation PASSED for {Email}. Email is deliverable.", email);
                }
                
                return isValid;
            }
            
            _logger.LogError("Email validation API failed for {Email} with status: {StatusCode}", email, response.StatusCode);
            return false; // Nếu API lỗi, không cho phép gửi khi có API key
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating email {Email}", email);
            return false; // Nếu có lỗi, không cho phép gửi khi có API key
        }
    }

    private async Task<bool> ValidatePhoneExistsAsync(string phoneNumber)
    {
        try
        {
            var phoneValidationApiKey = Environment.GetEnvironmentVariable("PHONE_VALIDATION_API_KEY");
            if (string.IsNullOrEmpty(phoneValidationApiKey))
            {
                _logger.LogWarning("Phone validation API key not configured, skipping validation");
                return true; // Chỉ skip khi không có API key
            }

            var apiUrl = $"https://phonevalidation.abstractapi.com/v1/?api_key={phoneValidationApiKey}&phone={phoneNumber}";
            
            var response = await _httpClient.GetAsync(apiUrl);
            if (response.IsSuccessStatusCode)
            {
                var jsonResponse = await response.Content.ReadAsStringAsync();
                _logger.LogInformation("Phone validation response for {Phone}: {Response}", phoneNumber, jsonResponse);
                
                var result = JsonSerializer.Deserialize<PhoneValidationResponse>(jsonResponse);
                
                // Nghiêm ngặt - chỉ chấp nhận số điện thoại hợp lệ
                var isValid = result?.Valid == true;
                
                if (!isValid)
                {
                    _logger.LogWarning("Phone validation FAILED for {Phone}. Valid: {Valid}, Carrier: {Carrier}. Phone does not exist or is invalid.", 
                        phoneNumber, result?.Valid, result?.Carrier);
                }
                else
                {
                    _logger.LogInformation("Phone validation PASSED for {Phone}. Phone is valid.", phoneNumber);
                }
                
                return isValid;
            }
            
            _logger.LogError("Phone validation API failed for {Phone} with status: {StatusCode}", phoneNumber, response.StatusCode);
            return false; // Nếu API lỗi, không cho phép gửi khi có API key
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating phone {Phone}", phoneNumber);
            return false; // Nếu có lỗi, không cho phép gửi khi có API key
        }
    }

    private string NormalizePhoneNumber(string phoneNumber)
    {
        // Loại bỏ khoảng trắng và các ký tự đặc biệt
        phoneNumber = phoneNumber.Replace(" ", "").Replace("-", "").Replace("(", "").Replace(")", "");

        // Nếu số bắt đầu bằng 0, thay thế bằng +84
        if (phoneNumber.StartsWith("0"))
        {
            phoneNumber = "+84" + phoneNumber.Substring(1);
        }
        // Nếu số bắt đầu bằng 84, thêm +
        else if (phoneNumber.StartsWith("84"))
        {
            phoneNumber = "+" + phoneNumber;
        }
        // Nếu chưa có mã quốc gia, thêm +84
        else if (!phoneNumber.StartsWith("+"))
        {
            phoneNumber = "+84" + phoneNumber;
        }

        return phoneNumber;
    }

    public async Task<bool> ValidateContactAsync(string contact, string type)
    {
        return type.ToLower() switch
        {
            "email" => await ValidateEmailExistsAsync(contact),
            "phone" => await ValidatePhoneExistsAsync(contact),
            _ => false
        };
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
