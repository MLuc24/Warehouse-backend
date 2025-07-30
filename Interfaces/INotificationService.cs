namespace WarehouseManage.Interfaces
{
    /// <summary>
    /// Interface for notification services (Email, SMS)
    /// </summary>
    public interface INotificationService
    {
        /// <summary>
        /// Send email notification
        /// </summary>
        /// <param name="to">Recipient email address</param>
        /// <param name="subject">Email subject</param>
        /// <param name="body">Email body content</param>
        /// <returns>True if sent successfully, false otherwise</returns>
        Task<bool> SendEmailAsync(string to, string subject, string body);

        /// <summary>
        /// Send SMS notification
        /// </summary>
        /// <param name="to">Recipient phone number</param>
        /// <param name="message">SMS message content</param>
        /// <returns>True if sent successfully, false otherwise</returns>
        Task<bool> SendSmsAsync(string to, string message);

        /// <summary>
        /// Send verification code via email
        /// </summary>
        /// <param name="email">Recipient email</param>
        /// <param name="code">Verification code</param>
        /// <param name="purpose">Purpose of verification (registration, password reset, etc.)</param>
        /// <returns>True if sent successfully, false otherwise</returns>
        Task<bool> SendVerificationEmailAsync(string email, string code, string purpose);

        /// <summary>
        /// Send verification code via SMS
        /// </summary>
        /// <param name="phoneNumber">Recipient phone number</param>
        /// <param name="code">Verification code</param>
        /// <param name="purpose">Purpose of verification (registration, password reset, etc.)</param>
        /// <returns>True if sent successfully, false otherwise</returns>
        Task<bool> SendVerificationSmsAsync(string phoneNumber, string code, string purpose);
    }
}
