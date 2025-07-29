using Microsoft.EntityFrameworkCore;
using WarehouseManage.Constants;
using WarehouseManage.Data;
using WarehouseManage.Interfaces;
using WarehouseManage.Model;

namespace WarehouseManage.Services;

public class VerificationService : IVerificationService
{
    private readonly WarehouseDbContext _context;
    private readonly IEmailService _emailService;
    private readonly ISmsService _smsService;
    private readonly ILogger<VerificationService> _logger;

    public VerificationService(
        WarehouseDbContext context,
        IEmailService emailService,
        ISmsService smsService,
        ILogger<VerificationService> logger)
    {
        _context = context;
        _emailService = emailService;
        _smsService = smsService;
        _logger = logger;
    }

    public async Task<bool> SendVerificationCodeAsync(string contact, string type)
    {
        try
        {
            // Xóa các mã cũ chưa sử dụng
            await CleanupOldCodesAsync(contact, type);

            // Kiểm tra cooldown (không cho gửi lại quá nhanh)
            var lastCode = await _context.VerificationCodes
                .Where(c => c.Contact == contact && c.Type == type)
                .OrderByDescending(c => c.CreatedAt)
                .FirstOrDefaultAsync();

            if (lastCode != null && 
                DateTime.UtcNow.Subtract(lastCode.CreatedAt).TotalSeconds < VerificationConstants.RESEND_COOLDOWN_SECONDS)
            {
                return false; // Còn trong thời gian cooldown
            }

            // Tạo mã xác thực mới
            var code = GenerateVerificationCode();
            var verificationCode = new VerificationCode
            {
                Contact = contact,
                Code = code,
                Type = type,
                Purpose = VerificationConstants.Purposes.REGISTRATION,
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddMinutes(VerificationConstants.CODE_EXPIRY_MINUTES)
            };

            _context.VerificationCodes.Add(verificationCode);
            await _context.SaveChangesAsync();

            // Gửi mã xác thực
            bool sent = false;
            if (type == VerificationConstants.Types.EMAIL)
            {
                sent = await _emailService.SendEmailAsync(
                    contact,
                    "Mã xác thực đăng ký tài khoản",
                    $"Mã xác thực của bạn là: {code}. Mã có hiệu lực trong {VerificationConstants.CODE_EXPIRY_MINUTES} phút."
                );
            }
            else if (type == VerificationConstants.Types.PHONE)
            {
                sent = await _smsService.SendSmsAsync(
                    contact,
                    $"Ma xac thuc cua ban la: {code}. Ma co hieu luc trong {VerificationConstants.CODE_EXPIRY_MINUTES} phut."
                );
            }

            if (!sent)
            {
                // Nếu gửi thất bại, xóa mã đã tạo
                _context.VerificationCodes.Remove(verificationCode);
                await _context.SaveChangesAsync();
            }

            return sent;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending verification code to {Contact}", contact);
            return false;
        }
    }

    public async Task<bool> VerifyCodeAsync(string contact, string code, string type, string purpose)
    {
        try
        {
            var verificationCode = await _context.VerificationCodes
                .FirstOrDefaultAsync(c => 
                    c.Contact == contact && 
                    c.Code == code && 
                    c.Type == type && 
                    c.Purpose == purpose &&
                    !c.IsUsed);

            if (verificationCode == null)
            {
                return false; // Mã không tồn tại
            }

            // Tăng số lần thử
            verificationCode.Attempts++;
            
            if (verificationCode.Attempts > VerificationConstants.MAX_ATTEMPTS)
            {
                verificationCode.IsUsed = true; // Đánh dấu là đã sử dụng để không thể thử lại
                await _context.SaveChangesAsync();
                return false; // Quá số lần thử
            }

            if (DateTime.UtcNow > verificationCode.ExpiresAt)
            {
                await _context.SaveChangesAsync();
                return false; // Mã đã hết hạn
            }

            // Đánh dấu mã đã được sử dụng
            verificationCode.IsUsed = true;
            await _context.SaveChangesAsync();

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error verifying code for {Contact}", contact);
            return false;
        }
    }

    public async Task<bool> IsContactVerifiedAsync(string contact, string type, string purpose)
    {
        try
        {
            var currentTime = DateTime.UtcNow;
            return await _context.VerificationCodes
                .AnyAsync(c => 
                    c.Contact == contact && 
                    c.Type == type && 
                    c.Purpose == purpose &&
                    c.IsUsed &&
                    c.ExpiresAt > currentTime);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking verification status for {Contact}", contact);
            return false;
        }
    }

    public async Task CleanupExpiredCodesAsync()
    {
        try
        {
            var expiredCodes = await _context.VerificationCodes
                .Where(c => c.ExpiresAt < DateTime.UtcNow)
                .ToListAsync();

            if (expiredCodes.Any())
            {
                _context.VerificationCodes.RemoveRange(expiredCodes);
                await _context.SaveChangesAsync();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error cleaning up expired verification codes");
        }
    }

    private async Task CleanupOldCodesAsync(string contact, string type)
    {
        var oldCodes = await _context.VerificationCodes
            .Where(c => c.Contact == contact && c.Type == type && !c.IsUsed)
            .ToListAsync();

        if (oldCodes.Any())
        {
            _context.VerificationCodes.RemoveRange(oldCodes);
            await _context.SaveChangesAsync();
        }
    }

    private static string GenerateVerificationCode()
    {
        var random = new Random();
        return random.Next(100000, 999999).ToString();
    }
}
