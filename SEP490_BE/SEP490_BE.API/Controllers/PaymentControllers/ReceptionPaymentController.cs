using Microsoft.AspNetCore.Mvc;
using SEP490_BE.BLL.IServices.IPaymentServices;
using SEP490_BE.DAL.DTOs.PaymentDTO;
using System;
using System.Web;
using Twilio.TwiML.Voice;

namespace SEP490_BE.API.Controllers.PaymentControllers
{
    [Route("1/[controller]")]
    [ApiController]
    public class ReceptionPaymentController : ControllerBase
    {
        private readonly IPaymentService _paymentService;
        private readonly IConfiguration _config;
        public ReceptionPaymentController(IPaymentService paymentService, IConfiguration config)
        {
            _paymentService = paymentService;
            _config = config;
        }


        [HttpPost("generate-qr")]
        public IActionResult GenerateQr([FromBody] GenerateQrDto dto)
        {
            try
            {
                // Truyền config trực tiếp vào service
                var result = _paymentService.GenerateQrLink(dto, _config);
                return Ok(result);
            }
            catch (System.Exception ex)
            {
                return StatusCode(500, $"Không thể tạo QR code: {ex.Message}");
            }
        }



    }
}
