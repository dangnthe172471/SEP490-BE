using SEP490_BE.BLL.IServices;
using SEP490_BE.DAL.IRepositories;
using SEP490_BE.DAL.Models;
using Microsoft.EntityFrameworkCore;

namespace SEP490_BE.BLL.Services
{
    public class OtpService : IOtpService
    {
        private readonly IUserRepository _userRepository;
        private readonly DiamondHealthContext _dbContext;

        public OtpService(IUserRepository userRepository, DiamondHealthContext dbContext)
        {
            _userRepository = userRepository;
            _dbContext = dbContext;
        }

        public async Task<string> GenerateOtpAsync(string phone, string purpose, CancellationToken cancellationToken = default)
        {
            // Generate 6-digit OTP
            var random = new Random();
            var otpCode = random.Next(100000, 999999).ToString();

            // Create OTP record
            var otpRecord = new OtpVerification
            {
                Phone = phone,
                OtpCode = otpCode,
                Purpose = purpose,
                ExpiresAt = DateTime.UtcNow.AddMinutes(5), // OTP expires in 5 minutes
                IsUsed = false,
                CreatedAt = DateTime.UtcNow
            };

            // Store OTP in database
            await _dbContext.OtpVerifications.AddAsync(otpRecord, cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);

            // In production, you would integrate with an SMS service to send the OTP
            return otpCode;
        }

        public async Task<bool> VerifyOtpAsync(string phone, string otpCode, string purpose, CancellationToken cancellationToken = default)
        {
            // Find the OTP record
            var otpRecord = await _dbContext.OtpVerifications
                .FirstOrDefaultAsync(o => o.Phone == phone 
                    && o.OtpCode == otpCode 
                    && o.Purpose == purpose 
                    && !o.IsUsed 
                    && o.ExpiresAt > DateTime.UtcNow, 
                    cancellationToken);

            if (otpRecord == null)
            {
                return false;
            }

            // Mark as used
            otpRecord.IsUsed = true;
            await _dbContext.SaveChangesAsync(cancellationToken);

            return true;
        }

        public async Task<bool> IsOtpValidAsync(string phone, string otpCode, string purpose, CancellationToken cancellationToken = default)
        {
            // Check if OTP exists and is valid (not used and not expired)
            var otpRecord = await _dbContext.OtpVerifications
                .FirstOrDefaultAsync(o => o.Phone == phone 
                    && o.OtpCode == otpCode 
                    && o.Purpose == purpose 
                    && !o.IsUsed 
                    && o.ExpiresAt > DateTime.UtcNow, 
                    cancellationToken);

            return otpRecord != null;
        }
    }
}
