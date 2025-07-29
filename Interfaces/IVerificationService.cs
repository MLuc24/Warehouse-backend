namespace WarehouseManage.Interfaces;

public interface IVerificationService
{
    Task<bool> SendVerificationCodeAsync(string contact, string type);
    Task<bool> VerifyCodeAsync(string contact, string code, string type, string purpose);
    Task<bool> IsContactVerifiedAsync(string contact, string type, string purpose);
    Task CleanupExpiredCodesAsync();
}

public interface IEmailService
{
    Task<bool> SendEmailAsync(string to, string subject, string body);
}

public interface ISmsService
{
    Task<bool> SendSmsAsync(string phoneNumber, string message);
}
