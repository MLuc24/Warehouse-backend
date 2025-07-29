using System.Text;
using WarehouseManage.Interfaces;

namespace WarehouseManage.Services;

public class SmsService : ISmsService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<SmsService> _logger;
    private readonly HttpClient _httpClient;

    public SmsService(IConfiguration configuration, ILogger<SmsService> logger, HttpClient httpClient)
    {
        _configuration = configuration;
        _logger = logger;
        _httpClient = httpClient;
    }

    public async Task<bool> SendSmsAsync(string phoneNumber, string message)
    {
        try
        {
            var apiKey = _configuration["SmsSettings:ApiKey"];
            var apiSecret = _configuration["SmsSettings:ApiSecret"];
            var brandName = _configuration["SmsSettings:BrandName"];
            var apiUrl = _configuration["SmsSettings:ApiUrl"];

            if (string.IsNullOrEmpty(apiKey) || string.IsNullOrEmpty(apiSecret) || 
                string.IsNullOrEmpty(apiUrl))
            {
                _logger.LogError("SMS configuration is missing");
                return false;
            }

            // Chuẩn hóa số điện thoại (thêm +84 nếu cần)
            var normalizedPhone = NormalizePhoneNumber(phoneNumber);

            // Tạo request payload (ví dụ cho Twilio hoặc các dịch vụ SMS khác)
            var payload = new
            {
                to = normalizedPhone,
                message = message,
                brandName = brandName
            };

            var json = System.Text.Json.JsonSerializer.Serialize(payload);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            // Thêm authentication header
            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");

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
}
