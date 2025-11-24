using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SEP490_BE.BLL.IServices;
using SEP490_BE.DAL.DTOs;
using System.Security.Claims;

namespace SEP490_BE.API.Controllers.ReceptionistControllers
{
    [Route("api/ReceptionistReappointmentRequests")]
    [ApiController]
    [Authorize(Roles = "Receptionist")]
    public class ReappointmentRequestController : ControllerBase
    {
        private readonly IReappointmentRequestService _reappointmentRequestService;

        public ReappointmentRequestController(IReappointmentRequestService reappointmentRequestService)
        {
            _reappointmentRequestService = reappointmentRequestService;
        }

        [HttpGet("pending")]
        public async Task<ActionResult<PagedResponse<ReappointmentRequestDto>>> GetPendingReappointmentRequests(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? searchTerm = null,
            [FromQuery] string sortBy = "createdDate",
            [FromQuery] string sortDirection = "desc",
            CancellationToken cancellationToken = default)
        {
            try
            {
                var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");
                if (userId == 0)
                {
                    return Unauthorized(new { message = "Không tìm thấy thông tin người dùng." });
                }

                var requests = await _reappointmentRequestService.GetPendingReappointmentRequestsAsync(
                    userId,
                    pageNumber,
                    pageSize,
                    searchTerm,
                    sortBy,
                    sortDirection,
                    cancellationToken);
                return Ok(requests);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("{notificationId}")]
        public async Task<ActionResult<ReappointmentRequestDto>> GetReappointmentRequestById(
            int notificationId,
            CancellationToken cancellationToken)
        {
            try
            {
                var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");
                if (userId == 0)
                {
                    return Unauthorized(new { message = "Không tìm thấy thông tin người dùng." });
                }

                var request = await _reappointmentRequestService.GetReappointmentRequestByIdAsync(notificationId, userId, cancellationToken);
                if (request == null)
                {
                    return NotFound(new { message = "Không tìm thấy yêu cầu tái khám." });
                }

                return Ok(request);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("complete")]
        public async Task<ActionResult<int>> CompleteReappointmentRequest(
            [FromBody] CompleteReappointmentRequestDto request,
            CancellationToken cancellationToken)
        {
            try
            {
                var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");
                if (userId == 0)
                {
                    return Unauthorized(new { message = "Không tìm thấy thông tin người dùng." });
                }

                var appointmentId = await _reappointmentRequestService.CompleteReappointmentRequestAsync(request, userId, cancellationToken);
                return Ok(new { appointmentId, message = "Đã tạo lịch hẹn tái khám thành công." });
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
    }
}

