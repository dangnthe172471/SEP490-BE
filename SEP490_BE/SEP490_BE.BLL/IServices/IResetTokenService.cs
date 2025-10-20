using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SEP490_BE.BLL.IServices
{
    public interface IResetTokenService
    {
        Task StoreOtpAsync(string email, string otpCode, TimeSpan expiry);
        public Task<string> GenerateOtpAsync(string email);
        public Task<bool> ValidateOtpAsync(string email, string otpCode);
        public Task<string> GenerateAndStoreTokenAsync(string email);
        public Task<bool> ValidateTokenAsync(string email, string token); // 👈 Bổ sung dòng này
    }
}
