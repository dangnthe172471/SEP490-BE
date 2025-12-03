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
            if (recordId <= 0)
            {
                return BadRequest("recordId must be greater than 0");
            }
            var items = await _paymentService.GetServicesForRecordAsync(recordId);
            if (items == null || !items.Any())
            {
                return NotFound($"No services found for record {recordId}");
            }
            var total = items.Sum(x => x.Total);

            return Ok(new { recordId, totalAmount = total, items });
        }

        [HttpPost("create")]
        public async Task<IActionResult> CreatePayment([FromBody] CreatePaymentRequestDTO dto)
        {
            if (dto == null)
            {
                return BadRequest(new { message = "Dữ liệu thanh toán là bắt buộc." });
            }

            if (dto.MedicalRecordId <= 0)
            {
                return BadRequest(new { message = "Mã hồ sơ y tế không hợp lệ." });
            }

            if (dto.Amount <= 0)
            {
                return BadRequest(new { message = "Số tiền thanh toán phải lớn hơn 0." });
            }
            try
            {
                var result = await _paymentService.CreatePaymentAsync(dto);
                if (result == null)
                {
                    return StatusCode(StatusCodes.Status500InternalServerError,
                        new { message = "Không tạo được giao dịch thanh toán." });
                }
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
            if (recordId <= 0)
            {
                return BadRequest(new { message = "RecordId must be greater than 0." });
            }

            try
            {
                var result = await _paymentService.GetPaymentStatusAsync(recordId);
                if (result == null)
                {
                    return NotFound(new { message = "Không tìm thấy thông tin thanh toán cho hồ sơ này." });
                }
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new
                {
                    message = "Có lỗi xảy ra khi lấy trạng thái thanh toán.",
                    detail = ex.Message
                });
            }
        }

        [HttpGet("payments-chart")]
        public async Task<IActionResult> GetPayments([FromQuery] DateTime start, [FromQuery] DateTime end)
        {
            if (start > end)
            {
                return BadRequest(new { message = "Ngày bắt đầu phải nhỏ hơn hoặc bằng ngày kết thúc." });
            }

            try
            {
                var data = await _paymentService.GetPaymentsForChartAsync(start, end);
            return Ok(data);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new
                {
                    message = "Có lỗi xảy ra khi lấy dữ liệu thanh toán cho biểu đồ.",
                    detail = ex.Message
                });
            }
        }

    }
}
