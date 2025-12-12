using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SEP490_BE.BLL.IServices;
using SEP490_BE.BLL.Services;
using SEP490_BE.DAL.DTOs;
using System.Security.Claims;

namespace SEP490_BE.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AdministratorController : ControllerBase
    {
        private readonly IAdministratorService _administratorService;
        private readonly IRoomService _roomService;

        public AdministratorController(IAdministratorService administratorService, IRoomService roomService)
        {
            _administratorService = administratorService;
            _roomService = roomService;
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<UserDto>>> GetAll(CancellationToken cancellationToken)
        {
            var users = await _administratorService.GetAllAsync(cancellationToken);
            return Ok(users);
        }

        [HttpPost]
        [Authorize(Roles = "Administrator")]
        public async Task<ActionResult<UserDto>> CreateUser([FromBody] CreateUserRequest request, CancellationToken cancellationToken)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var user = await _administratorService.CreateUserAsync(request, cancellationToken);
                if (user == null)
                {
                    return BadRequest("Không thể tạo người dùng mới.");
                }

                return CreatedAtAction(nameof(GetById), new { id = user.UserId }, user);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Đã xảy ra lỗi khi tạo người dùng.", error = ex.Message });
            }
        }

        [HttpGet("list-rooms")]
        [Authorize(Roles = "Administrator")]
        public async Task<ActionResult<IEnumerable<RoomDto>>> GetAllRooms(CancellationToken cancellationToken)
        {
            try
            {
                var rooms = await _roomService.GetAllAsync(cancellationToken);
                return Ok(rooms);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Đã xảy ra lỗi khi lấy danh sách phòng.", error = ex.Message });
            }
        }

        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<ActionResult<UserDto>> GetById(int id, CancellationToken cancellationToken)
        {
            var user = await _administratorService.GetByIdAsync(id, cancellationToken);
            if (user == null)
            {
                return NotFound();
            }
            return Ok(user);
        }

        [HttpPut("{id}")]
        [Authorize]
        public async Task<ActionResult<UserDto>> UpdateUser(int id, [FromBody] UpdateUserRequest request, CancellationToken cancellationToken)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                // Check authorization: user can update themselves or admin can update anyone
                var currentUserIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var isAdmin = User.IsInRole("Administrator");

                if (!isAdmin && (!int.TryParse(currentUserIdClaim, out var currentUserId) || currentUserId != id))
                {
                    return StatusCode(403, new { message = "Bạn chỉ có thể cập nhật thông tin của chính mình." });
                }

                var user = await _administratorService.UpdateUserAsync(id, request, cancellationToken);
                if (user == null)
                {
                    return NotFound(new { message = "Không tìm thấy người dùng." });
                }

                return Ok(user);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Đã xảy ra lỗi khi cập nhật người dùng.", error = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Administrator")]
        public async Task<ActionResult> DeleteUser(int id, CancellationToken cancellationToken)
        {
            try
            {
                var result = await _administratorService.DeleteUserAsync(id, cancellationToken);
                if (!result)
                {
                    return NotFound(new { message = "Không tìm thấy người dùng để xóa." });
                }

                return NoContent();
            }
            catch (InvalidOperationException ex)
            {
                // Business rule / FK dependency prevents deletion
                return StatusCode(409, new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Đã xảy ra lỗi khi xóa người dùng.", error = ex.Message });
            }
        }

        [HttpPatch("{id}/toggle-status")]
        [Authorize(Roles = "Administrator")]
        public async Task<ActionResult<UserDto>> ToggleUserStatus(int id, CancellationToken cancellationToken)
        {
            try
            {
                var result = await _administratorService.ToggleUserStatusAsync(id, cancellationToken);
                if (!result)
                {
                    return NotFound(new { message = "Không tìm thấy người dùng." });
                }

                var updatedUser = await _administratorService.GetByIdAsync(id, cancellationToken);
                return Ok(updatedUser);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Đã xảy ra lỗi khi thay đổi trạng thái người dùng.", error = ex.Message });
            }
        }

        [HttpPost("search")]
        [Authorize(Roles = "Administrator")]
        public async Task<ActionResult<SearchUserResponse>> SearchUsers([FromBody] SearchUserRequest request, CancellationToken cancellationToken)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var result = await _administratorService.SearchUsersAsync(request, cancellationToken);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Đã xảy ra lỗi khi tìm kiếm người dùng.", error = ex.Message });
            }
        }

        [HttpGet("patients")]
        [Authorize(Roles = "Administrator")]
        public async Task<ActionResult<IEnumerable<UserDto>>> GetAllPatients(CancellationToken cancellationToken)
        {
            try
            {
                var patients = await _administratorService.GetAllPatientsAsync(cancellationToken);
                return Ok(patients);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Đã xảy ra lỗi khi lấy danh sách bệnh nhân.", error = ex.Message });
            }
        }
    }
}
