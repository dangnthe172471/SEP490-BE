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

            var paymentData = new PaymentData(
                orderCode,
                dto.Amount,
                dto.Description,
                items,
                _config["PayOS:CancelUrl"],
                _config["PayOS:ReturnUrl"]
            );

            var result = await _payOS.createPaymentLink(paymentData);
            return result.checkoutUrl;
        }

        private string Shorten(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return "";

            input = input.Trim();
            return input.Length > 25 ? input.Substring(0, 25) : input;
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
