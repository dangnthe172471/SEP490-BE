using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Net.payOS;
using Net.payOS.Types;
using SEP490_BE.BLL.IServices.IPaymentServices;
using SEP490_BE.BLL.Services.PaymentServices;
using SEP490_BE.DAL.DTOs.PaymentDTO;
using SEP490_BE.DAL.Models;

namespace SEP490_BE.API.Controllers.PaymentControllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PaymentsController : ControllerBase
    {
        private readonly IPayOSService _payOSService;
        private readonly IPaymentService _paymentService;

        public PaymentsController(IPayOSService payOSService, IPaymentService paymentService)
        {
            _payOSService = payOSService;
            _paymentService = paymentService;
        }



        [HttpGet("record/{recordId}")]
        public async Task<IActionResult> GetPaymentDetails(int recordId)
        {
            var items = await _paymentService.GetServicesForRecordAsync(recordId);
            var total = items.Sum(x => x.Total);

            return Ok(new { recordId, totalAmount = total, items });
        }

        [HttpPost("create")]
        public async Task<IActionResult> CreatePayment([FromBody] CreatePaymentRequestDTO dto)
        {
            try
            {
                var result = await _paymentService.CreatePaymentAsync(dto);

                return Ok(new
                {
                    paymentId = result.PaymentId,
                    checkoutUrl = result.CheckoutUrl
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        //[HttpPost("callback")]
        //public async Task<IActionResult> Callback([FromBody] PayOSCallbackDTO callback)
        //{
        //    await _paymentService.UpdatePaymentStatusAsync(callback);
        //    return Ok();
        //}
        [HttpPost("callback")]
        public async Task<IActionResult> Callback([FromBody] WebhookType webhook)
        {
            WebhookData result;

            try
            {
                result = _payOSService.VerifyWebhook(webhook);
            }
            catch
            {
                return BadRequest("Invalid signature");
            }

            long orderCode = result.orderCode;

            string status = result.code switch
            {
                "00" => "PAID",
                "01" => "PENDING",
                "09" => "FAILED",
                _ => "UNKNOWN"
            };

            await _paymentService.UpdatePaymentStatusAsync(orderCode, status);

            return Ok();
        }

        [HttpGet("status/{recordId}")]
        public async Task<IActionResult> GetPaymentStatus(int recordId)
        {
            var result = await _paymentService.GetPaymentStatusAsync(recordId);
            return Ok(result);
        }

        [HttpGet("payments-chart")]
        public async Task<IActionResult> GetPayments([FromQuery] DateTime start, [FromQuery] DateTime end)
        {
            var data = await _paymentService.GetPaymentsForChartAsync(start, end);
            return Ok(data);
        }

    }
}
