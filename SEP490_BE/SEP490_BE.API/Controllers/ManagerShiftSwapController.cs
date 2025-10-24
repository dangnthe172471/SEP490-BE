using Microsoft.AspNetCore.Mvc;
using SEP490_BE.BLL.IServices;
using SEP490_BE.DAL.DTOs;

namespace SEP490_BE.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ManagerShiftSwapController : ControllerBase
    {
        private readonly IDoctorShiftExchangeService _shiftExchangeService;

        public ManagerShiftSwapController(IDoctorShiftExchangeService shiftExchangeService)
        {
            _shiftExchangeService = shiftExchangeService;
        }


        /// <summary>
        /// Lấy tất cả yêu cầu đổi ca (chờ duyệt, đã chấp nhận, đã từ chối)
        /// </summary>
        [HttpGet("all-requests")]
        public async Task<IActionResult> GetAllRequests()
        {
            try
            {
                var requests = await _shiftExchangeService.GetAllRequestsAsync();
                return Ok(new { success = true, data = requests });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Manager chấp nhận hoặc từ chối yêu cầu đổi ca
        /// </summary>
        [HttpPost("review-request")]
        public async Task<IActionResult> ReviewShiftSwapRequest([FromBody] ReviewShiftSwapRequestDTO review)
        {
            try
            {
                var result = await _shiftExchangeService.ReviewShiftSwapRequestAsync(review);
                
                if (result)
                {
                    var statusText = review.Status == "Approved" ? "chấp nhận" : "từ chối";
                    return Ok(new { 
                        success = true, 
                        message = $"Đã {statusText} yêu cầu đổi ca thành công" 
                    });
                }
                else
                {
                    return BadRequest(new { 
                        success = false, 
                        message = "Không thể cập nhật trạng thái yêu cầu" 
                    });
                }
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Lỗi hệ thống: " + ex.Message });
            }
        }

        /// <summary>
        /// Lấy chi tiết yêu cầu đổi ca
        /// </summary>
        [HttpGet("request/{exchangeId}")]
        public async Task<IActionResult> GetRequestDetails(int exchangeId)
        {
            try
            {
                var request = await _shiftExchangeService.GetRequestByIdAsync(exchangeId);
                
                if (request == null)
                {
                    return NotFound(new { success = false, message = "Không tìm thấy yêu cầu đổi ca" });
                }

                return Ok(new { success = true, data = request });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }
    }
}
