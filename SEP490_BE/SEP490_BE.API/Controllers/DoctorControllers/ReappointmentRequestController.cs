using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SEP490_BE.BLL.IServices;
using SEP490_BE.DAL.DTOs;
using System.Security.Claims;

namespace SEP490_BE.API.Controllers.DoctorControllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Doctor")]
    public class ReappointmentRequestController : ControllerBase
    {
        private readonly IReappointmentRequestService _reappointmentRequestService;

        public ReappointmentRequestController(IReappointmentRequestService reappointmentRequestService)
        {
            _reappointmentRequestService = reappointmentRequestService;
        }

        [HttpPost]
        public async Task<ActionResult<int>> CreateReappointmentRequest(
            [FromBody] CreateReappointmentRequestDto request,
            CancellationToken cancellationToken)
        {
            try
            {
                var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");
                if (userId == 0)
                {
                    return Unauthorized(new { message = "Không tìm thấy thông tin người dùng." });
                }

                var notificationId = await _reappointmentRequestService.CreateReappointmentRequestAsync(request, userId, cancellationToken);
                return Ok(new { notificationId, message = "Đã gửi yêu cầu tái khám thành công." });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Forbid(ex.Message);
            }
        }

        [HttpGet("my-requests")]
        public async Task<ActionResult<List<ReappointmentRequestDto>>> GetMyReappointmentRequests(
            CancellationToken cancellationToken)
        {
            try
            {
                var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");
                if (userId == 0)
                {
                    return Unauthorized(new { message = "Không tìm thấy thông tin người dùng." });
                }

                var requests = await _reappointmentRequestService.GetMyReappointmentRequestsAsync(userId, cancellationToken);
                return Ok(requests);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}

