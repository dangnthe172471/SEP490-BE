using Microsoft.Extensions.Configuration;
using SEP490_BE.DAL.DTOs.PaymentDTO;
using Net.payOS;
using Net.payOS.Types;
using System.Linq;
using System.Threading.Tasks;
using SEP490_BE.BLL.IServices.IPaymentServices;

namespace SEP490_BE.BLL.Services.PaymentServices
{


    public class PayOSService : IPayOSService
    {
        private readonly PayOS _payOS;
        private readonly IConfiguration _config;

        public PayOSService(IConfiguration config)
        {
            _config = config;

            _payOS = new PayOS(
                _config["PayOS:ClientId"],
                _config["PayOS:ApiKey"],
                _config["PayOS:ChecksumKey"],
                _config["PayOS:PartnerCode"] // ko dùng thì để "" hoặc bỏ overload
            );
        }

        public async Task<string> CreatePaymentLinkAsync(long orderCode, CreatePaymentRequestDTO dto, List<ItemData> items)
        {
            dto.Description = Shorten(dto.Description);

            var cancelUrl = _config["PayOS:CancelUrl"];
            var returnUrl = _config["PayOS:ReturnUrl"];

            // Đảm bảo URL luôn dùng domain, không dùng IP
            cancelUrl = NormalizeUrl(cancelUrl);
            returnUrl = NormalizeUrl(returnUrl);

            Console.WriteLine($"[PayOS] Creating payment link - CancelUrl: {cancelUrl}, ReturnUrl: {returnUrl}");

            var paymentData = new PaymentData(
                orderCode,
                dto.Amount,
                dto.Description,
                items,
                cancelUrl,
                returnUrl
            );

            var result = await _payOS.createPaymentLink(paymentData);
            Console.WriteLine($"[PayOS] Created checkout URL: {result.checkoutUrl}");
            return result.checkoutUrl;
        }
        public async Task<string> CreatePaymentReceptionistAsync(long orderCode, CreatePaymentRequestDTO dto, List<ItemData> items)
        {
            dto.Description = Shorten(dto.Description);

            var cancelUrl = _config["PayOS:CancelReceptionistUrl"];
            var returnUrl = _config["PayOS:ReturnReceptionistUrl"];

            if (string.IsNullOrWhiteSpace(cancelUrl))
            {
                cancelUrl = "https://diamondhealth.io.vn/reception/records";
            }
            if (string.IsNullOrWhiteSpace(returnUrl))
            {
                returnUrl = "https://diamondhealth.io.vn/reception/records";
            }

        
            cancelUrl = NormalizeUrl(cancelUrl);
            returnUrl = NormalizeUrl(returnUrl);

            if (string.IsNullOrWhiteSpace(cancelUrl))
            {
                cancelUrl = "https://diamondhealth.io.vn/reception/records";
            }
            if (string.IsNullOrWhiteSpace(returnUrl))
            {
                returnUrl = "https://diamondhealth.io.vn/reception/records";
            }

     

            var paymentData = new PaymentData(
                orderCode,
                dto.Amount,
                dto.Description,
                items,
                cancelUrl,
                returnUrl
            );

            var result = await _payOS.createPaymentLink(paymentData);
            Console.WriteLine($"[PayOS] Created checkout URL: {result.checkoutUrl}");
            return result.checkoutUrl;
        }

        private string Shorten(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return "";

            input = input.Trim();
            return input.Length > 25 ? input.Substring(0, 25) : input;
        }

      
        private string NormalizeUrl(string url)
        {
            if (string.IsNullOrWhiteSpace(url))
                return url;

            // Thay thế IP bằng domain nếu có
            url = url.Replace("http://103.200.22.75", "https://diamondhealth.io.vn");
            url = url.Replace("https://103.200.22.75", "https://diamondhealth.io.vn");
            
            // Đảm bảo dùng HTTPS
            if (url.StartsWith("http://diamondhealth.io.vn"))
            {
                url = url.Replace("http://", "https://");
            }

            return url;
        }
        public WebhookData VerifyWebhook(WebhookType webhook)
        {
            return _payOS.verifyPaymentWebhookData(webhook);
        }

        public async Task<bool> IsPaymentLinkActive(long orderCode)
        {
            try
            {
                var info = await _payOS.getPaymentLinkInformation(orderCode);

                // INIT : link còn tồn tại và chưa thanh toán
                return info.status == "INIT";
            }
            catch (Exception)
            {
                return false;
            }
        }

    }
}
