using SEP490_BE.BLL.IServices;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SEP490_BE.BLL.Services
{
    public class ResetTokenService : IResetTokenService
    {
        private static readonly ConcurrentDictionary<string, string> _tokenStorage = new();
        private static readonly ConcurrentDictionary<string, OtpInfo> _otpStorage = new();

        public Task StoreOtpAsync(string email, string otp, TimeSpan expire)
        {
            _otpStorage[email] = new OtpInfo
            {
                OtpCode = otp,
                Expiry = DateTime.UtcNow.Add(expire)
            };
            return Task.CompletedTask;
        }

        public async Task<string> GenerateOtpAsync(string email)
        {
            var otpCode = new Random().Next(100000, 999999).ToString();
            var expiry = DateTime.UtcNow.AddMinutes(5);

            _otpStorage[email] = new OtpInfo
            {
                OtpCode = otpCode,
                Expiry = expiry
            };

            return await Task.FromResult(otpCode);
        }

        public async Task<bool> ValidateTokenAsync(string email, string token)
        {
            if (_tokenStorage.TryGetValue(email, out var storedToken))
            {
                bool isValid = storedToken == token;
                if (isValid)
                {
                    _tokenStorage.TryRemove(email, out _); // Xóa để token 1 lần dùng
                }
                return await Task.FromResult(isValid);
            }
            return await Task.FromResult(false);
        }
        public async Task<bool> ValidateOtpAsync(string email, string otpCode)
        {
            if (_otpStorage.TryGetValue(email, out var otpInfo))
            {
                // Kiểm tra còn hạn và mã đúng không
                if (otpInfo.Expiry > DateTime.UtcNow && otpInfo.OtpCode == otpCode)
                {
                    _otpStorage.TryRemove(email, out _); // Xóa sau khi dùng
                    return await Task.FromResult(true);
                }
            }

            return await Task.FromResult(false);
        }
        public async Task<string> GenerateAndStoreTokenAsync(string email)
        {
            string token = Guid.NewGuid().ToString();

            // Lưu token lại cho email tương ứng
            _tokenStorage[email] = token;

            await Task.CompletedTask;
            return token;
        }
    }

    public class OtpInfo
    {
        public string OtpCode { get; set; }
        public DateTime Expiry { get; set; }
    }



}
