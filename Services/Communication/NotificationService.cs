using System.Net;
using System.Net.Mail;
using System.Text;
using System.Text.RegularExpressions;
using WarehouseManage.Interfaces;

namespace WarehouseManage.Services.Communication;

public class NotificationService : INotificationService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<NotificationService> _logger;
    private readonly HttpClient _httpClient;

    public NotificationService(IConfiguration configuration, ILogger<NotificationService> logger, HttpClient httpClient)
    {
        _configuration = configuration;
        _logger = logger;
        _httpClient = httpClient;
    }

    public async Task<bool> SendEmailAsync(string to, string subject, string body)
    {
        try
        {
            // Chỉ kiểm tra format email cơ bản
            if (!IsValidEmailFormat(to))
            {
                _logger.LogWarning("Email {Email} has invalid format", to);
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
                IsBodyHtml = true // Enable HTML email
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
            // Chỉ kiểm tra format phone cơ bản và normalize
            var normalizedPhone = NormalizePhoneNumber(phoneNumber);
            if (!IsValidPhoneFormat(normalizedPhone))
            {
                _logger.LogWarning("Phone number {Phone} has invalid format", phoneNumber);
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

    private bool IsValidEmailFormat(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return false;

        try
        {
            // Sử dụng MailAddress để validate format
            var addr = new MailAddress(email);
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

        // Kiểm tra format phone đơn giản - phải có ít nhất 10 số và bắt đầu bằng +
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

    public Task<bool> ValidateContactAsync(string contact, string type)
    {
        var result = type.ToLower() switch
        {
            "email" => IsValidEmailFormat(contact),
            "phone" => IsValidPhoneFormat(NormalizePhoneNumber(contact)),
            _ => false
        };
        
        return Task.FromResult(result);
    }
}
