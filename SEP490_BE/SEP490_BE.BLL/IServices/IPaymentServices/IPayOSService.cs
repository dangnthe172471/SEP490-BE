using Net.payOS.Types;
using SEP490_BE.DAL.DTOs.PaymentDTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SEP490_BE.BLL.IServices.IPaymentServices
{
    public interface IPayOSService
    {
        Task<string> CreatePaymentLinkAsync(long orderCode, CreatePaymentRequestDTO dto, List<ItemData> items);
        WebhookData VerifyWebhook(WebhookType webhook);
        Task<bool> IsPaymentLinkActive(long orderCode);
    }
}
