namespace SEP490_BE.BLL.IServices
{
    public interface IOtpService
    {
        Task<string> GenerateOtpAsync(string phone, string purpose, CancellationToken cancellationToken = default);
        Task<bool> VerifyOtpAsync(string phone, string otpCode, string purpose, CancellationToken cancellationToken = default);
        Task<bool> IsOtpValidAsync(string phone, string otpCode, string purpose, CancellationToken cancellationToken = default);
    }
}
