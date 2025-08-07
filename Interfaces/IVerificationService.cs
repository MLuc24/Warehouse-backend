namespace WarehouseManage.Interfaces;

public interface IVerificationService
{
    Task<bool> SendVerificationCodeAsync(string contact, string type);
    Task<bool> SendVerificationCodeAsync(string contact, string type, string purpose);
    Task<bool> VerifyCodeAsync(string contact, string code, string type, string purpose);
    Task<bool> IsContactVerifiedAsync(string contact, string type, string purpose);
    Task CleanupExpiredCodesAsync();
}

public interface INotificationService
{
    Task<bool> SendEmailAsync(string to, string subject, string body);
    Task<bool> SendEmailWithAttachmentsAsync(string to, string subject, string body, List<(string fileName, byte[] content, string mimeType)> attachments);
    Task<bool> SendSmsAsync(string phoneNumber, string message);
    Task<bool> ValidateContactAsync(string contact, string type);
}

// Legacy interfaces - deprecated, use INotificationService instead
[Obsolete("Use INotificationService instead")]
public interface IEmailService
{
    Task<bool> SendEmailAsync(string to, string subject, string body);
}

[Obsolete("Use INotificationService instead")]
public interface ISmsService
{
    Task<bool> SendSmsAsync(string phoneNumber, string message);
}
